using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(RecordsCardUtility), nameof(RecordsCardUtility.DrawRecordsCard))]
    public static class RecordsCardOverridePatch
    {
        private static readonly StatsListPanel Panel = new StatsListPanel();

        public static bool Prepare()
        {
            return ModSettings.OverrideRecordsTabWithStats;
        }

        public static bool Prefix(Rect rect, Pawn pawn)
        {
            if (!ModSettings.OverrideRecordsTabWithStats)
            {
                return true;
            }

            if (ModSettings.OnlyTrackPlayerColonists && pawn != null && !pawn.IsFreeColonist)
            {
                return true;
            }

            Panel.Draw(rect, pawn);
            return false;
        }
    }
}
