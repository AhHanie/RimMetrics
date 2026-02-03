using System.Collections.Generic;
using RimMetrics.Components;

namespace RimMetrics.CalculatedStats
{
    public interface ICalculatedKeyedStatProvider
    {
        Dictionary<string, int> CalculateKeyedIntTotals(string statId);
        Dictionary<string, float> CalculateKeyedFloatTotals(string statId);
        Dictionary<string, int> CalculateKeyedIntTotals(string statId, Comp_PawnStats stats);
        Dictionary<string, float> CalculateKeyedFloatTotals(string statId, Comp_PawnStats stats);
    }
}
