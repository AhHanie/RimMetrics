using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(WorldComponent_GravshipController), nameof(WorldComponent_GravshipController.InitiateTakeoff))]
    public static class GameGravshipLaunchedStatsPatch
    {
        public static void Prefix()
        {
            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_GRAVSHIPS_LAUNCHED);
        }
    }

    [HarmonyPatch(typeof(CaravanShuttleUtility), nameof(CaravanShuttleUtility.LaunchShuttle))]
    public static class GameShuttleLaunchedStatsPatch
    {
        public static void Prefix()
        {
            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            gameStats.IncrementTotalInt(StatIds.GAME_SHUTTLES_LAUNCHED);
        }
    }

    [HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.TryLaunch))]
    public static class GameTransportPodsLaunchedStatsPatch
    {
        public static void Prefix(CompLaunchable __instance)
        {
            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            if (__instance is CompLaunchable_TransportPod)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_TRANSPORT_PODS_LAUNCHED);
            }
            else if (ModsConfig.OdysseyActive && __instance.parent.def == ThingDefOf.PassengerShuttle)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_SHUTTLES_LAUNCHED);
            }
        }
    }
}
