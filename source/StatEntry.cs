using Verse;

namespace RimMetrics
{
    public class StatEntry : IExposable
    {
        private string typeId;
        private string key;
        private StatRecord record;

        public StatEntry()
        {
        }

        public StatEntry(string typeId, string key, StatRecord record)
        {
            this.typeId = typeId ?? string.Empty;
            this.key = key ?? string.Empty;
            this.record = record;
        }

        public string TypeId => typeId;
        public string Key => key;
        public StatRecord Record => record;

        public void ExposeData()
        {
            Scribe_Values.Look(ref typeId, "typeId");
            Scribe_Values.Look(ref key, "key");
            Scribe_Deep.Look(ref record, "record");
            if (typeId == null)
            {
                typeId = string.Empty;
            }
            if (key == null)
            {
                key = string.Empty;
            }
        }
    }
}
