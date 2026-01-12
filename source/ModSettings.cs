using Verse;

namespace RimMetrics
{
    public class ModSettings : Verse.ModSettings
    {
        public static bool OnlyTrackPlayerColonists = true;
        public static bool OverrideRecordsTabWithStats = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref OnlyTrackPlayerColonists, "rimMetricsOnlyTrackPlayerColonists", true);
            Scribe_Values.Look(ref OverrideRecordsTabWithStats, "rimMetricsOverrideRecordsTabWithStats", true);
        }
    }
}
