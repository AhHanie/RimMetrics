using System.Collections.Generic;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;
using Verse.AI;
using RimMetrics;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(JobDriver_Research), "MakeNewToils")]
    public static class PawnResearchStartedPatch
    {
        public static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_Research __instance)
        {
            return WrapToils(__result, __instance);
        }

        private static IEnumerable<Toil> WrapToils(IEnumerable<Toil> toils, JobDriver_Research driver)
        {
            var wrapped = false;
            foreach (var toil in toils)
            {
                if (!wrapped && IsResearchToil(toil))
                {
                    var originalInit = toil.initAction;
                    toil.initAction = delegate
                    {
                        if (toil.actor.TryGetComp(out Comp_PawnStats comp))
                        {
                            comp.IncrementTotalInt(StatIds.PAWN_RESEARCH_SESSIONS);
                        }

                        originalInit?.Invoke();
                    };
                    wrapped = true;
                }

                yield return toil;
            }
        }

        private static bool IsResearchToil(Toil toil)
        {
            return toil.defaultCompleteMode == ToilCompleteMode.Delay && toil.activeSkill() == SkillDefOf.Intellectual;
        }
    }
}
