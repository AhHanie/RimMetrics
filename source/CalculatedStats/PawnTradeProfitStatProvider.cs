using RimMetrics.Components;

namespace RimMetrics.CalculatedStats
{
    public class PawnTradeProfitStatProvider : CalculatedStatProvider
    {
        public override float CalculateFloat(string statId, Comp_PawnStats stats)
        {
            var earned = 0f;
            var paid = 0f;
            if (stats.TryGetStat(StatIds.PAWN_TRADES_EARNED, out var earnedRecord))
            {
                earned = earnedRecord.TotalFloat;
            }

            if (stats.TryGetStat(StatIds.PAWN_TRADES_PAID, out var paidRecord))
            {
                paid = paidRecord.TotalFloat;
            }

            return earned - paid;
        }
    }
}
