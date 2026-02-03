using System.Collections.Generic;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.CalculatedStats
{
    public class JobTimeShareByTypeStatProvider : CalculatedStatProvider, ICalculatedKeyedStatProvider
    {
        public Dictionary<string, int> CalculateKeyedIntTotals(string statId)
        {
            return null;
        }

        public Dictionary<string, int> CalculateKeyedIntTotals(string statId, Comp_PawnStats stats)
        {
            return null;
        }

        public Dictionary<string, float> CalculateKeyedFloatTotals(string statId)
        {
            var totalByJob = new Dictionary<string, int>();
            var totalTicks = 0;

            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (!colonist.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                if (!comp.TryGetKeyedStats(StatIds.PAWN_JOBS_TOTAL_TIME_BY_TYPE, out var keyed) || keyed == null)
                {
                    continue;
                }

                foreach (var pair in keyed)
                {
                    var ticks = pair.Value?.TotalInt ?? 0;
                    if (ticks <= 0)
                    {
                        continue;
                    }

                    totalTicks += ticks;
                    if (totalByJob.TryGetValue(pair.Key, out var current))
                    {
                        totalByJob[pair.Key] = current + ticks;
                    }
                    else
                    {
                        totalByJob[pair.Key] = ticks;
                    }
                }
            }

            return CalculateShares(totalByJob, totalTicks);
        }

        public Dictionary<string, float> CalculateKeyedFloatTotals(string statId, Comp_PawnStats stats)
        {
            if (stats == null)
            {
                return null;
            }

            if (!stats.TryGetKeyedStats(StatIds.PAWN_JOBS_TOTAL_TIME_BY_TYPE, out var keyed) || keyed == null)
            {
                return null;
            }

            var totalByJob = new Dictionary<string, int>();
            var totalTicks = 0;
            foreach (var pair in keyed)
            {
                var ticks = pair.Value?.TotalInt ?? 0;
                if (ticks <= 0)
                {
                    continue;
                }

                totalTicks += ticks;
                totalByJob[pair.Key] = ticks;
            }

            return CalculateShares(totalByJob, totalTicks);
        }

        private static Dictionary<string, float> CalculateShares(Dictionary<string, int> totalByJob, int totalTicks)
        {
            if (totalTicks <= 0 || totalByJob == null || totalByJob.Count == 0)
            {
                return null;
            }

            var result = new Dictionary<string, float>(totalByJob.Count);
            foreach (var pair in totalByJob)
            {
                var share = (pair.Value / (float)totalTicks) * 100f;
                result[pair.Key] = share;
            }

            return result;
        }
    }
}
