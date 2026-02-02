using Verse;

namespace RimMetrics
{
    public class ModSettings : Verse.ModSettings
    {
        public static bool OnlyTrackPlayerColonists = true;
        public static bool OverrideRecordsTabWithStats = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref OnlyTrackPlayerColonists, "onlyTrackPlayerColonists", true);
            Scribe_Values.Look(ref OverrideRecordsTabWithStats, "overrideRecordsTabWithStats", true);
        }
    }
}
