using System;
using System.Collections.Generic;
using RimMetrics.Components;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimMetrics
{
    public sealed class StatsListPanel
    {
        private const float SearchBarWidth = 260f;
        private const float SectionSpacing = 12f;
        private const float CategoryHeaderHeight = 30f;
        private const float CategorySummaryHeight = 16f;
        private const float GroupHeaderHeight = 24f;
        private const float StatRowHeight = 26f;
        private const float StatRowSpacing = 6f;
        private const float CategoryPadding = 6f;
        private const float CategorySpacing = 12f;
        private const float GroupIndent = 28f;
        private const float GroupChildrenPadding = 4f;
        private const float StatIconSize = 36f;
        private const float StatIconPadding = 4f;

        private static readonly Color CategoryPanelColor = new Color(0.16f, 0.165f, 0.176f, 0.95f);
        private static readonly Color CategoryHeaderColor = new Color(0.16f, 0.17f, 0.18f, 1f);
        private static readonly Color DividerColor = new Color(0.27f, 0.28f, 0.3f, 1f);
        private static readonly Color SearchHighlightColor = new Color(0.9f, 0.82f, 0.2f, 0.28f);
        private static readonly Color CategoryTitleColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color StatTextColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ValueTextColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color ChildStatTextColor = new Color(1f, 1f, 1f, 1f);
        private static readonly Color SummaryTextColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        private string searchText = string.Empty;
        private Vector2 statsScrollPos;
        private readonly Dictionary<string, bool> categoryExpanded = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> groupExpanded = new Dictionary<string, bool>();

        public void ResetScrollPosition()
        {
            statsScrollPos = Vector2.zero;
        }

        public void Draw(Rect rect, Pawn selectedPawn)
        {
            if (selectedPawn == null)
            {
                Widgets.Label(rect, "RimMetrics.UI.SelectColonistToView".Translate());
                return;
            }

            var searchRect = new Rect(rect.x, rect.y, SearchBarWidth, 24f);
            var buttonsWidth = 220f;
            if (rect.width > SearchBarWidth + buttonsWidth + 12f)
            {
                var buttonsRect = new Rect(rect.xMax - buttonsWidth, rect.y, buttonsWidth, 24f);
                DrawExpandCollapseButtons(buttonsRect, selectedPawn);
            }
            DrawSearchBar(searchRect);

            var listRect = new Rect(rect.x, searchRect.yMax + SectionSpacing, rect.width, rect.height - searchRect.height - SectionSpacing);
            var groupedStats = GetGroupedStats(selectedPawn);
            if (groupedStats.Count == 0 && IsWaitingForGroupedStats())
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(listRect, "RimMetrics.UI.GroupedStatsWaiting".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }
            var viewHeight = CalculateStatsViewHeight(groupedStats);
            var viewRect = new Rect(0f, 0f, listRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(listRect, ref statsScrollPos, viewRect);
            var cursor = 0f;
            DrawStatsGroups(ref cursor, viewRect, groupedStats);
            Widgets.EndScrollView();
        }

        private void DrawExpandCollapseButtons(Rect rect, Pawn selectedPawn)
        {
            var buttonWidth = (rect.width - 6f) / 2f;
            var collapseRect = new Rect(rect.x, rect.y, buttonWidth, rect.height);
            var expandRect = new Rect(collapseRect.xMax + 6f, rect.y, buttonWidth, rect.height);

            if (Widgets.ButtonText(collapseRect, "RimMetrics.UI.CollapseAll".Translate()))
            {
                SetAllExpanded(selectedPawn, false);
            }

            if (Widgets.ButtonText(expandRect, "RimMetrics.UI.ExpandAll".Translate()))
            {
                SetAllExpanded(selectedPawn, true);
            }
        }

        private void DrawSearchBar(Rect rect)
        {
            var iconSize = rect.height;
            var iconRect = new Rect(rect.x, rect.y, iconSize, iconSize);
            var labelRect = new Rect(iconRect.xMax + 4f, rect.y, 60f, rect.height);
            var infoRect = new Rect(rect.xMax - iconSize, rect.y, iconSize, iconSize);
            var fieldRect = new Rect(labelRect.xMax + 6f, rect.y, rect.width - iconSize * 2f - 76f, rect.height);

            GUI.DrawTexture(iconRect, TexButton.Search);
            var previousAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, "RimMetrics.UI.Search".Translate());
            Text.Anchor = previousAnchor;
            searchText = Widgets.TextField(fieldRect, searchText ?? string.Empty);

            Widgets.DrawHighlightIfMouseover(infoRect);
            GUI.DrawTexture(infoRect, TexButton.Info);
            TooltipHandler.TipRegion(infoRect, "RimMetrics.UI.StatsListSearchTooltip".Translate());
        }

        private void DrawStatsGroups(ref float cursor, Rect viewRect, List<StatsGroupingService.CategoryGroup> grouped)
        {
            var search = (searchText ?? string.Empty).Trim();
            var hasSearch = search.Length > 0;

            foreach (var category in grouped)
            {
                if (hasSearch && !StatsGroupingService.CategoryMatchesSearch(category, search))
                {
                    continue;
                }

                var categoryKey = category.Category;
                var expanded = GetExpanded(categoryExpanded, categoryKey, true);
                var categoryHeight = CalculateCategoryBlockHeight(category, search, hasSearch);
                var categoryRect = new Rect(viewRect.x, cursor, viewRect.width, categoryHeight);
                Widgets.DrawBoxSolid(categoryRect, CategoryPanelColor);

                var contentRect = categoryRect.ContractedBy(CategoryPadding);
                var headerRect = new Rect(contentRect.x, contentRect.y, contentRect.width, CategoryHeaderHeight);
                Widgets.DrawBoxSolid(headerRect, CategoryHeaderColor);
                Widgets.DrawHighlightIfMouseover(headerRect);
                if (Widgets.ButtonInvisible(headerRect))
                {
                    categoryExpanded[categoryKey] = !expanded;
                }

                var previousFont = Text.Font;
                var previousAnchor = Text.Anchor;
                var previousColor = GUI.color;
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleLeft;
                var arrowRect = new Rect(headerRect.xMax - 20f, headerRect.y + (headerRect.height - 16f) / 2f, 16f, 16f);
                var labelRect = new Rect(headerRect.x + 8f, headerRect.y, headerRect.width - 28f, headerRect.height);
                var labelText = StatStringHelper.ToCategoryString(categoryKey);
                GUI.color = CategoryTitleColor;
                Widgets.Label(labelRect, labelText);
                GUI.DrawTexture(arrowRect, expanded ? TexButton.Collapse : TexButton.Reveal);
                GUI.color = DividerColor;
                Widgets.DrawLineHorizontal(headerRect.x, headerRect.yMax, headerRect.width);
                Text.Anchor = previousAnchor;
                Text.Font = previousFont;
                GUI.color = previousColor;

                var summaryRect = new Rect(contentRect.x + 8f, headerRect.yMax, contentRect.width - 16f, CategorySummaryHeight);
                previousFont = Text.Font;
                previousAnchor = Text.Anchor;
                previousColor = GUI.color;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = SummaryTextColor;
                Widgets.Label(summaryRect, "RimMetrics.UI.CategoryStatsCount".Translate(StatsGroupingService.GetCategoryTotalCount(category)));
                Text.Anchor = previousAnchor;
                Text.Font = previousFont;
                GUI.color = previousColor;

                var localCursor = summaryRect.yMax + 6f;

                if (!expanded)
                {
                    cursor += categoryRect.height + CategorySpacing;
                    continue;
                }

                var categoryRowIndex = 0;
                DrawUngroupedRows(ref localCursor, contentRect, category.Rows, search, hasSearch, ref categoryRowIndex);
                DrawGroupedRows(ref localCursor, contentRect, category, search, hasSearch, ref categoryRowIndex);

                cursor += categoryRect.height + CategorySpacing;
            }
        }

        private void DrawUngroupedRows(ref float cursor, Rect viewRect, List<StatsGroupingService.StatRow> rows, string search, bool hasSearch, ref int rowIndex)
        {
            foreach (var row in rows)
            {
                if (hasSearch && row.Label.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                DrawStatRow(ref cursor, viewRect, row, 0f, rowIndex, search, false);
                rowIndex++;
            }
        }

        private void DrawGroupedRows(
            ref float cursor,
            Rect viewRect,
            StatsGroupingService.CategoryGroup category,
            string search,
            bool hasSearch,
            ref int rowIndex)
        {
            foreach (var group in category.Groups)
            {
                if (hasSearch && !StatsGroupingService.GroupMatchesSearch(group.Value, search))
                {
                    continue;
                }

                var groupKey = $"{category.Category}:{group.Key}";
                var groupExpandedValue = GetExpanded(groupExpanded, groupKey, false);
                var headerRect = new Rect(viewRect.x, cursor, viewRect.width, GroupHeaderHeight);
                Widgets.DrawHighlightIfMouseover(headerRect);
                if (Widgets.ButtonInvisible(headerRect))
                {
                    groupExpanded[groupKey] = !groupExpandedValue;
                }

                var previousFont = Text.Font;
                var previousAnchor = Text.Anchor;
                var previousColor = GUI.color;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                var arrowRect = new Rect(headerRect.xMax - 18f, headerRect.y + (headerRect.height - 16f) / 2f, 16f, 16f);
                var labelRect = new Rect(headerRect.x + 6f, headerRect.y, headerRect.width - 110f, headerRect.height);
                var countRect = new Rect(headerRect.xMax - 80f, headerRect.y, 60f, headerRect.height);
                GUI.color = Color.white;
                DrawLabelWithSearchHighlight(labelRect, group.Key, search);
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(countRect, "RimMetrics.UI.GroupCount".Translate(group.Value.Count));
                GUI.DrawTexture(arrowRect, groupExpandedValue ? TexButton.Collapse : TexButton.Reveal);
                Text.Anchor = previousAnchor;
                Text.Font = previousFont;
                GUI.color = previousColor;

                cursor += headerRect.height + 4f;
                rowIndex++;

                if (!groupExpandedValue)
                {
                    continue;
                }

                var groupRowIndex = 0;
                foreach (var row in group.Value)
                {
                    if (hasSearch && row.Label.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        continue;
                    }

                    DrawStatRow(ref cursor, viewRect, row, GroupIndent + GroupChildrenPadding + 12f, groupRowIndex, search, true);
                    groupRowIndex++;
                }
            }
        }

        private void DrawStatRow(
            ref float cursor,
            Rect viewRect,
            StatsGroupingService.StatRow row,
            float indent,
            int rowIndex,
            string search,
            bool isChild = false)
        {
            var rowRect = new Rect(viewRect.x + indent, cursor, viewRect.width - indent, StatRowHeight);
            Widgets.DrawHighlightIfMouseover(rowRect);

            var bulletOffset = isChild ? 10f : 0f;
            if (isChild)
            {
                var bulletRect = new Rect(rowRect.x + 6f, rowRect.y, 10f, rowRect.height);
                var previousBulletColor = GUI.color;
                GUI.color = ChildStatTextColor;
                Widgets.Label(bulletRect, "-");
                GUI.color = previousBulletColor;
            }

            var labelRect = new Rect(rowRect.x + 8f + bulletOffset, rowRect.y, rowRect.width - 86f - bulletOffset, rowRect.height);
            var valueRect = new Rect(rowRect.xMax - 72f, rowRect.y, 68f, rowRect.height);

            var previousAnchor = Text.Anchor;
            var previousColor = GUI.color;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = isChild ? ChildStatTextColor : StatTextColor;
            DrawStatLabel(labelRect, row, search);
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = isChild ? ChildStatTextColor : ValueTextColor;
            Widgets.Label(valueRect, GetDisplayValue(row));
            Text.Anchor = previousAnchor;
            GUI.color = previousColor;

            cursor += rowRect.height + StatRowSpacing;
        }

        private void DrawStatLabel(Rect rect, StatsGroupingService.StatRow row, string search)
        {
            var labelRect = rect;
            if (row.IconData != null)
            {
                var labelWidth = Mathf.Min(Text.CalcSize(row.Label).x, rect.width);
                var iconX = rect.x + Mathf.Min(labelWidth + StatIconPadding, rect.width - StatIconSize);
                var iconRect = new Rect(
                    iconX,
                    rect.y + (rect.height - StatIconSize) / 2f,
                    StatIconSize,
                    StatIconSize);
                DrawStatIcon(iconRect, row.IconData);
            }

            DrawLabelWithSearchHighlight(labelRect, row.Label, search);
        }

        private void DrawLabelWithSearchHighlight(Rect rect, string label, string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                Widgets.Label(rect, label);
                return;
            }

            var matchIndex = label.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                Widgets.Label(rect, label);
                return;
            }

            var prefix = label.Substring(0, matchIndex);
            var match = label.Substring(matchIndex, search.Length);
            var prefixWidth = Text.CalcSize(prefix).x;
            var matchWidth = Text.CalcSize(match).x;
            var highlightRect = new Rect(rect.x + prefixWidth, rect.y, matchWidth, rect.height);
            Widgets.DrawBoxSolid(highlightRect, SearchHighlightColor);
            Widgets.Label(rect, label);
        }

        private string GetDisplayValue(StatsGroupingService.StatRow row)
        {
            var value = row.Value;
            var meta = StatRegistry.GetMeta(row.TypeId);
            var transformer = StatValueTransformerResolver.GetTransformer(meta);
            if (transformer != null && transformer.TryTransformToString(meta, row.Key, row.ValueType, row.TotalInt, row.TotalFloat, out var transformed))
            {
                return transformed;
            }
            return value;
        }

        private float CalculateStatsViewHeight(List<StatsGroupingService.CategoryGroup> grouped)
        {
            var search = (searchText ?? string.Empty).Trim();
            var hasSearch = search.Length > 0;
            var height = 0f;

            foreach (var category in grouped)
            {
                if (hasSearch && !StatsGroupingService.CategoryMatchesSearch(category, search))
                {
                    continue;
                }

                height += CalculateCategoryBlockHeight(category, search, hasSearch) + CategorySpacing;
            }

            return Math.Max(height + 10f, 40f);
        }

        private float CalculateRowsHeight(List<StatsGroupingService.StatRow> rows, string search, bool hasSearch)
        {
            var count = 0;
            foreach (var row in rows)
            {
                if (hasSearch && row.Label.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                count++;
            }

            if (count == 0)
            {
                return 0f;
            }

            return count * (StatRowHeight + StatRowSpacing);
        }

        private float CalculateCategoryBlockHeight(StatsGroupingService.CategoryGroup category, string search, bool hasSearch)
        {
            var height = CategoryHeaderHeight + CategorySummaryHeight + CategoryPadding * 2f;
            var expanded = GetExpanded(categoryExpanded, category.Category, true);
            if (!expanded)
            {
                return height;
            }

            height += CalculateRowsHeight(category.Rows, search, hasSearch);
            foreach (var group in category.Groups)
            {
                height += GroupHeaderHeight + 4f;
                var groupKey = $"{category.Category}:{group.Key}";
                if (!GetExpanded(groupExpanded, groupKey, false))
                {
                    continue;
                }

                height += CalculateRowsHeight(group.Value, search, hasSearch);
            }

            height += 8f;
            return height;
        }

        private List<StatsGroupingService.CategoryGroup> GetGroupedStats(Pawn pawn)
        {
            if (pawn == null)
            {
                return new List<StatsGroupingService.CategoryGroup>();
            }

            var gameStats = Current.Game?.GetComponent<GameComponent_GameStats>();
            if (gameStats == null)
            {
                return new List<StatsGroupingService.CategoryGroup>();
            }

            return gameStats.TryGetGroupedStats(pawn, out var groupedStats)
                ? groupedStats
                : new List<StatsGroupingService.CategoryGroup>();
        }

        private bool IsWaitingForGroupedStats()
        {
            var gameStats = Current.Game?.GetComponent<GameComponent_GameStats>();
            return gameStats != null && gameStats.IsWaitingForGroupedStats();
        }

        private void DrawStatIcon(Rect rect, StatIconData iconData)
        {
            if (iconData == null)
            {
                return;
            }

            if (iconData.UseDefIcon && iconData.Def != null)
            {
                if (iconData.Def is ThingDef thingDef)
                {
                    Widgets.DefIcon(rect, thingDef, iconData.StuffDef);
                }
                else
                {
                    Widgets.DefIcon(rect, iconData.Def);
                }

                return;
            }

            if (iconData.Icon != null)
            {
                GUI.DrawTexture(rect, iconData.Icon);
            }
        }

        private bool GetExpanded<T>(Dictionary<T, bool> map, T key, bool defaultValue)
        {
            if (!map.TryGetValue(key, out var expanded))
            {
                expanded = defaultValue;
                map[key] = expanded;
            }

            return expanded;
        }

        private void SetAllExpanded(Pawn pawn, bool expanded)
        {
            var groupedStats = GetGroupedStats(pawn);
            foreach (var category in groupedStats)
            {
                categoryExpanded[category.Category] = expanded;
                foreach (var group in category.Groups.Keys)
                {
                    var groupKey = $"{category.Category}:{group}";
                    groupExpanded[groupKey] = expanded;
                }
            }
        }
    }
}
