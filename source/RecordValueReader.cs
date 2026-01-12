using RimWorld;
using Verse;

namespace RimMetrics
{
    public static class RecordValueReader
    {
        public static float GetRecordValueFloat(Pawn pawn, string recordDefName)
        {
            if (pawn?.records == null || string.IsNullOrWhiteSpace(recordDefName))
            {
                return 0f;
            }

            var def = DefDatabase<RecordDef>.GetNamedSilentFail(recordDefName);
            if (def == null)
            {
                return 0f;
            }

            return pawn.records.GetValue(def);
        }

        public static int GetRecordValueInt(Pawn pawn, string recordDefName)
        {
            if (pawn?.records == null || string.IsNullOrWhiteSpace(recordDefName))
            {
                return 0;
            }

            var def = DefDatabase<RecordDef>.GetNamedSilentFail(recordDefName);
            if (def == null)
            {
                return 0;
            }

            return pawn.records.GetAsInt(def);
        }
    }
}
