using System.Collections.Generic;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.CalculatedStats
{
    public class ColonistManualKeyedTotalStatProvider : CalculatedStatProvider
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

                if (!comp.TryGetKeyedStats(statId, out var keyed))
                {
                    continue;
                }

                foreach (var pair in keyed)
                {
                    total += pair.Value.TotalInt;
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

                if (!comp.TryGetKeyedStats(statId, out var keyed))
                {
                    continue;
                }

                foreach (var pair in keyed)
                {
                    total += pair.Value.TotalFloat;
                }
            }

            return total;
        }

        public Dictionary<string, int> CalculateKeyedIntTotals(string statId)
        {
            var totals = new Dictionary<string, int>();
            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (!colonist.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                if (!comp.TryGetKeyedStats(statId, out var keyed))
                {
                    continue;
                }

                foreach (var pair in keyed)
                {
                    var key = pair.Key ?? string.Empty;
                    if (totals.TryGetValue(key, out var existing))
                    {
                        totals[key] = existing + pair.Value.TotalInt;
                    }
                    else
                    {
                        totals[key] = pair.Value.TotalInt;
                    }
                }
            }

            return totals;
        }

        public Dictionary<string, float> CalculateKeyedFloatTotals(string statId)
        {
            var totals = new Dictionary<string, float>();
            foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                if (!colonist.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                if (!comp.TryGetKeyedStats(statId, out var keyed))
                {
                    continue;
                }

                foreach (var pair in keyed)
                {
                    var key = pair.Key ?? string.Empty;
                    if (totals.TryGetValue(key, out var existing))
                    {
                        totals[key] = existing + pair.Value.TotalFloat;
                    }
                    else
                    {
                        totals[key] = pair.Value.TotalFloat;
                    }
                }
            }

            return totals;
        }
    }
}
