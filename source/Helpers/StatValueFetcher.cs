using RimMetrics.CalculatedStats;
using RimMetrics.Components;
using Verse;

namespace RimMetrics.Helpers
{
    public static class StatValueFetcher
    {
        public static bool TryGetInt(string statId, Pawn pawn, out int value)
        {
            return TryGetInt(statId, pawn, string.Empty, out value);
        }

        public static bool TryGetFloat(string statId, Pawn pawn, out float value)
        {
            return TryGetFloat(statId, pawn, string.Empty, out value);
        }

        public static bool TryGetValue(
            string statId,
            Pawn pawn,
            out int intValue,
            out float floatValue,
            out StatValueType valueType)
        {
            return TryGetValue(statId, pawn, string.Empty, out intValue, out floatValue, out valueType);
        }

        public static bool TryGetInt(string statId, Pawn pawn, string key, out int value)
        {
            value = 0;
            if (!TryGetValue(statId, pawn, key, out var intValue, out var floatValue, out var valueType))
            {
                return false;
            }

            if (valueType != StatValueType.Int)
            {
                return false;
            }

            value = intValue;
            return true;
        }

        public static bool TryGetFloat(string statId, Pawn pawn, string key, out float value)
        {
            value = 0f;
            if (!TryGetValue(statId, pawn, key, out var intValue, out var floatValue, out var valueType))
            {
                return false;
            }

            if (valueType != StatValueType.Float)
            {
                return false;
            }

            value = floatValue;
            return true;
        }

        public static bool TryGetValue(
            string statId,
            Pawn pawn,
            string key,
            out int intValue,
            out float floatValue,
            out StatValueType valueType)
        {
            intValue = 0;
            floatValue = 0f;
            valueType = StatValueType.Int;

            if (string.IsNullOrWhiteSpace(statId))
            {
                return false;
            }

            var meta = StatRegistry.GetMeta(statId);
            if (meta == null)
            {
                return false;
            }

            var hasKey = meta.HasKey;
            if (hasKey && string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            valueType = meta.StatValueType;
            var isGameStat = meta.StatType == StatType.GAME;
            var isPawnStat = meta.StatType == StatType.PAWN;

            switch (meta.Source)
            {
                case StatSource.Manual:
                    return TryGetManualValue(statId, key, pawn, isGameStat, isPawnStat, meta.StatValueType, hasKey, out intValue, out floatValue);
                case StatSource.RecordDef:
                    return TryGetRecordValue(meta, pawn, isGameStat, isPawnStat, hasKey, out intValue, out floatValue);
                case StatSource.CalculatedStat:
                    if (hasKey)
                    {
                        return false;
                    }
                    return TryGetCalculatedValue(statId, meta, pawn, isGameStat, isPawnStat, out intValue, out floatValue);
                default:
                    return false;
            }
        }

        private static bool TryGetManualValue(
            string statId,
            string key,
            Pawn pawn,
            bool isGameStat,
            bool isPawnStat,
            StatValueType statType,
            bool hasKey,
            out int intValue,
            out float floatValue)
        {
            intValue = 0;
            floatValue = 0f;

            if (isGameStat)
            {
                var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
                if (gameStats == null)
                {
                    return false;
                }

                if (!gameStats.All.TryGetValue(new StatKey(statId, hasKey ? key : string.Empty), out var record) || record == null)
                {
                    return false;
                }

                if (statType == StatValueType.Float)
                {
                    floatValue = record.TotalFloat;
                }
                else
                {
                    intValue = record.TotalInt;
                }

                return true;
            }

            if (isPawnStat && pawn == null)
            {
                return false;
            }

            if (pawn != null && pawn.TryGetComp(out Comp_PawnStats comp))
            {
                var hasRecord = hasKey
                    ? comp.TryGetStat(statId, key, out var pawnRecord)
                    : comp.TryGetStat(statId, out pawnRecord);

                if (!hasRecord || pawnRecord == null)
                {
                    return false;
                }

                if (statType == StatValueType.Float)
                {
                    floatValue = pawnRecord.TotalFloat;
                }
                else
                {
                    intValue = pawnRecord.TotalInt;
                }

                return true;
            }

            return false;
        }

        private static bool TryGetRecordValue(
            StatMeta meta,
            Pawn pawn,
            bool isGameStat,
            bool isPawnStat,
            bool hasKey,
            out int intValue,
            out float floatValue)
        {
            intValue = 0;
            floatValue = 0f;

            if (hasKey)
            {
                return false;
            }

            if (isPawnStat && pawn != null)
            {
                if (meta.StatValueType == StatValueType.Float)
                {
                    floatValue = RecordValueReader.GetRecordValueFloat(pawn, meta.RecordDefName);
                }
                else
                {
                    intValue = RecordValueReader.GetRecordValueInt(pawn, meta.RecordDefName);
                }

                return true;
            }

            if (isGameStat && meta.CalculatorType != null)
            {
                return TryGetCalculatedValue(meta.TypeId, meta, null, true, false, out intValue, out floatValue);
            }

            return false;
        }

        private static bool TryGetCalculatedValue(
            string statId,
            StatMeta meta,
            Pawn pawn,
            bool isGameStat,
            bool isPawnStat,
            out int intValue,
            out float floatValue)
        {
            intValue = 0;
            floatValue = 0f;

            if (meta.CalculatorType == null)
            {
                return false;
            }

            var provider = CalculatedStatProviderCache.GetOrCreate(meta.CalculatorType);
            if (provider == null)
            {
                return false;
            }

            if (isGameStat)
            {
                if (meta.StatValueType == StatValueType.Float)
                {
                    floatValue = provider.CalculateFloat(statId);
                }
                else
                {
                    intValue = provider.CalculateInt(statId);
                }

                return true;
            }

            if (isPawnStat && pawn != null && pawn.TryGetComp(out Comp_PawnStats comp))
            {
                if (meta.StatValueType == StatValueType.Float)
                {
                    floatValue = provider.CalculateFloat(statId, comp);
                }
                else
                {
                    intValue = provider.CalculateInt(statId, comp);
                }

                return true;
            }

            return false;
        }
    }
}
