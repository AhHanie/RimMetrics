using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(TradeSession), "SetupWith")]
    public static class PawnTradeInitiatedPatch
    {
        public static void Postfix(ITrader newTrader, Pawn newPlayerNegotiator, bool giftMode)
        {
            if (giftMode)
            {
                return;
            }

            if (!newPlayerNegotiator.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_TRADES_INITIATED);
        }
    }
}
