using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class PawnMemoryThoughtStatsPatch
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought, Pawn otherPawn)
        {
            var pawn = __instance.pawn;
            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            if (!ThoughtUtility.CanGetThought(pawn, newThought.def))
            {
                return;
            }

            if (newThought is Thought_MemorySocial)
            {
                otherPawn = otherPawn ?? newThought.otherPawn;
                if (otherPawn == null)
                {
                    return;
                }

                if (!newThought.def.socialTargetDevelopmentalStageFilter.Has(otherPawn.DevelopmentalStage))
                {
                    return;
                }
            }

            comp.IncrementTotalInt(StatIds.PAWN_MEMORY_THOUGHTS);
            comp.IncrementTotalInt(StatIds.PAWN_MEMORY_THOUGHTS_BY_TYPE, newThought.def.defName);
        }
    }
}
