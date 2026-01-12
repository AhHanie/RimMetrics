using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.CalculatedStats
{
    public class GameShotsAccuracyAverageStatProvider : CalculatedStatProvider
    {
        public override float CalculateFloat(string statId)
        {
            var pawnStatId = StatIds.PAWN_SHOTS_ACCURACY;
            var total = 0f;
            var count = 0;

            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                count++;
                if (!colonist.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                if (comp.TryGetStat(pawnStatId, out var record))
                {
                    total += record.TotalFloat;
                }
            }

            if (count <= 0)
            {
                return 0f;
            }

            var average = total / count;
            return (float)System.Math.Round(average, 2, System.MidpointRounding.AwayFromZero);
        }
    }
}
