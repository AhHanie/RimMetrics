using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    public struct RelationChangeState
    {
        public bool ShouldTrack;
        public FactionRelationKind OldKind;
    }

    [HarmonyPatch(typeof(Faction), nameof(Faction.TryAffectGoodwillWith))]
    public static class GameFactionRelationStatsPatch
    {
        public static void Prefix(Faction __instance, Faction other, ref RelationChangeState __state)
        {
            __state = default;

            if (__instance != Faction.OfPlayer && other != Faction.OfPlayer)
            {
                return;
            }

            __state = new RelationChangeState
            {
                ShouldTrack = true,
                OldKind = __instance.RelationKindWith(other)
            };
        }

        public static void Postfix(bool __result, Faction __instance, Faction other, RelationChangeState __state)
        {
            if (!__result || !__state.ShouldTrack)
            {
                return;
            }

            var newKind = __instance.RelationKindWith(other);
            if (newKind == __state.OldKind)
            {
                return;
            }

            if (newKind != FactionRelationKind.Ally && newKind != FactionRelationKind.Hostile)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            if (newKind == FactionRelationKind.Ally)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_FACTIONS_ALLIED);
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_FACTIONS_MADE_HOSTILE);
        }
    }

    [HarmonyPatch(typeof(Faction), nameof(Faction.SetRelationDirect))]
    public static class GameFactionRelationDirectStatsPatch
    {
        public static void Prefix(Faction __instance, Faction other, ref RelationChangeState __state)
        {
            __state = default;

            if (__instance != Faction.OfPlayer && other != Faction.OfPlayer)
            {
                return;
            }

            __state = new RelationChangeState
            {
                ShouldTrack = true,
                OldKind = __instance.RelationKindWith(other)
            };
        }

        public static void Postfix(Faction __instance, Faction other, RelationChangeState __state)
        {
            if (!__state.ShouldTrack)
            {
                return;
            }

            var newKind = __instance.RelationKindWith(other);
            if (newKind == __state.OldKind)
            {
                return;
            }

            if (newKind != FactionRelationKind.Ally && newKind != FactionRelationKind.Hostile)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            if (newKind == FactionRelationKind.Ally)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_FACTIONS_ALLIED);
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_FACTIONS_MADE_HOSTILE);
        }
    }
}
