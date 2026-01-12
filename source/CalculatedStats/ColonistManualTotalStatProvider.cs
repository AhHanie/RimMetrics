using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.CalculatedStats
{
    public class ColonistManualTotalStatProvider : CalculatedStatProvider
    {
        public override int CalculateInt(string statId)
        {
            var total = 0;
            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (!colonist.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                if (comp.TryGetStat(statId, out var record))
                {
                    total += record.TotalInt;
                }
            }

            return total;
        }

        public override float CalculateFloat(string statId)
        {
            var total = 0f;
            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (!colonist.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                if (comp.TryGetStat(statId, out var record))
                {
                    total += record.TotalFloat;
                }
            }

            return total;
        }
    }
}
