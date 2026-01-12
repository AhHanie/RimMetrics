using System.Collections.Generic;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(TradeDeal), "TryExecute")]
    public static class PawnNegotiatedTradesPatch
    {
        private static float pendingEarned;
        private static float pendingPaid;
        private static Pawn pendingNegotiator;
        private static Dictionary<string, int> pendingItemsBoughtByType = new Dictionary<string, int>();
        private static Dictionary<string, int> pendingItemsSoldByType = new Dictionary<string, int>();

        public static void Prefix(TradeDeal __instance)
        {
            pendingEarned = 0f;
            pendingPaid = 0f;
            pendingNegotiator = TradeSession.playerNegotiator;
            pendingItemsBoughtByType.Clear();
            pendingItemsSoldByType.Clear();

            var tradeables = __instance.AllTradeables;
            foreach (var tradeable in tradeables)
            {
                if (tradeable == null || tradeable.IsCurrency || tradeable.IsFavor)
                {
                    continue;
                }

                switch (tradeable.ActionToDo)
                {
                    case TradeAction.PlayerBuys:
                        {
                            var count = tradeable.CountToTransferToSource;
                            if (count > 0)
                            {
                                pendingPaid += tradeable.GetPriceFor(TradeAction.PlayerBuys) * count;
                                AccumulateItemCount(pendingItemsBoughtByType, tradeable, count);
                            }

                            break;
                        }
                    case TradeAction.PlayerSells:
                        {
                            var count = tradeable.CountToTransferToDestination;
                            if (count > 0)
                            {
                                pendingEarned += tradeable.GetPriceFor(TradeAction.PlayerSells) * count;
                                AccumulateItemCount(pendingItemsSoldByType, tradeable, count);
                            }

                            break;
                        }
                }
            }
        }

        public static void Postfix(bool __result, ref bool actuallyTraded)
        {
            if (!__result || !actuallyTraded)
            {
                pendingNegotiator = null;
                return;
            }

            var negotiator = pendingNegotiator ?? TradeSession.playerNegotiator;
            if (negotiator == null)
            {
                return;
            }

            if (!negotiator.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_NEGOTIATED_TRADES);

            if (pendingEarned > 0f)
            {
                comp.IncrementTotalFloat(StatIds.PAWN_TRADES_EARNED, pendingEarned);
            }

            if (pendingPaid > 0f)
            {
                comp.IncrementTotalFloat(StatIds.PAWN_TRADES_PAID, pendingPaid);
            }

            foreach (var pair in pendingItemsBoughtByType)
            {
                comp.IncrementTotalInt(StatIds.PAWN_ITEMS_BOUGHT_BY_TYPE, pair.Key, pair.Value);
            }

            foreach (var pair in pendingItemsSoldByType)
            {
                comp.IncrementTotalInt(StatIds.PAWN_ITEMS_SOLD_BY_TYPE, pair.Key, pair.Value);
            }
        }

        private static void AccumulateItemCount(Dictionary<string, int> totals, Tradeable tradeable, int count)
        {
            var defName = tradeable.ThingDef.defName;
            if (totals.TryGetValue(defName, out var existing))
            {
                totals[defName] = existing + count;
            }
            else
            {
                totals[defName] = count;
            }
        }
    }
}
