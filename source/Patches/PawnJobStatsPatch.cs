using HarmonyLib;
using RimMetrics.Components;
using RimMetrics.Helpers;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public static class PawnJobStartedPatch
    {
        public static void Postfix(Pawn_JobTracker __instance, Job newJob)
        {
            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            if (__instance.curJob != newJob || __instance.curDriver == null || __instance.curDriver.job != newJob)
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_JOBS_STARTED);
            comp.IncrementTotalInt(StatIds.PAWN_JOBS_STARTED_BY_TYPE, newJob.def.defName);
            JobUsageTracker.RecordJobStarted(comp, newJob, Find.TickManager.TicksGame);
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "CleanupCurrentJob")]
    public static class PawnJobEndedPatch
    {
        public static void Prefix(Pawn_JobTracker __instance)
        {
            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            JobUsageTracker.RecordJobEnded(comp, Find.TickManager.TicksGame);
        }
    }
}
