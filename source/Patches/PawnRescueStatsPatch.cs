using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(RestUtility), nameof(RestUtility.TuckIntoBed), new[] { typeof(Building_Bed), typeof(Pawn), typeof(Pawn), typeof(bool) })]
    public static class PawnRescueStatsPatch
    {
        public static void Postfix(Pawn taker, Pawn takee, bool rescued)
        {
            if (!rescued || taker == takee)
            {
                return;
            }

            if (takee.TryGetComp(out Comp_PawnStats takeeComp))
            {
                takeeComp.IncrementTotalInt(StatIds.PAWN_RESCUES_RECEIVED);
            }

            if (taker.TryGetComp(out Comp_PawnStats takerComp))
            {
                takerComp.IncrementTotalInt(StatIds.PAWN_RESCUES_PERFORMED);
            }
        }
    }
}
