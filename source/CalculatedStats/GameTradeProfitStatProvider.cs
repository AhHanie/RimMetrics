namespace RimMetrics.CalculatedStats
{
    public class GameTradeProfitStatProvider : CalculatedStatProvider
    {
        public override float CalculateFloat(string statId)
        {
            var provider = CalculatedStatProviderCache.GetOrCreate(typeof(ColonistManualTotalStatProvider));
            return provider.CalculateFloat(StatIds.PAWN_TRADE_PROFIT);
        }
    }
}
