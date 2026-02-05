using System.Collections.Generic;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(IncidentWorker_NeutralGroup), "SpawnPawns")]
    public static class GameNeutralGroupSpawnCachePatch
    {
        private static List<Pawn> spawnedPawns;

        public static void Postfix(IncidentWorker __instance, List<Pawn> __result)
        {
            if (__result == null || __result.Count == 0)
            {
                return;
            }

            if (!(__instance is IncidentWorker_VisitorGroup) && !(__instance is IncidentWorker_TraderCaravanArrival))
            {
                return;
            }

            spawnedPawns = __result;
        }

        public static List<Pawn> TakeSpawnedPawns()
        {
            var pawns = spawnedPawns;
            spawnedPawns = null;
            return pawns;
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_VisitorGroup), "TryExecuteWorker")]
    public static class GameVisitorGroupStatsPatch
    {
        public static void Postfix(bool __result, IncidentWorker_VisitorGroup __instance)
        {
            if (!__result)
            {
                return;
            }

            var pawns = GameNeutralGroupSpawnCachePatch.TakeSpawnedPawns();
            if (pawns == null || pawns.Count == 0)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_VISITORS, pawns.Count);
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_TraderCaravanArrival), "TryExecuteWorker")]
    public static class GameTraderCaravanVisitorStatsPatch
    {
        public static void Postfix(bool __result, IncidentWorker_TraderCaravanArrival __instance)
        {
            if (!__result)
            {
                return;
            }

            var pawns = GameNeutralGroupSpawnCachePatch.TakeSpawnedPawns();
            if (pawns == null || pawns.Count == 0)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_VISITORS, pawns.Count);
            gameStats.IncrementTotalInt(StatIds.GAME_TRADE_CARAVANS_VISITED_PAWNS, pawns.Count);
        }
    }
}
