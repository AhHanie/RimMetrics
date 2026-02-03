using RimMetrics.Components;
using Verse;
using Verse.AI;

namespace RimMetrics.Helpers
{
    public static class JobUsageTracker
    {
        public static void RecordJobStarted(Comp_PawnStats comp, Job job, int currentTick)
        {
            FlushUsage(comp, currentTick);
            comp.CachedJobDefName = job.def.defName;
            comp.CachedJobUpdatedTick = currentTick;
        }

        public static void RecordJobEnded(Comp_PawnStats comp, int currentTick)
        {
            FlushUsage(comp, currentTick);
            comp.CachedJobDefName = string.Empty;
            comp.CachedJobUpdatedTick = currentTick;
        }

        private static void FlushUsage(Comp_PawnStats comp, int currentTick)
        {
            if (string.IsNullOrWhiteSpace(comp.CachedJobDefName))
            {
                return;
            }

            var elapsedTicks = currentTick - comp.CachedJobUpdatedTick;
            if (elapsedTicks <= 0)
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_JOBS_TOTAL_TIME_BY_TYPE, comp.CachedJobDefName, elapsedTicks);
        }
    }
}
