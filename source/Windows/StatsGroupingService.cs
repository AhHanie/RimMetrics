using System;
using System.Collections.Generic;
using RimMetrics.Components;
using RimMetrics.Helpers;
using Verse;

namespace RimMetrics
{
    public static class StatsGroupingService
    {
        public sealed class StatRow : IExposable
        {
            public string TypeId;
            public string Key;
            public string Label;
            public string Value;
            public bool HasValue;
            public StatValueType ValueType;
            public int TotalInt;
            public float TotalFloat;
            public StatIconData IconData;

            public StatRow()
            {
            }

            public StatRow(
                string typeId,
                string key,
                string label,
                string value,
                StatValueType valueType,
                int totalInt,
                float totalFloat,
                StatIconData iconData)
            {
                TypeId = typeId ?? string.Empty;
                Key = key ?? string.Empty;
                Label = label;
                Value = value;
                HasValue = true;
                ValueType = valueType;
                TotalInt = totalInt;
                TotalFloat = totalFloat;
                IconData = iconData;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref TypeId, "typeId");
                Scribe_Values.Look(ref Key, "key");
                Scribe_Values.Look(ref Label, "label");
                Scribe_Values.Look(ref Value, "value");
                Scribe_Values.Look(ref HasValue, "hasValue", false);
                Scribe_Values.Look(ref ValueType, "valueType", StatValueType.Int);
                Scribe_Values.Look(ref TotalInt, "totalInt", 0);
                Scribe_Values.Look(ref TotalFloat, "totalFloat", 0f);
            }
        }

        public sealed class CategoryGroup : IExposable
        {
            public string Category;
            public List<StatRow> Rows = new List<StatRow>();
            public Dictionary<string, List<StatRow>> Groups = new Dictionary<string, List<StatRow>>();

            public CategoryGroup()
            {
            }

            public CategoryGroup(string category)
            {
                Category = category ?? string.Empty;
            }

            public void ExposeData()
            {
                Scribe_Values.Look(ref Category, "category");
                Scribe_Collections.Look(ref Rows, "rows", LookMode.Deep);
                Scribe_Collections.Look(ref Groups, "groups", LookMode.Value, LookMode.Deep);

                if (Rows == null)
                {
                    Rows = new List<StatRow>();
                }

                if (Groups == null)
                {
                    Groups = new Dictionary<string, List<StatRow>>();
                }
            }
        }

        public static List<CategoryGroup> BuildGroupedStats(Pawn pawn)
        {
            var results = new List<CategoryGroup>();
            if (pawn == null)
            {
                return results;
            }

            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return results;
            }

            var metas = new List<StatMeta>(StatRegistry.GetAllMetas());
            metas.Sort((a, b) =>
            {
                var categoryComparison = StatCategoryRegistry.Compare(a.Category, b.Category);
                if (categoryComparison != 0)
                {
                    return categoryComparison;
                }

                var orderComparison = a.DisplayOrder.CompareTo(b.DisplayOrder);
                if (orderComparison != 0)
                {
                    return orderComparison;
                }

                return string.CompareOrdinal(a.TypeId, b.TypeId);
            });

            var byCategory = new Dictionary<string, CategoryGroup>();
            foreach (var meta in metas)
            {
                if (meta.StatType != StatType.PAWN)
                {
                    continue;
                }

                if (meta.HasKey)
                {
                    AddKeyedStats(comp, meta, byCategory, results);
                }
                else
                {
                    AddSingleStat(meta, pawn, byCategory, results);
                }
            }

            results.RemoveAll(category => category.Rows.Count == 0 && category.Groups.Count == 0);
            return results;
        }

        public static bool CategoryMatchesSearch(CategoryGroup category, string search)
        {
            foreach (var row in category.Rows)
            {
                if (row.Label.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            foreach (var group in category.Groups)
            {
                foreach (var row in group.Value)
                {
                    if (row.Label.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool GroupMatchesSearch(List<StatRow> rows, string search)
        {
            foreach (var row in rows)
            {
                if (row.Label.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static int GetCategoryTotalCount(CategoryGroup category)
        {
            var total = category.Rows.Count;
            foreach (var group in category.Groups.Values)
            {
                total += group.Count;
            }

            return total;
        }

        public static void PopulateIconData(List<CategoryGroup> groupedStats)
        {
            if (groupedStats == null)
            {
                return;
            }

            foreach (var category in groupedStats)
            {
                PopulateIconData(category.Rows);
                foreach (var group in category.Groups.Values)
                {
                    PopulateIconData(group);
                }
            }
        }

        private static void PopulateIconData(List<StatRow> rows)
        {
            if (rows == null)
            {
                return;
            }

            foreach (var row in rows)
            {
                var meta = StatRegistry.GetMeta(row.TypeId);
                row.IconData = GetIconData(meta, row.Key);
            }
        }

        private static void AddKeyedStats(
            Comp_PawnStats comp,
            StatMeta meta,
            Dictionary<string, CategoryGroup> byCategory,
            List<CategoryGroup> results)
        {
            if (!comp.TryGetKeyedStats(meta.TypeId, out var keyed) || keyed == null || keyed.Count == 0)
            {
                return;
            }

            if (!byCategory.TryGetValue(meta.Category, out var category))
            {
                category = new CategoryGroup(meta.Category);
                byCategory[meta.Category] = category;
                results.Add(category);
            }

            var rows = new List<StatRow>();
            foreach (var pair in keyed)
            {
                var key = pair.Key;
                var record = pair.Value;
                if (record == null)
                {
                    continue;
                }

                if (IsZeroValue(meta.StatValueType, record.TotalInt, record.TotalFloat))
                {
                    continue;
                }

                var label = StatStringHelper.ToKeyedString(meta.TypeId, key);
                var value = StatValueFormatter.FormatValue(meta.StatValueType, record.TotalInt, record.TotalFloat);
                var iconData = GetIconData(meta, key);
                rows.Add(new StatRow(meta.TypeId, key, label, value, meta.StatValueType, record.TotalInt, record.TotalFloat, iconData));
            }

            if (rows.Count > 0)
            {
                var groupLabel = StatStringHelper.ToKeyedString(meta.TypeId);
                category.Groups[groupLabel] = rows;
            }
        }

        private static void AddSingleStat(
            StatMeta meta,
            Pawn pawn,
            Dictionary<string, CategoryGroup> byCategory,
            List<CategoryGroup> results)
        {
            if (!StatValueFetcher.TryGetValue(meta.TypeId, pawn, out var intValue, out var floatValue, out var valueType))
            {
                return;
            }

            if (IsZeroValue(valueType, intValue, floatValue))
            {
                return;
            }

            if (!byCategory.TryGetValue(meta.Category, out var category))
            {
                category = new CategoryGroup(meta.Category);
                byCategory[meta.Category] = category;
                results.Add(category);
            }

            var label = StatStringHelper.ToKeyedString(meta.TypeId);
            var value = StatValueFormatter.FormatValue(valueType, intValue, floatValue);
            var iconData = GetIconData(meta, string.Empty);
            category.Rows.Add(new StatRow(meta.TypeId, string.Empty, label, value, valueType, intValue, floatValue, iconData));
        }

        private static bool IsZeroValue(StatValueType valueType, int intValue, float floatValue)
        {
            return valueType == StatValueType.Float ? floatValue == 0f : intValue == 0;
        }

        private static StatIconData GetIconData(StatMeta meta, string key)
        {
            var selector = StatIconSelectorResolver.GetSelector(meta);
            if (selector == null)
            {
                return null;
            }

            return selector.TryGetIcon(meta, key, out var iconData) ? iconData : null;
        }
    }
}
