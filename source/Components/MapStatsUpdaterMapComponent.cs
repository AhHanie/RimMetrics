using System.Collections.Generic;
using RimWorld;
using RimMetrics.Helpers;
using Verse;
using System.Linq;

namespace RimMetrics.Components
{
    public class MapStatsUpdaterMapComponent : MapComponent
    {
        private HashSet<int> trackedRoomIds = new HashSet<int>();

        public MapStatsUpdaterMapComponent(Map map) : base(map)
        {
        }

        public override void FinalizeInit()
        {
            var currentTick = Find.TickManager.TicksGame;
            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!pawn.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                WeaponUsageTracker.SeedForPawn(pawn, comp, currentTick);
            }
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay != 0)
            {
                return;
            }

            var currentTick = Find.TickManager.TicksGame;
            foreach (var pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!pawn.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                ResetNearDeathBleedFlags(comp);
                IncrementSickDays(pawn, comp);
                UpdateWeaponUsage(pawn, comp, currentTick);
            }

            UpdateRoomsBuilt();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref trackedRoomIds, "rimMetricsTrackedRoomIds", LookMode.Value);
            if (trackedRoomIds == null)
            {
                trackedRoomIds = new HashSet<int>();
            }
        }

        private void ResetNearDeathBleedFlags(Comp_PawnStats comp)
        {
            if (comp.NearDeathBleedActive)
            {
                comp.NearDeathBleedActive = false;
            }
        }

        private void IncrementSickDays(Pawn pawn, Comp_PawnStats comp)
        {
            if (pawn.health.hediffSet?.hediffs == null)
            {
                return;
            }

            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (DiseaseDefs.IsDisease(hediff.def))
                {
                    comp.IncrementTotalInt(StatIds.PAWN_DAYS_SICK);
                    break;
                }
            }
        }

        private void UpdateWeaponUsage(Pawn pawn, Comp_PawnStats comp, int currentTick)
        {
            WeaponUsageTracker.SyncDaily(pawn, comp, currentTick);
        }

        private void UpdateRoomsBuilt()
        {
            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            var allRooms = map.regionGrid.AllRooms;

            var currentRoomIds = new HashSet<int>();
            foreach (var room in allRooms)
            {
                if (room == null || room.PsychologicallyOutdoors || !RoomIsPlayerBuilt(room, this.map) || room.CellCount == 1)
                {
                    continue;
                }

                if (trackedRoomIds.Add(room.ID))
                {
                    gameStats.IncrementTotalInt(StatIds.GAME_ROOMS_BUILT);
                    if (room.Role != RoomRoleDefOf.None)
                    {
                        gameStats.IncrementTotalInt(StatIds.GAME_ROOMS_BUILT_BY_ROLE, room.Role.defName);
                    }
                }

                currentRoomIds.Add(room.ID);
            }

            trackedRoomIds = currentRoomIds;
        }

        private bool RoomIsPlayerBuilt(Room room, Map map)
        {
            foreach (var cell in room.BorderCellsCached)
            {
                var edifice = cell.GetEdifice(map);
                if (edifice == null || edifice.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                if (edifice is Building_Door)
                {
                    return true;
                }

                if (edifice.def.building.isWall)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
