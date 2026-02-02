using System.Collections.Generic;
using RimMetrics;
using RimMetrics.Helpers;
using RimWorld;
using Verse;

namespace RimMetrics.Components
{
    public class GameComponent_GameStats : GameComponent
    {
        private const int CurrentVersion = 1;
        private Dictionary<StatKey, StatRecord> stats = new Dictionary<StatKey, StatRecord>();
        private Dictionary<string, Dictionary<string, StatRecord>> keyedStatsByType = new Dictionary<string, Dictionary<string, StatRecord>>();
        private List<ColonistTopStats> colonistTopStats = new List<ColonistTopStats>();
        private List<PawnGroupedStats> colonistGroupedStats = new List<PawnGroupedStats>();
        private int groupedStatsDay = -1;
        private List<StatsGroupingService.CategoryGroup> gameGroupedStats = new List<StatsGroupingService.CategoryGroup>();
        private int gameGroupedStatsDay = -1;
        private int version = CurrentVersion;

        public GameComponent_GameStats(Game game)
        {
        }

        public IReadOnlyDictionary<StatKey, StatRecord> All => stats;
        public IReadOnlyList<ColonistTopStats> ColonistTopStats => colonistTopStats;
        public IReadOnlyList<StatsGroupingService.CategoryGroup> GameGroupedStats => gameGroupedStats;

        public override void GameComponentTick()
        {
            if (Find.TickManager.TicksGame % GenDate.TicksPerDay != 0)
            {
                return;
            }

            UpdateColonistTopStats();
            UpdateColonistGroupedStats();
            UpdateGameGroupedStats();
        }

        public bool TryGetKeyedStats(string typeId, out IReadOnlyDictionary<string, StatRecord> records)
        {
            records = null;
            if (string.IsNullOrWhiteSpace(typeId))
            {
                return false;
            }

            if (!keyedStatsByType.TryGetValue(typeId, out var keyed))
            {
                return false;
            }

            records = keyed;
            return true;
        }

        public bool TryGetGroupedStats(Pawn pawn, out List<StatsGroupingService.CategoryGroup> groupedStats)
        {
            groupedStats = null;
            if (pawn == null)
            {
                return false;
            }

            EnsureGroupedStatsCache();
            foreach (var entry in colonistGroupedStats)
            {
                if (entry.Pawn == pawn)
                {
                    groupedStats = entry.Groups;
                    return true;
                }
            }

            return false;
        }

        public bool IsWaitingForGroupedStats()
        {
            return colonistGroupedStats.Count == 0 && groupedStatsDay != GenDate.DaysPassed;
        }

        public bool TryGetGameGroupedStats(out List<StatsGroupingService.CategoryGroup> groupedStats)
        {
            groupedStats = null;
            EnsureGameGroupedStatsCache();
            if (gameGroupedStats == null)
            {
                return false;
            }

            groupedStats = gameGroupedStats;
            return true;
        }

        public bool IsWaitingForGameGroupedStats()
        {
            return gameGroupedStats.Count == 0 && gameGroupedStatsDay != GenDate.DaysPassed;
        }

        public void ForceRefreshGroupedStats()
        {
            UpdateColonistGroupedStats();
        }

        public void ForceRefreshGameGroupedStats()
        {
            UpdateGameGroupedStats();
        }

        public void IncrementTotalInt(string typeId, int amount = 1)
        {
            IncrementTotalInt(typeId, string.Empty, amount);
        }

        public void IncrementTotalInt(string typeId, string key, int amount = 1)
        {
            var meta = StatRegistry.GetMeta(typeId);
            if (meta == null || meta.Source != StatSource.Manual)
            {
                return;
            }

            Logger.Message($"Incrementing int stat {StatStringHelper.ToKeyedString(typeId, key)} by {amount} for Game.");

            var statKey = new StatKey(typeId, key);
            if (!stats.TryGetValue(statKey, out var record))
            {
                record = new StatRecord(typeId, key);
                stats[statKey] = record;
            }

            record.TotalInt += amount;
            TrackKeyedStat(typeId, key, record);
            StatUpdateEvents.Raise(amount, record);
        }

        public void IncrementTotalFloat(string typeId, float amount)
        {
            IncrementTotalFloat(typeId, string.Empty, amount);
        }

        public void IncrementTotalFloat(string typeId, string key, float amount)
        {
            var meta = StatRegistry.GetMeta(typeId);
            if (meta == null || meta.Source != StatSource.Manual)
            {
                return;
            }

            Logger.Message($"Incrementing float stat {StatStringHelper.ToKeyedString(typeId, key)} by {amount} for Game.");

            var statKey = new StatKey(typeId, key);
            if (!stats.TryGetValue(statKey, out var record))
            {
                record = new StatRecord(typeId, key);
                stats[statKey] = record;
            }

            record.TotalFloat += amount;
            TrackKeyedStat(typeId, key, record);
            StatUpdateEvents.Raise(amount, record);
        }

        public void SetTotalFloat(string typeId, float value)
        {
            SetTotalFloat(typeId, string.Empty, value);
        }

