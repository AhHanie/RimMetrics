using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(IncidentWorker), "TryExecute")]
    public static class GameIncidentStatsPatch
    {
        public static void Postfix(bool __result, IncidentWorker __instance, IncidentParms parms)
        {
            if (!__result)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_TOTAL_INCIDENTS);
            gameStats.IncrementTotalInt(StatIds.GAME_TOTAL_INCIDENTS_BY_TYPE, __instance.def.defName);
            if (__instance is IncidentWorker_Raid)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_TOTAL_RAIDS);
                var raidFaction = parms?.faction;
                if (raidFaction != null)
                {
                    gameStats.IncrementTotalInt(StatIds.GAME_TOTAL_RAIDS_BY_FACTION, raidFaction.def.defName);
                }
            }

            if (__instance is IncidentWorker_OrbitalTraderArrival)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_ORBITAL_TRADERS_VISITED);
                var traderKind = parms?.traderKind;
                if (traderKind != null)
                {
                    gameStats.IncrementTotalInt(StatIds.GAME_ORBITAL_TRADERS_VISITED_BY_TYPE, traderKind.defName);
                }
            }
            else if (__instance is IncidentWorker_TraderCaravanArrival)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_TRADE_CARAVANS_VISITED);
                var traderKind = parms?.traderKind;
                if (traderKind != null)
                {
                    gameStats.IncrementTotalInt(StatIds.GAME_TRADE_CARAVANS_VISITED_BY_TYPE, traderKind.defName);
                }
            }
            else if (__instance is IncidentWorker_VisitorGroup)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_VISITORS);
            }
        }
    }
}
