using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(IncidentWorker), "TryExecute")]
    public static class GameIncidentStatsPatch
    {
        public static void Postfix(bool __result, IncidentWorker __instance)
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
            }
        }
    }
}
