using System.Collections.Generic;
using Verse;

namespace RimMetrics
{
    public class TopStatEntry : IExposable
    {
        private string statId;
        private int rank;
        private int totalInt;
        private float totalFloat;
        private StatValueType valueType;

        public TopStatEntry()
        {
        }

        public TopStatEntry(string statId, int rank, StatValueType valueType, int totalInt, float totalFloat)
        {
            this.statId = statId ?? string.Empty;
            this.rank = rank;
            this.valueType = valueType;
            this.totalInt = totalInt;
            this.totalFloat = totalFloat;
        }

        public string StatId => statId;
        public int Rank => rank;
        public int TotalInt => totalInt;
        public float TotalFloat => totalFloat;
        public StatValueType ValueType => valueType;

        public void ExposeData()
        {
            Scribe_Values.Look(ref statId, "statId");
            Scribe_Values.Look(ref rank, "rank");
            Scribe_Values.Look(ref totalInt, "totalInt");
            Scribe_Values.Look(ref totalFloat, "totalFloat");
            Scribe_Values.Look(ref valueType, "valueType");
            if (statId == null)
            {
                statId = string.Empty;
            }
        }
    }

    public class ColonistTopStats : IExposable
    {
        private Pawn pawn;
        private List<TopStatEntry> stats = new List<TopStatEntry>();

        public ColonistTopStats()
        {
        }

        public ColonistTopStats(Pawn pawn, List<TopStatEntry> stats)
        {
            this.pawn = pawn;
            this.stats = stats ?? new List<TopStatEntry>();
        }

        public Pawn Pawn => pawn;
        public IReadOnlyList<TopStatEntry> Stats => stats;

        public void Add(TopStatEntry entry)
        {
            if (entry != null)
            {
                stats.Add(entry);
            }
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_Collections.Look(ref stats, "stats", LookMode.Deep);
            if (stats == null)
            {
                stats = new List<TopStatEntry>();
            }
        }
    }
}
