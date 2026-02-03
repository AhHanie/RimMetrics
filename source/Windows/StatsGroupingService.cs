using System;
using System.Collections.Generic;
using RimMetrics.CalculatedStats;
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

        public static List<CategoryGroup> BuildGroupedGameStats(GameComponent_GameStats gameStats)
        {
            var results = new List<CategoryGroup>();
            if (gameStats == null)
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
                if (meta.StatType != StatType.GAME)
                {
                    continue;
                }

                if (meta.HasKey)
                {
                    AddKeyedGameStats(gameStats, meta, byCategory, results);
                }
                else
                {
                    AddSingleGameStat(meta, byCategory, results);
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
            if (!TryGetKeyedPawnStats(comp, meta, out var keyedInt, out var keyedFloat))
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
            if (meta.StatValueType == StatValueType.Float)
            {
                foreach (var pair in keyedFloat)
                {
                    var key = pair.Key;
                    var totalFloat = pair.Value;
                    if (IsZeroValue(meta.StatValueType, 0, totalFloat))
                    {
                        continue;
                    }

                    var label = StatStringHelper.ToKeyedString(meta.TypeId, key);
                    var value = StatValueFormatter.FormatValue(meta.StatValueType, 0, totalFloat);
                    var iconData = GetIconData(meta, key);
                    rows.Add(new StatRow(meta.TypeId, key, label, value, meta.StatValueType, 0, totalFloat, iconData));
                }
            }
            else
            {
                foreach (var pair in keyedInt)
                {
                    var key = pair.Key;
                    var totalInt = pair.Value;
                    if (IsZeroValue(meta.StatValueType, totalInt, 0f))
                    {
                        continue;
                    }

                    var label = StatStringHelper.ToKeyedString(meta.TypeId, key);
                    var value = StatValueFormatter.FormatValue(meta.StatValueType, totalInt, 0f);
                    var iconData = GetIconData(meta, key);
                    rows.Add(new StatRow(meta.TypeId, key, label, value, meta.StatValueType, totalInt, 0f, iconData));
                }
            }

            if (rows.Count > 0)
            {
                var groupLabel = StatStringHelper.ToKeyedString(meta.TypeId);
                category.Groups[groupLabel] = rows;
            }
        }

        private static bool TryGetKeyedPawnStats(
            Comp_PawnStats comp,
            StatMeta meta,
            out Dictionary<string, int> keyedInt,
            out Dictionary<string, float> keyedFloat)
        {
            keyedInt = null;
            keyedFloat = null;

            if (meta.Source == StatSource.Manual)
            {
                if (!comp.TryGetKeyedStats(meta.TypeId, out var keyed) || keyed == null || keyed.Count == 0)
                {
                    return false;
                }

                if (meta.StatValueType == StatValueType.Float)
                {
                    keyedFloat = new Dictionary<string, float>(keyed.Count);
                    foreach (var pair in keyed)
                    {
                        keyedFloat[pair.Key] = pair.Value?.TotalFloat ?? 0f;
                    }
                }
                else
                {
                    keyedInt = new Dictionary<string, int>(keyed.Count);
                    foreach (var pair in keyed)
                    {
                        keyedInt[pair.Key] = pair.Value?.TotalInt ?? 0;
                    }
                }

                return true;
            }

            if (meta.Source == StatSource.CalculatedStat && meta.CalculatorType != null)
            {
                var provider = CalculatedStatProviderCache.GetOrCreate(meta.CalculatorType) as ICalculatedKeyedStatProvider;
                if (provider == null)
                {
                    return false;
                }

                if (meta.StatValueType == StatValueType.Float)
                {
                    keyedFloat = provider.CalculateKeyedFloatTotals(meta.TypeId, comp);
                }
                else
                {
                    keyedInt = provider.CalculateKeyedIntTotals(meta.TypeId, comp);
                }

                return keyedInt != null || keyedFloat != null;
            }

            return false;
        }

        private static void AddKeyedGameStats(
            GameComponent_GameStats gameStats,
            StatMeta meta,
            Dictionary<string, CategoryGroup> byCategory,
            List<CategoryGroup> results)
        {
            if (!TryGetKeyedGameTotals(gameStats, meta, out var keyedInt, out var keyedFloat))
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
            if (meta.StatValueType == StatValueType.Float)
            {
                foreach (var pair in keyedFloat)
                {
                    var key = pair.Key;
                    var totalFloat = pair.Value;
                    if (IsZeroValue(meta.StatValueType, 0, totalFloat))
                    {
                        continue;
                    }

                    var label = StatStringHelper.ToKeyedString(meta.TypeId, key);
                    var value = StatValueFormatter.FormatValue(meta.StatValueType, 0, totalFloat);
                    var iconData = GetIconData(meta, key);
                    rows.Add(new StatRow(meta.TypeId, key, label, value, meta.StatValueType, 0, totalFloat, iconData));
                }
            }
            else
            {
                foreach (var pair in keyedInt)
                {
                    var key = pair.Key;
                    var totalInt = pair.Value;
                    if (IsZeroValue(meta.StatValueType, totalInt, 0f))
                    {
                        continue;
                    }

                    var label = StatStringHelper.ToKeyedString(meta.TypeId, key);
                    var value = StatValueFormatter.FormatValue(meta.StatValueType, totalInt, 0f);
                    var iconData = GetIconData(meta, key);
                    rows.Add(new StatRow(meta.TypeId, key, label, value, meta.StatValueType, totalInt, 0f, iconData));
                }
            }

            if (rows.Count > 0)
            {
                var groupLabel = StatStringHelper.ToKeyedString(meta.TypeId);
                category.Groups[groupLabel] = rows;
            }
        }

        private static bool TryGetKeyedGameTotals(
            GameComponent_GameStats gameStats,
            StatMeta meta,
            out Dictionary<string, int> keyedInt,
            out Dictionary<string, float> keyedFloat)
        {
            keyedInt = null;
            keyedFloat = null;

            if (meta.Source == StatSource.Manual)
            {
                if (!gameStats.TryGetKeyedStats(meta.TypeId, out var keyed) || keyed == null || keyed.Count == 0)
                {
                    return false;
                }

                if (meta.StatValueType == StatValueType.Float)
                {
                    keyedFloat = new Dictionary<string, float>(keyed.Count);
                    foreach (var pair in keyed)
                    {
                        keyedFloat[pair.Key] = pair.Value?.TotalFloat ?? 0f;
                    }
                }
                else
                {
                    keyedInt = new Dictionary<string, int>(keyed.Count);
                    foreach (var pair in keyed)
                    {
                        keyedInt[pair.Key] = pair.Value?.TotalInt ?? 0;
                    }
                }

                return true;
            }

            if (meta.Source == StatSource.CalculatedStat && meta.CalculatorType != null)
            {
                var keyedProvider = CalculatedStatProviderCache.GetOrCreate(meta.CalculatorType) as ICalculatedKeyedStatProvider;
                if (keyedProvider != null)
                {
                    if (meta.StatValueType == StatValueType.Float)
                    {
                        keyedFloat = keyedProvider.CalculateKeyedFloatTotals(meta.TypeId);
                    }
                    else
                    {
                        keyedInt = keyedProvider.CalculateKeyedIntTotals(meta.TypeId);
                    }

                    return keyedInt != null || keyedFloat != null;
                }

                var provider = CalculatedStatProviderCache.GetOrCreate(meta.CalculatorType) as ColonistManualKeyedTotalStatProvider;
                if (provider == null)
                {
                    return false;
                }

                var sourceStatId = GetSourceStatId(meta.TypeId);
                if (meta.StatValueType == StatValueType.Float)
                {
                    keyedFloat = provider.CalculateKeyedFloatTotals(sourceStatId);
                }
                else
                {
                    keyedInt = provider.CalculateKeyedIntTotals(sourceStatId);
                }

                return keyedInt != null || keyedFloat != null;
            }

            return false;
        }

        private static string GetSourceStatId(string gameStatId)
        {
            if (string.IsNullOrWhiteSpace(gameStatId))
            {
                return gameStatId;
            }

            const string gamePrefix = "GAME_";
            const string pawnPrefix = "PAWN_";
            if (gameStatId.StartsWith(gamePrefix, StringComparison.Ordinal))
            {
                return pawnPrefix + gameStatId.Substring(gamePrefix.Length);
            }

            return gameStatId;
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

        private static void AddSingleGameStat(
            StatMeta meta,
            Dictionary<string, CategoryGroup> byCategory,
            List<CategoryGroup> results)
        {
            if (!StatValueFetcher.TryGetValue(meta.TypeId, null, out var intValue, out var floatValue, out var valueType))
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
