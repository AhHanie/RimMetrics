using System;
using System.Collections.Generic;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Helpers
{
    public static class ColonistTopStatCalculator
    {
        private const int MaxRank = 3;
        private static readonly ColonistStatValueComparer StatValueComparer = new ColonistStatValueComparer();

        public static List<ColonistTopStats> BuildTopStats()
        {
            var statsByType = new Dictionary<string, List<ColonistStatValue>>();
            var metas = new List<StatMeta>(StatRegistry.GetAllMetas());

            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                foreach (var meta in metas)
                {
                    if (meta.StatType != StatType.PAWN || meta.HasKey)
                    {
                        continue;
                    }

                    if (!StatValueFetcher.TryGetValue(meta.TypeId, colonist, out var intValue, out var floatValue, out var valueType))
                    {
                        continue;
                    }

                    if (IsZeroValue(valueType, intValue, floatValue))
                    {
                        continue;
                    }

                    var entry = new ColonistStatValue(
                        colonist,
                        meta.StatValueType,
                        intValue,
                        floatValue);

                    if (!statsByType.TryGetValue(meta.TypeId, out var statValues))
                    {
                        statValues = new List<ColonistStatValue>();
                        statsByType[meta.TypeId] = statValues;
                    }

                    statValues.Add(entry);
                }
            }

            var topStatsByPawn = new Dictionary<Pawn, ColonistTopStats>();
            foreach (var pair in statsByType)
            {
                var statId = pair.Key;
                var statValues = pair.Value;
                statValues.Sort(StatValueComparer);

                var max = Math.Min(MaxRank, statValues.Count);
                for (var index = 0; index < max; index++)
                {
                    var value = statValues[index];
                    if (!topStatsByPawn.TryGetValue(value.Pawn, out var summary))
                    {
                        summary = new ColonistTopStats(value.Pawn, new List<TopStatEntry>());
                        topStatsByPawn[value.Pawn] = summary;
                    }

                    summary.Add(new TopStatEntry(
                        statId,
                        index + 1,
                        value.ValueType,
                        value.TotalInt,
                        value.TotalFloat));
                }
            }

            return new List<ColonistTopStats>(topStatsByPawn.Values);
        }

        private readonly struct ColonistStatValue
        {
            public readonly Pawn Pawn;
            public readonly StatValueType ValueType;
            public readonly int TotalInt;
            public readonly float TotalFloat;

            public ColonistStatValue(Pawn pawn, StatValueType valueType, int totalInt, float totalFloat)
            {
                Pawn = pawn;
                ValueType = valueType;
                TotalInt = totalInt;
                TotalFloat = totalFloat;
            }

            public double Value => ValueType == StatValueType.Float ? TotalFloat : TotalInt;
        }

        private sealed class ColonistStatValueComparer : IComparer<ColonistStatValue>
        {
            public int Compare(ColonistStatValue x, ColonistStatValue y)
            {
                var valueComparison = y.Value.CompareTo(x.Value);
                if (valueComparison != 0)
                {
                    return valueComparison;
                }

                var xId = x.Pawn?.thingIDNumber ?? 0;
                var yId = y.Pawn?.thingIDNumber ?? 0;
                return xId.CompareTo(yId);
            }
        }

        private static bool IsZeroValue(StatValueType valueType, int intValue, float floatValue)
        {
            return valueType == StatValueType.Float ? floatValue == 0f : intValue == 0;
        }
    }
}
