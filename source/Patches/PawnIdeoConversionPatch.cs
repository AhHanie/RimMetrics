using System.Collections.Generic;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch]
    public static class PawnIdeoConversionPatch
    {
        private static Pawn targetPawn;

        [HarmonyPatch(typeof(InteractionWorker_ConvertIdeoAttempt), "Interacted")]
        public static class InteractionWorker_ConvertIdeoAttemptInteracted
        {
            public static void Prefix(Pawn initiator)
            {
                targetPawn = initiator;
            }
        }

        [HarmonyPatch(typeof(CompAbilityEffect_Convert), "Apply")]
        public static class CompAbilityEffect_ConvertApply
        {
            public static void Prefix(CompAbilityEffect_Convert __instance)
            {
                targetPawn = __instance.parent.pawn;
            }
        }


        [HarmonyPatch(typeof(Pawn_IdeoTracker), "IdeoConversionAttempt")]
        [HarmonyPostfix]
        public static void IdeoConversionAttemptPostfix(bool __result)
        {
            if (!__result || targetPawn == null)
            {
                return;
            }

            if (!targetPawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_PEOPLE_CONVERTED);
            targetPawn = null;
        }
    }
}
