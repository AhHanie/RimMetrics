using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Quest), "End")]
    public static class QuestEndStatsPatch
    {
        public static void Prefix(QuestEndOutcome outcome)
        {
            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            switch (outcome)
            {
                case QuestEndOutcome.Success:
                    gameStats.IncrementTotalInt(StatIds.GAME_QUESTS_COMPLETED);
                    break;
                case QuestEndOutcome.Fail:
                    gameStats.IncrementTotalInt(StatIds.GAME_QUESTS_FAILED);
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(Quest), "Accept")]
    public static class QuestAcceptStatsPatch
    {
        public static void Postfix(Quest __instance, Pawn by)
        {
            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_QUESTS_ACCEPTED);
        }
    }
}
