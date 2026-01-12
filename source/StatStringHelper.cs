using RimWorld;
using Verse;

namespace RimMetrics
{
    public static class StatStringHelper
    {
        public static string ToCategoryString(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                return string.Empty;
            }

            var key = $"RimMetrics.StatCategories.{categoryId}";
            var translated = key.Translate();
            return translated == key ? categoryId : translated.ToString();
        }

        public static string ToKeyedString(string typeId)
        {
            var key = $"RimMetrics.StatTypes.{typeId}";
            return key.Translate();
        }

        public static string ToDescriptionString(string typeId)
        {
            var meta = StatRegistry.GetMeta(typeId);
            if (meta.Source == StatSource.RecordDef)
            {
                var def = DefDatabase<RecordDef>.GetNamedSilentFail(meta.RecordDefName);
                return def.description;
            }

            return $"RimMetrics.StatTypes.{typeId}.Description";
        }

        public static string ToKeyedString(string typeId, string key)
        {
            var label = ToKeyedString(typeId);
            if (string.IsNullOrWhiteSpace(key))
            {
                return label;
            }

            return $"{label}: {key}";
        }
    }
}