        public void SetTotalFloat(string typeId, string key, float value)
        {
            var meta = StatRegistry.GetMeta(typeId);
            if (meta != null && meta.Source != StatSource.Manual)
            {
                return;
            }

            Logger.Message($"Setting float stat {StatStringHelper.ToKeyedString(typeId)} by {value} for Game.");

            var statKey = new StatKey(typeId, key);
            if (!stats.TryGetValue(statKey, out var record))
            {
                record = new StatRecord(typeId, key);
                stats[statKey] = record;
            }

            var delta = value - record.TotalFloat;
            record.TotalFloat = value;
            TrackKeyedStat(typeId, key, record);
            StatUpdateEvents.Raise(delta, record);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref version, "rimMetricsGameVersion", CurrentVersion);
            Scribe_Collections.Look(ref colonistTopStats, "rimMetricsColonistTopStats", LookMode.Deep);
            Scribe_Collections.Look(ref colonistGroupedStats, "rimMetricsColonistGroupedStats", LookMode.Deep);
            Scribe_Values.Look(ref groupedStatsDay, "rimMetricsGroupedStatsDay", -1);
            Scribe_Collections.Look(ref gameGroupedStats, "rimMetricsGameGroupedStats", LookMode.Deep);
            Scribe_Values.Look(ref gameGroupedStatsDay, "rimMetricsGameGroupedStatsDay", -1);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var entries = new List<StatEntry>(stats.Count);
                foreach (var pair in stats)
                {
                    entries.Add(new StatEntry(pair.Key.TypeId, pair.Key.Key, pair.Value));
                }

                Scribe_Collections.Look(ref entries, "stats", LookMode.Deep);
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                List<StatEntry> entries = null;
                Scribe_Collections.Look(ref entries, "stats", LookMode.Deep);
                if (entries != null)
                {
                    stats = new Dictionary<StatKey, StatRecord>();
                    foreach (var entry in entries)
                    {
                        if (entry == null)
                        {
                            continue;
                        }

                        var statKey = new StatKey(entry.TypeId, entry.Key);
                        stats[statKey] = entry.Record ?? new StatRecord(entry.TypeId, entry.Key);
                    }
                }
                else
                {
                    Scribe_Collections.Look(ref stats, "stats", LookMode.Deep, LookMode.Deep);
                }
            }

            if (stats == null)
            {
                stats = new Dictionary<StatKey, StatRecord>();
            }
            if (colonistTopStats == null)
            {
                colonistTopStats = new List<ColonistTopStats>();
            }
            if (colonistGroupedStats == null)
            {
                colonistGroupedStats = new List<PawnGroupedStats>();
            }
            if (gameGroupedStats == null)
            {
                gameGroupedStats = new List<StatsGroupingService.CategoryGroup>();
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach (var entry in colonistGroupedStats)
                {
                    if (entry?.Groups == null)
                    {
                        continue;
                    }

                    StatsGroupingService.PopulateIconData(entry.Groups);
                }

                StatsGroupingService.PopulateIconData(gameGroupedStats);
            }
            RebuildKeyedStatsIndex();
            if (version <= 0)
            {
                version = CurrentVersion;
            }
        }

        private void TrackKeyedStat(string typeId, string key, StatRecord record)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (!keyedStatsByType.TryGetValue(typeId, out var keyed))
            {
                keyed = new Dictionary<string, StatRecord>();
                keyedStatsByType[typeId] = keyed;
            }

            keyed[key] = record;
        }

        private void RebuildKeyedStatsIndex()
        {
            keyedStatsByType = new Dictionary<string, Dictionary<string, StatRecord>>();
            foreach (var pair in stats)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                TrackKeyedStat(pair.Key.TypeId, pair.Key.Key, pair.Value);
            }
        }

        private void UpdateColonistTopStats()
        {
            colonistTopStats = ColonistTopStatCalculator.BuildTopStats();
        }

        private void UpdateColonistGroupedStats()
        {
            groupedStatsDay = GenDate.DaysPassed;
            colonistGroupedStats = new List<PawnGroupedStats>();
            foreach (var pawn in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
            {
                var groupedStats = StatsGroupingService.BuildGroupedStats(pawn);
                colonistGroupedStats.Add(new PawnGroupedStats(pawn, groupedStats));
            }
        }

        private void UpdateGameGroupedStats()
        {
            gameGroupedStatsDay = GenDate.DaysPassed;
            gameGroupedStats = StatsGroupingService.BuildGroupedGameStats(this);
        }

        private void EnsureGroupedStatsCache()
        {
            if (groupedStatsDay == GenDate.DaysPassed)
            {
                return;
            }

            UpdateColonistGroupedStats();
        }

        private void EnsureGameGroupedStatsCache()
        {
            if (gameGroupedStatsDay == GenDate.DaysPassed)
            {
                return;
            }

            UpdateGameGroupedStats();
        }
    }

    public sealed class PawnGroupedStats
        : IExposable
    {
        private Pawn pawn;
        private List<StatsGroupingService.CategoryGroup> groups = new List<StatsGroupingService.CategoryGroup>();

        public PawnGroupedStats()
        {
        }

        public PawnGroupedStats(Pawn pawn, List<StatsGroupingService.CategoryGroup> groups)
        {
            this.pawn = pawn;
            this.groups = groups ?? new List<StatsGroupingService.CategoryGroup>();
        }

        public Pawn Pawn => pawn;
        public List<StatsGroupingService.CategoryGroup> Groups => groups;

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Collections.Look(ref groups, "groups", LookMode.Deep);
            if (groups == null)
            {
                groups = new List<StatsGroupingService.CategoryGroup>();
            }
        }
    }
}
