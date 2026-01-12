using HarmonyLib;
using RimMetrics.Components;
using RimWorld.Planet;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Caravan), "AddPawn")]
    public static class PawnCaravanJoinPatch
    {
        public static void Postfix(Pawn p)
        {
            if (!p.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_CARAVANS_JOINED);
        }
    }
}
