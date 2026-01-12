using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(JobDriver_SmoothFloor), "DoEffect")]
    public static class PawnSmoothFloorPatch
    {
        public static void Postfix(JobDriver_SmoothFloor __instance)
        {
            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_FLOORS_SMOOTHED);
        }
    }

    [HarmonyPatch(typeof(SmoothableWallUtility), nameof(SmoothableWallUtility.Notify_SmoothedByPawn))]
    public static class PawnSmoothWallPatch
    {
        public static void Postfix(Pawn p)
        {
            if (!p.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_WALLS_SMOOTHED);
        }
    }
}
