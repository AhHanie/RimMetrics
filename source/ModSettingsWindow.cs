using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimMetrics
{
    public static class ModSettingsWindow
    {
        private static Vector2 scrollPosition = Vector2.zero;
        private static float lastContentHeight;

        public static void Draw(Rect parent)
        {
            var viewWidth = parent.width - 16f;
            var viewRect = new Rect(0f, 0f, viewWidth, Mathf.Max(parent.height, lastContentHeight));
            Widgets.BeginScrollView(parent, ref scrollPosition, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            listing.CheckboxLabeled(
                "RimMetrics.Settings.OnlyTrackPlayerColonists".Translate(),
                ref ModSettings.OnlyTrackPlayerColonists);
            listing.CheckboxLabeled(
                "RimMetrics.Settings.OverrideRecordsTabWithStats".Translate(),
                ref ModSettings.OverrideRecordsTabWithStats);
            listing.Label("RimMetrics.Settings.RestartRequiredNotice".Translate());
            listing.GapLine();

            var compatHeaderRect = listing.GetRect(Text.LineHeight);
            Widgets.Label(compatHeaderRect, "RimMetrics.Settings.CompatHeader".Translate());
            TooltipHandler.TipRegion(compatHeaderRect, "RimMetrics.Settings.CompatHeaderTooltip".Translate());
            var compatMods = ModCompat.GetRegisteredCompatModNames();
            if (compatMods.Count == 0)
            {
                listing.Label("RimMetrics.Settings.CompatNone".Translate());
            }
            else
            {
                foreach (var name in compatMods)
                {
                    listing.Label("- " + name);
                }
            }

            listing.End();
            Widgets.EndScrollView();
            lastContentHeight = listing.CurHeight + 10f;
        }
    }
}
