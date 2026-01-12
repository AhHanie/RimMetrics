using Verse;

namespace RimMetrics
{
    public static class StatCategory
    {
        public const string COMBAT = "COMBAT";
        public const string DAMAGE_DEFENSE = "DAMAGE_DEFENSE";
        public const string EQUIPMENT = "EQUIPMENT";
        public const string CRAFTING_PRODUCTION = "CRAFTING_PRODUCTION";
        public const string CONSTRUCTION = "CONSTRUCTION";
        public const string WORK_LABOR = "WORK_LABOR";
        public const string ANIMALS = "ANIMALS";
        public const string MEDICAL_HEALTH = "MEDICAL_HEALTH";
        public const string SOCIAL_IDEOLOGY = "SOCIAL_IDEOLOGY";
        public const string MENTAL_MOOD = "MENTAL_MOOD";
        public const string RITUALS_ABILITIES = "RITUALS_ABILITIES";
        public const string RESEARCH = "RESEARCH";
        public const string ECONOMY_TRADE = "ECONOMY_TRADE";
        public const string TRAVEL_MOVEMENT = "TRAVEL_MOVEMENT";
        public const string NEEDS_SURVIVAL = "NEEDS_SURVIVAL";
        public const string TIME_ACTIVITY = "TIME_ACTIVITY";
        public const string MISC_EVENTS = "MISC_EVENTS";
    }

    public enum StatType
    {
        PAWN,
        GAME
    }

    public class StatKey : IExposable, System.IEquatable<StatKey>
    {
        private string typeId;
        private string key;

        public StatKey()
        {
        }

        public StatKey(string typeId, string key = null)
        {
            this.typeId = typeId ?? string.Empty;
            this.key = key ?? string.Empty;
        }

        public string TypeId
        {
            get => typeId;
            set => typeId = value ?? string.Empty;
        }

        public string Key
        {
            get => key;
            set => key = value ?? string.Empty;
        }

        public bool Equals(StatKey other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(typeId, other.typeId) && string.Equals(key, other.key);
        }

        public override bool Equals(object obj)
        {
            return obj is StatKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((typeId != null ? typeId.GetHashCode() : 0) * 397) ^ (key != null ? key.GetHashCode() : 0);
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref typeId, "typeId");
            Scribe_Values.Look(ref key, "key");
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

    public class StatRecord : IExposable
    {
        private string typeId;
        private string key;
        private int totalInt;
        private float totalFloat;

        public StatRecord()
        {
        }

        public StatRecord(string typeId)
        {
            this.typeId = typeId ?? string.Empty;
        }

        public StatRecord(string typeId, string key)
        {
            this.typeId = typeId ?? string.Empty;
            this.key = key ?? string.Empty;
        }

        public string TypeId
        {
            get => typeId;
            set
            {
                typeId = value ?? string.Empty;
            }
        }

        public string Key
        {
            get => key;
            set => key = value ?? string.Empty;
        }

        public int TotalInt
        {
            get => totalInt;
            set => totalInt = value;
        }

        public float TotalFloat
        {
            get => totalFloat;
            set => totalFloat = value;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref typeId, "typeId");
            Scribe_Values.Look(ref key, "key");
            Scribe_Values.Look(ref totalInt, "totalInt");
            Scribe_Values.Look(ref totalFloat, "totalFloat");
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
