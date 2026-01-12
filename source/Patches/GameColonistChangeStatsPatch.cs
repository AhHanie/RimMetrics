using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(KidnappedPawnsTracker), nameof(KidnappedPawnsTracker.Kidnap))]
    public static class GameColonistKidnapStatsPatch
    {
        public static void Prefix(Pawn pawn)
        {
            if (pawn.Faction != Faction.OfPlayer)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_COLONISTS_LOST);
        }
    }

    [HarmonyPatch(typeof(KidnappedPawnsTracker), nameof(KidnappedPawnsTracker.RemoveKidnappedPawn))]
    public static class GameColonistKidnapResolvedStatsPatch
    {
        public static void Prefix(Pawn pawn)
        {
            if (pawn.Faction != Faction.OfPlayer)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_COLONISTS_JOINED);
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
    public static class GameColonistFactionChangeStatsPatch
    {
        public static void Prefix(Pawn __instance, Faction newFaction, ref Faction __state)
        {
           var oldFaction = __instance.Faction; 

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            if (oldFaction == Faction.OfPlayer && newFaction != Faction.OfPlayer && __instance.IsFreeColonist)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_COLONISTS_LOST);
                return;
            }

            __state = oldFaction;
        }

        public static void Postfix(Pawn __instance, Faction newFaction, Faction __state)
        {
            if (__state == null)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            if (newFaction == Faction.OfPlayer && __state != Faction.OfPlayer && __instance.IsFreeColonist)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_COLONISTS_JOINED);
                return;
            }
        }
    }
}
