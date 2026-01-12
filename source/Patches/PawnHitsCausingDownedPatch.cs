using HarmonyLib;
using RimMetrics.Components;
using Verse;
using RimWorld;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(RecordsUtility), "Notify_PawnDowned")]
    public static class PawnHitsCausingDownedPatch
    {
        public static void Postfix(Pawn downed, Pawn instigator)
        {
            if (downed.TryGetComp(out Comp_PawnStats downedComp))
            {
                downedComp.IncrementTotalInt(StatIds.PAWN_DOWNED);
            }
        }
    }
}
