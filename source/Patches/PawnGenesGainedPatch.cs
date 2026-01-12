using System.Collections;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Pawn_GeneTracker), "AddGene", new[] { typeof(Gene), typeof(bool) })]
    public static class PawnGenesGainedPatch
    {
        public static bool shouldTrack = false;
        public static bool Prepare()
        {
            return ModsConfig.BiotechActive;
        }

        public static void Postfix(Pawn_GeneTracker __instance, Gene gene)
        {
            if (!shouldTrack)
            {
                return;
            }

            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_GENES_GAINED);
            comp.IncrementTotalInt(StatIds.PAWN_GENES_GAINED_BY_TYPE, gene.def.defName);
        }
    }

    [HarmonyPatch(typeof(GeneUtility), "ImplantXenogermItem")]
    public static class ImplantXenogermItemPatch
    {
        public static void Prefix()
        {
            PawnGenesGainedPatch.shouldTrack = true;
        }

        public static void Postfix()
        {
            PawnGenesGainedPatch.shouldTrack = false;
        }
    }

    [HarmonyPatch(typeof(GeneUtility), "ReimplantXenogerm")]
    public static class ReimplantXenogermPatch
    {
        public static void Prefix()
        {
            PawnGenesGainedPatch.shouldTrack = true;
        }

        public static void Postfix()
        {
            PawnGenesGainedPatch.shouldTrack = false;
        }
    }
}
