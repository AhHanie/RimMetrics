using HarmonyLib;
using RimMetrics.Components;
using Verse;
using Verse.AI;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(MentalStateHandler), "TryStartMentalState")]
    public static class PawnMentalStateStatsPatch
    {
        public static void Postfix(MentalStateHandler __instance, bool __result, MentalStateDef stateDef)
        {
            if (!__result)
            {
                return;
            }

            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_TIMES_IN_MENTAL_STATE_BY_TYPE, stateDef.defName);
        }
    }
}
