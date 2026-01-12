using System.Collections.Generic;
using RimMetrics;
using Verse;

namespace RimMetrics.Components
{
    public class CompProperties_PawnStats : CompProperties
    {
        public CompProperties_PawnStats()
        {
            compClass = typeof(Comp_PawnStats);
        }
    }

    public class Comp_PawnStats : ThingComp
    {
        private const int CurrentVersion = 1;
        private Dictionary<StatKey, StatRecord> stats = new Dictionary<StatKey, StatRecord>();
        private Dictionary<string, Dictionary<string, StatRecord>> keyedStatsByType = new Dictionary<string, Dictionary<string, StatRecord>>();
        private bool isRaider;
        private bool nearDeathBleedActive;
        private string cachedWeaponDefName;
        private int cachedWeaponUpdatedTick;
        private int version = CurrentVersion;

        public CompProperties_PawnStats Props => (CompProperties_PawnStats)props;

        public bool IsRaider
        {
            get => isRaider;
            set => isRaider = value;
        }

        public bool NearDeathBleedActive
        {
            get => nearDeathBleedActive;
            set => nearDeathBleedActive = value;
        }

        public string CachedWeaponDefName
        {
            get => cachedWeaponDefName;
            set => cachedWeaponDefName = value;
        }

        public int CachedWeaponUpdatedTick
        {
            get => cachedWeaponUpdatedTick;
            set => cachedWeaponUpdatedTick = value;
        }

        public void SetStat(string typeId, StatRecord record)
        {
            var meta = StatRegistry.GetMeta(typeId);
            if (meta.Source != StatSource.Manual)
            {
                return;
            }

            if (!ShouldTrackPawnStats())
            {
                return;
            }

            var key = new StatKey(typeId, record?.Key);
            stats[key] = record;
        }

        public bool TryGetStat(string typeId, out StatRecord record)
        {
            var key = new StatKey(typeId);
            return stats.TryGetValue(key, out record);
        }

        public bool TryGetStat(string typeId, string key, out StatRecord record)
        {
            var statKey = new StatKey(typeId, key);
            return stats.TryGetValue(statKey, out record);
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

        public IReadOnlyDictionary<StatKey, StatRecord> All => stats;

        public void IncrementTotalInt(string typeId, int amount = 1)
        {
            IncrementTotalInt(typeId, string.Empty, amount);
        }

        public void IncrementTotalInt(string typeId, string key, int amount = 1)
        {
            var meta = StatRegistry.GetMeta(typeId);
            if (meta == null || (meta != null && meta.Source != StatSource.Manual))
            {
                return;
            }

            if (!ShouldTrackPawnStats())
            {
                return;
            }

            Logger.Message($"Incrementing int stat {StatStringHelper.ToKeyedString(typeId, key)} by {amount} for {(parent as Pawn)?.LabelCap}.");

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
            if (meta == null || (meta != null && meta.Source != StatSource.Manual))
            {
                return;
            }

            if (!ShouldTrackPawnStats())
            {
                return;
            }

            Logger.Message($"Incrementing float stat {StatStringHelper.ToKeyedString(typeId, key)} by {amount} for {(parent as Pawn)?.LabelCap}.");

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
            Logger.Message($"Setting float stat {StatStringHelper.ToKeyedString(typeId)} by {value} for {(parent as Pawn)?.LabelCap}.");
            SetTotalFloat(typeId, string.Empty, value);
        }

        public void SetTotalFloat(string typeId, string key, float value)
        {
            var meta = StatRegistry.GetMeta(typeId);
            if (meta != null && meta.Source != StatSource.Manual)
            {
                return;
            }

            if (!ShouldTrackPawnStats())
            {
                return;
            }

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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref version, "rimMetricsVersion", CurrentVersion);
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
            Scribe_Values.Look(ref isRaider, "isRaider");
            Scribe_Values.Look(ref nearDeathBleedActive, "nearDeathBleedActive");
            Scribe_Values.Look(ref cachedWeaponDefName, "cachedWeaponDefName");
            Scribe_Values.Look(ref cachedWeaponUpdatedTick, "cachedWeaponUpdatedTick");
            if (stats == null)
            {
                stats = new Dictionary<StatKey, StatRecord>();
            }
            RebuildKeyedStatsIndex();
            if (cachedWeaponDefName == null)
            {
                cachedWeaponDefName = string.Empty;
            }
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

        private bool ShouldTrackPawnStats()
        {
            if (!ModSettings.OnlyTrackPlayerColonists)
            {
                return true;
            }

            var pawn = parent as Pawn;
            return pawn.IsFreeColonist;
        }
    }
}
