using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.CalculatedStats
{
    public class PawnInfectionsReceivedStatProvider : CalculatedStatProvider
    {
        private const string InfectionDefName = "WoundInfection";

        public override int CalculateInt(string statId, Comp_PawnStats stats)
        {
            if (stats == null)
            {
                return 0;
            }

            if (!stats.TryGetStat(StatIds.PAWN_DISEASES_CONTRACTED_BY_TYPE, InfectionDefName, out var record) || record == null)
            {
                return 0;
            }

            return record.TotalInt;
        }

        public override int CalculateInt(string statId)
        {
            var total = 0;
            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (!colonist.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                total += CalculateInt(statId, comp);
            }

            return total;
        }
    }
}
