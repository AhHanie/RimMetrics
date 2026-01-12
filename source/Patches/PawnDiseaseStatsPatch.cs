using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult))]
    public static class PawnDiseaseContractedPatch
    {
        public static void Postfix(Pawn_HealthTracker __instance, Hediff hediff)
        {
            if (hediff.def == null)
            {
                return;
            }

            if (!DiseaseDefs.IsDisease(hediff.def))
            {
                return;
            }

            var pawn = __instance.pawn;
            if (pawn == null)
            {
                return;
            }

            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_DISEASES_CONTRACTED);
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker), "RemoveHediff")]
    public static class PawnDiseaseRecoveredPatch
    {
        public static void Postfix(Pawn_HealthTracker __instance, Hediff hediff)
        {
            if (hediff.def == null)
            {
                return;
            }

            if (!DiseaseDefs.IsDisease(hediff.def))
            {
                return;
            }

            var pawn = __instance.pawn;
            if (pawn == null)
            {
                return;
            }

            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_DISEASES_RECOVERED);
        }
    }
}
