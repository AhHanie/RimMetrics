using System.Collections.Generic;
using RimMetrics.Components;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimMetrics
{
    public class MainTabWindow_RimMetrics : MainTabWindow
    {
        private enum RimMetricsTab
        {
            ColonistStatsViewer,
            GameStatsViewer,
            ColonistLeaderboards
        }

        private const float TabHeaderHeight = 32f;
        private const float OuterPadding = 18f;
        private const float SectionSpacing = 12f;
        private const float PortraitSize = 128f;
        private const float PortraitZoom = 1.3f;
        private const float CardSpacing = TopStatsPanel.CardSpacing;

        private RimMetricsTab currentTab = RimMetricsTab.ColonistStatsViewer;
        private Pawn selectedPawn;
        private readonly TopStatsPanel topStatsPanel = new TopStatsPanel();
        private readonly StatsListPanel statsListPanel = new StatsListPanel();
        private readonly GameStatsListPanel gameStatsListPanel = new GameStatsListPanel();
        private readonly ColonistLeaderboardPanel colonistLeaderboardPanel = new ColonistLeaderboardPanel();

        public override void PostOpen()
        {
            base.PostOpen();
            statsListPanel.ResetScrollPosition();
            gameStatsListPanel.ResetScrollPosition();
            topStatsPanel.RefreshSelection(selectedPawn);
        }

        public override void DoWindowContents(Rect inRect)
        {
            var contentRect = new Rect(
                inRect.x + OuterPadding,
                inRect.y + TabHeaderHeight + OuterPadding,
                inRect.width - OuterPadding * 2f,
                inRect.height - TabHeaderHeight - OuterPadding * 2f);

            var tabs = new List<TabRecord>
            {
                new TabRecord("RimMetrics.UI.Tab.ColonistStats".Translate(), () => currentTab = RimMetricsTab.ColonistStatsViewer, currentTab == RimMetricsTab.ColonistStatsViewer),
                new TabRecord("RimMetrics.UI.Tab.GameStats".Translate(), () => currentTab = RimMetricsTab.GameStatsViewer, currentTab == RimMetricsTab.GameStatsViewer),
                new TabRecord("RimMetrics.UI.Tab.ColonistLeaderboards".Translate(), () => currentTab = RimMetricsTab.ColonistLeaderboards, currentTab == RimMetricsTab.ColonistLeaderboards)
            };

            var tabsRect = new Rect(contentRect.x, contentRect.y, contentRect.width, 45f);
            TabDrawer.DrawTabs(tabsRect, tabs);
            contentRect.yMin = tabsRect.yMax + 6f;

            switch (currentTab)
            {
                case RimMetricsTab.ColonistStatsViewer:
                    DrawColonistStatsViewer(contentRect);
                    break;
                case RimMetricsTab.GameStatsViewer:
                    DrawGameStatsViewer(contentRect);
                    break;
                case RimMetricsTab.ColonistLeaderboards:
                    DrawColonistLeaderboards(contentRect);
                    break;
            }
        }

        private void DrawColonistStatsViewer(Rect rect)
        {
            EnsureSelectedPawn();
            var selectorHeight = 30f;
            var headerRowHeight = TopStatsPanel.HeaderHeight;
            var cardSectionHeight = PortraitSize + 24f;
            var topSectionHeight = selectorHeight + SectionSpacing + headerRowHeight + Mathf.Max(PortraitSize, cardSectionHeight) + SectionSpacing;

            var selectorLabelRect = new Rect(rect.x, rect.y, 90f, selectorHeight);
            var previousAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(selectorLabelRect, "RimMetrics.UI.SelectPawn".Translate());
            Text.Anchor = previousAnchor;
            var selectorRect = new Rect(selectorLabelRect.xMax + 6f, rect.y, 260f, selectorHeight);
            DrawColonistSelector(selectorRect);
            var refreshRect = new Rect(selectorRect.xMax + 8f, rect.y, 120f, selectorHeight);
            if (refreshRect.xMax <= rect.xMax && Widgets.ButtonText(refreshRect, "RimMetrics.UI.RefreshStats".Translate()))
            {
                Current.Game?.GetComponent<GameComponent_GameStats>()?.ForceRefreshGroupedStats();
                statsListPanel.ResetScrollPosition();
            }

            var headerRowRect = new Rect(rect.x, selectorRect.yMax + SectionSpacing, rect.width, headerRowHeight);
            var portraitRect = new Rect(rect.x, headerRowRect.yMax, PortraitSize, cardSectionHeight);
            DrawPawnPortrait(portraitRect);

            var cardsRect = new Rect(
                portraitRect.xMax + CardSpacing,
                portraitRect.y,
                rect.width - portraitRect.width - CardSpacing,
                cardSectionHeight);
            topStatsPanel.Draw(headerRowRect, cardsRect, selectedPawn);

            var listRect = new Rect(rect.x, rect.y + topSectionHeight, rect.width, rect.height - topSectionHeight);
            statsListPanel.Draw(listRect, selectedPawn);
        }

        private void DrawGameStatsViewer(Rect rect)
        {
            var buttonHeight = 30f;
            var buttonWidth = 140f;
            var refreshRect = new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, buttonHeight);
            if (Widgets.ButtonText(refreshRect, "RimMetrics.UI.RefreshStats".Translate()))
            {
                Current.Game?.GetComponent<GameComponent_GameStats>()?.ForceRefreshGameGroupedStats();
                gameStatsListPanel.ResetScrollPosition();
            }

            var listRect = new Rect(rect.x, rect.y + buttonHeight + SectionSpacing, rect.width, rect.height - buttonHeight - SectionSpacing);
            gameStatsListPanel.Draw(listRect);
        }

        private void DrawColonistLeaderboards(Rect rect)
        {
            colonistLeaderboardPanel.Draw(rect, GetColonists());
        }

        private void EnsureSelectedPawn()
        {
            if (selectedPawn != null && !selectedPawn.Destroyed)
            {
                return;
            }

            var colonists = GetColonists();
            selectedPawn = colonists.Count > 0 ? colonists[0] : null;
            topStatsPanel.RefreshSelection(selectedPawn);
            statsListPanel.ResetScrollPosition();
        }

        private void DrawColonistSelector(Rect rect)
        {
            var label = selectedPawn != null ? selectedPawn.LabelCap : "RimMetrics.UI.SelectColonist".Translate().ToString();
            if (Widgets.ButtonText(rect, label))
            {
                var options = new List<FloatMenuOption>();
                foreach (var colonist in GetColonists())
                {
                    var captured = colonist;
                    options.Add(new FloatMenuOption(captured.LabelCap, () =>
                    {
                        selectedPawn = captured;
                        topStatsPanel.RefreshSelection(selectedPawn);
                        statsListPanel.ResetScrollPosition();
                    }));
                }

                if (options.Count == 0)
                {
                    options.Add(new FloatMenuOption("RimMetrics.UI.NoColonistsFound".Translate(), null));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            }
        }

        private void DrawPawnPortrait(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            if (selectedPawn == null)
            {
                return;
            }

            var size = Mathf.Min(PortraitSize, Mathf.Min(rect.width, rect.height));
            var portraitRect = new Rect(
                rect.x + (rect.width - size) / 2f,
                rect.y + (rect.height - size) / 2f,
                size,
                size);
            var portrait = PortraitsCache.Get(selectedPawn, new Vector2(size * PortraitZoom, size * PortraitZoom), Rot4.South);
            GUI.BeginGroup(portraitRect);
            var zoomedRect = new Rect((size - size * PortraitZoom) / 2f, (size - size * PortraitZoom) / 2f, size * PortraitZoom, size * PortraitZoom);
            GUI.DrawTexture(zoomedRect, portrait);
            GUI.EndGroup();
        }

        private List<Pawn> GetColonists()
        {
            return new List<Pawn>(PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists);
        }
    }
}
