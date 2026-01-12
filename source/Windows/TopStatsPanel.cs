using System;
using System.Collections.Generic;
using RimMetrics.Components;
using RimMetrics.Helpers;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimMetrics
{
    public sealed class TopStatsPanel
    {
        public const int TopStatsCount = 6;
        public const float HeaderHeight = 30f;
        public const float CardSpacing = 8f;

        private Pawn topStatsPawn;
        private readonly List<TopStatEntry> topStatsSelection = new List<TopStatEntry>();

        public void RefreshSelection(Pawn selectedPawn)
        {
            topStatsSelection.Clear();
            topStatsPawn = selectedPawn;
            if (selectedPawn == null)
            {
                return;
            }

            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return;
            }

            ColonistTopStats pawnStats = null;
            foreach (var entry in gameStats.ColonistTopStats)
            {
                if (entry.Pawn == selectedPawn)
                {
                    pawnStats = entry;
                    break;
                }
            }

            if (pawnStats == null || pawnStats.Stats.Count == 0)
            {
                return;
            }

            var pool = new List<TopStatEntry>(pawnStats.Stats);
            var count = Math.Min(TopStatsCount, pool.Count);
            for (var i = 0; i < count; i++)
            {
                var entry = pool.RandomElement();
                topStatsSelection.Add(entry);
                pool.Remove(entry);
            }

            topStatsSelection.Sort((a, b) =>
            {
                var rankCompare = GetRankPriority(a.Rank).CompareTo(GetRankPriority(b.Rank));
                if (rankCompare != 0)
                {
                    return rankCompare;
                }

                return string.CompareOrdinal(a.StatId, b.StatId);
            });
        }

        public void Draw(Rect headerRect, Rect cardsRect, Pawn selectedPawn)
        {
            DrawHeader(headerRect, cardsRect);
            DrawCards(cardsRect, selectedPawn);
        }

        private void DrawHeader(Rect headerRect, Rect cardsRect)
        {
            var iconSize = 18f;
            var labelRect = new Rect(cardsRect.x, headerRect.y, cardsRect.width, headerRect.height);
            var previousFont = Text.Font;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Medium;
            var label = "RimMetrics.UI.TopStats".Translate();
            Widgets.Label(labelRect, label);
            var labelSize = Text.CalcSize(label);
            var iconRect = new Rect(
                labelRect.center.x + labelSize.x / 2f + 4f,
                headerRect.y + (headerRect.height - iconSize) / 2f,
                iconSize,
                iconSize);
            Widgets.DrawHighlightIfMouseover(iconRect);
            GUI.DrawTexture(iconRect, TexButton.Info);
            TooltipHandler.TipRegion(iconRect, "RimMetrics.UI.TopStatsTooltip".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = previousFont;
        }

        private void DrawCards(Rect rect, Pawn selectedPawn)
        {
            if (selectedPawn == null)
            {
                return;
            }

            if (topStatsPawn != selectedPawn)
            {
                RefreshSelection(selectedPawn);
            }

            if (topStatsSelection.Count == 0)
            {
                if (IsWaitingForTopStats(selectedPawn))
                {
                    var messageRect = new Rect(rect.x, rect.y + HeaderHeight, rect.width, rect.height - HeaderHeight);
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(messageRect, "RimMetrics.UI.TopStatsWaiting".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                return;
            }

            var cardHeight = (rect.height - HeaderHeight - CardSpacing) / 2f;
            var cardWidth = (rect.width - CardSpacing * 2f) / 3f;
            for (var index = 0; index < TopStatsCount; index++)
            {
                var row = index / 3;
                var col = index % 3;
                var cardRect = new Rect(
                    rect.x + col * (cardWidth + CardSpacing),
                    rect.y + HeaderHeight + row * (cardHeight + CardSpacing),
                    cardWidth,
                    cardHeight);

                if (index < topStatsSelection.Count)
                {
                    DrawCard(cardRect, topStatsSelection[index]);
                }
                else
                {
                    DrawCardFrame(cardRect);
                }
            }
        }

        private void DrawCard(Rect rect, TopStatEntry entry)
        {
            DrawCardFrame(rect);
            var inner = rect.ContractedBy(8f);

            var nameRect = new Rect(inner.x, inner.y, inner.width - 24f, inner.height / 2f);
            var valueRect = new Rect(inner.x, inner.y + inner.height / 2f, inner.width - 24f, inner.height / 2f);
            var iconRect = new Rect(inner.xMax - 20f, inner.y, 20f, 20f);

            Text.Font = GameFont.Small;
            var statLabel = StatStringHelper.ToKeyedString(entry.StatId);
            if (Text.CalcSize(statLabel).x > nameRect.width)
            {
                Widgets.DrawHighlightIfMouseover(nameRect);
                TooltipHandler.TipRegion(nameRect, statLabel);
            }
            Widgets.Label(nameRect, statLabel);
            Widgets.Label(valueRect, GetDisplayValue(entry));

            var icon = GetRankIcon(entry.Rank);
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon);
            }
        }

        private void DrawCardFrame(Rect rect)
        {
            Widgets.DrawShadowAround(rect);
            Widgets.DrawBoxSolid(rect, new Color(0.12f, 0.12f, 0.12f, 0.9f));
            Widgets.DrawBoxSolid(rect.ContractedBy(1f), new Color(0.17f, 0.17f, 0.17f, 0.9f));
            Widgets.DrawBox(rect, 1);
        }

        private string GetDisplayValue(TopStatEntry entry)
        {
            var meta = StatRegistry.GetMeta(entry.StatId);
            var transformer = StatValueTransformerResolver.GetTransformer(meta);
            if (transformer != null
                && transformer.TryTransformToString(meta, string.Empty, entry.ValueType, entry.TotalInt, entry.TotalFloat, out var transformed))
            {
                return transformed;
            }

            return StatValueFormatter.FormatValue(entry.ValueType, entry.TotalInt, entry.TotalFloat);
        }

        private int GetRankPriority(int rank)
        {
            switch (rank)
            {
                case 1:
                    return 0;
                case 2:
                    return 1;
                case 3:
                    return 2;
                default:
                    return 3;
            }
        }

        private Texture2D GetRankIcon(int rank)
        {
            switch (rank)
            {
                case 1:
                    return ResourcesAssets.GoldStarFilled.Texture;
                case 2:
                    return ResourcesAssets.SilverStarFilled.Texture;
                case 3:
                    return ResourcesAssets.BronzeStarFilled.Texture;
                default:
                    return null;
            }
        }

        private bool IsWaitingForTopStats(Pawn selectedPawn)
        {
            if (selectedPawn == null)
            {
                return false;
            }

            var gameStats = Current.Game?.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return false;
            }

            return gameStats.ColonistTopStats.Count == 0;
        }

    }
}
