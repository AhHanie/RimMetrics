using System;
using System.Collections.Generic;
using System.Linq;
using RimMetrics.CalculatedStats;
using RimMetrics.Components;
using RimMetrics.Helpers;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimMetrics
{
    public sealed class ColonistLeaderboardPanel
    {
        private const float SectionSpacing = 12f;
        private const float LeaderboardPortraitSize = 64f;
        private const float LeaderboardBarWidth = 86f;
        private const float LeaderboardBarSpacing = 12f;
        private const float LeaderboardNameHeight = 18f;
        private const float LeaderboardValueHeight = 18f;

        private readonly List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>();
        private readonly HashSet<int> leaderboardCachedPawnIds = new HashSet<int>();
        private string leaderboardCachedStatId = string.Empty;
        private string leaderboardCachedKey = string.Empty;
        private bool leaderboardCacheReady;
        private string leaderboardSearchText = string.Empty;
        private string selectedLeaderboardStatId = string.Empty;
        private string selectedLeaderboardKey = string.Empty;
        private string selectedLeaderboardLabel = string.Empty;
        private Vector2 leaderboardScrollPos;

        public void Draw(Rect rect, List<Pawn> colonists)
        {
            var headerHeight = 26f;
            var searchRect = new Rect(rect.x, rect.y, 260f, headerHeight);
            DrawLeaderboardSearchBar(searchRect);

            var selectRect = new Rect(searchRect.xMax + 8f, rect.y, 320f, headerHeight);
            var selectLabel = string.IsNullOrWhiteSpace(selectedLeaderboardLabel)
                ? "RimMetrics.UI.SelectStat".Translate().ToString()
                : selectedLeaderboardLabel;

            if (Widgets.ButtonText(selectRect, selectLabel))
            {
                ShowLeaderboardStatMenu(colonists);
            }

            var refreshRect = new Rect(selectRect.xMax + 8f, rect.y, 120f, headerHeight);
            if (refreshRect.xMax <= rect.xMax && Widgets.ButtonText(refreshRect, "RimMetrics.UI.RefreshStats".Translate()))
            {
                BuildLeaderboardCache(colonists);
            }

            var graphRect = new Rect(rect.x, searchRect.yMax + SectionSpacing, rect.width, rect.height - headerHeight - SectionSpacing);
            if (colonists == null || colonists.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(graphRect, "RimMetrics.UI.NoColonistsFound".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedLeaderboardStatId))
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(graphRect, "RimMetrics.UI.LeaderboardSelectStatHint".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            EnsureLeaderboardCache(colonists);
            DrawLeaderboardGraph(graphRect);
        }

        private void DrawLeaderboardSearchBar(Rect rect)
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
            leaderboardSearchText = Widgets.TextField(fieldRect, leaderboardSearchText ?? string.Empty);

            Widgets.DrawHighlightIfMouseover(infoRect);
            GUI.DrawTexture(infoRect, TexButton.Info);
            TooltipHandler.TipRegion(infoRect, "RimMetrics.UI.LeaderboardSearchTooltip".Translate());
        }

        private void ShowLeaderboardStatMenu(List<Pawn> colonists)
        {
            var search = (leaderboardSearchText ?? string.Empty).Trim();
            var options = BuildLeaderboardStatOptions(colonists, search);
            if (options.Count == 0)
            {
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
                {
                    new FloatMenuOption("RimMetrics.UI.NoStatsFound".Translate(), null)
                }));
                return;
            }

            var menuOptions = new List<FloatMenuOption>(options.Count);
            foreach (var option in options)
            {
                var captured = option;
                menuOptions.Add(new FloatMenuOption(captured.Label, () =>
                {
                    SetLeaderboardSelection(captured);
                }));
            }

            Find.WindowStack.Add(new FloatMenu(menuOptions));
        }

        private List<LeaderboardStatOption> BuildLeaderboardStatOptions(List<Pawn> colonists, string search)
        {
            var results = new List<LeaderboardStatOption>();
            var metas = StatRegistry.GetAllMetas();
            foreach (var meta in metas)
            {
                if (meta.StatType != StatType.PAWN)
                {
                    continue;
                }

                if (!meta.HasKey)
                {
                    var label = StatStringHelper.ToKeyedString(meta.TypeId);
                    if (!MatchesSearch(label, search))
                    {
                        continue;
                    }

                    results.Add(new LeaderboardStatOption(meta.TypeId, string.Empty, label));
                    continue;
                }

                var keys = CollectLeaderboardKeys(meta, colonists);
                foreach (var key in keys)
                {
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    var label = StatStringHelper.ToKeyedString(meta.TypeId, key);
                    if (!MatchesSearch(label, search))
                    {
                        continue;
                    }

                    results.Add(new LeaderboardStatOption(meta.TypeId, key, label));
                }
            }

            results.Sort((a, b) => string.CompareOrdinal(a.Label, b.Label));
            return results;
        }

        private HashSet<string> CollectLeaderboardKeys(StatMeta meta, List<Pawn> colonists)
        {
            var keys = new HashSet<string>();
            if (meta == null || colonists == null || colonists.Count == 0)
            {
                return keys;
            }

            if (meta.Source == StatSource.Manual)
            {
                foreach (var pawn in colonists)
                {
                    if (!pawn.TryGetComp(out Comp_PawnStats comp))
                    {
                        continue;
                    }

                    if (comp.TryGetKeyedStats(meta.TypeId, out var keyed) && keyed != null)
                    {
                        foreach (var key in keyed.Keys)
                        {
                            keys.Add(key);
                        }
                    }
                }

                return keys;
            }

            if (meta.Source == StatSource.CalculatedStat && meta.CalculatorType != null)
            {
                var provider = CalculatedStatProviderCache.GetOrCreate(meta.CalculatorType) as ICalculatedKeyedStatProvider;
                if (provider == null)
                {
                    return keys;
                }

                foreach (var pawn in colonists)
                {
                    if (!pawn.TryGetComp(out Comp_PawnStats comp))
                    {
                        continue;
                    }

                    if (meta.StatValueType == StatValueType.Float)
                    {
                        var keyed = provider.CalculateKeyedFloatTotals(meta.TypeId, comp);
                        if (keyed == null)
                        {
                            continue;
                        }

                        foreach (var key in keyed.Keys)
                        {
                            keys.Add(key);
                        }
                    }
                    else
                    {
                        var keyed = provider.CalculateKeyedIntTotals(meta.TypeId, comp);
                        if (keyed == null)
                        {
                            continue;
                        }

                        foreach (var key in keyed.Keys)
                        {
                            keys.Add(key);
                        }
                    }
                }
            }

            return keys;
        }

        private bool MatchesSearch(string label, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            return label?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SetLeaderboardSelection(LeaderboardStatOption option)
        {
            selectedLeaderboardStatId = option.TypeId;
            selectedLeaderboardKey = option.Key ?? string.Empty;
            selectedLeaderboardLabel = option.Label ?? string.Empty;
            BuildLeaderboardCache(null);
        }

        private void EnsureLeaderboardCache(List<Pawn> colonists)
        {
            if (IsLeaderboardCacheStale(colonists))
            {
                BuildLeaderboardCache(colonists);
            }
        }

        private bool IsLeaderboardCacheStale(List<Pawn> colonists)
        {
            if (leaderboardEntries.Count == 0)
            {
                return !leaderboardCacheReady;
            }

            if (colonists == null || colonists.Count == 0)
            {
                return true;
            }

            if (!leaderboardCacheReady)
            {
                return true;
            }

            if (!string.Equals(leaderboardCachedStatId, selectedLeaderboardStatId, StringComparison.Ordinal)
                || !string.Equals(leaderboardCachedKey, selectedLeaderboardKey, StringComparison.Ordinal))
            {
                return true;
            }

            if (leaderboardCachedPawnIds.Count != colonists.Count)
            {
                return true;
            }

            foreach (var pawn in colonists)
            {
                if (!leaderboardCachedPawnIds.Contains(pawn.thingIDNumber))
                {
                    return true;
                }
            }

            return false;
        }

        private void BuildLeaderboardCache(List<Pawn> colonists)
        {
            if (colonists == null)
            {
                colonists = new List<Pawn>(PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists);
            }

            leaderboardEntries.Clear();
            leaderboardCachedPawnIds.Clear();
            leaderboardCacheReady = false;

            if (string.IsNullOrWhiteSpace(selectedLeaderboardStatId) || colonists.Count == 0)
            {
                return;
            }

            var meta = StatRegistry.GetMeta(selectedLeaderboardStatId);
            if (meta == null)
            {
                return;
            }

            foreach (var pawn in colonists)
            {
                if (!TryGetLeaderboardValue(meta, pawn, selectedLeaderboardKey, out var intValue, out var floatValue))
                {
                    continue;
                }

                var numericValue = meta.StatValueType == StatValueType.Float ? floatValue : intValue;
                var displayValue = GetLeaderboardDisplayValue(meta, selectedLeaderboardKey, meta.StatValueType, intValue, floatValue);
                leaderboardEntries.Add(new LeaderboardEntry(pawn, numericValue, displayValue));
                leaderboardCachedPawnIds.Add(pawn.thingIDNumber);
            }

            leaderboardEntries.Sort((a, b) =>
            {
                var valueComparison = b.NumericValue.CompareTo(a.NumericValue);
                if (valueComparison != 0)
                {
                    return valueComparison;
                }

                return string.CompareOrdinal(a.Pawn?.LabelShortCap ?? string.Empty, b.Pawn?.LabelShortCap ?? string.Empty);
            });

            leaderboardCachedStatId = selectedLeaderboardStatId;
            leaderboardCachedKey = selectedLeaderboardKey;
            leaderboardCacheReady = true;
        }

        private bool TryGetLeaderboardValue(StatMeta meta, Pawn pawn, string key, out int intValue, out float floatValue)
        {
            intValue = 0;
            floatValue = 0f;

            if (meta == null || pawn == null)
            {
                return false;
            }

            if (!meta.HasKey)
            {
                return StatValueFetcher.TryGetValue(meta.TypeId, pawn, out intValue, out floatValue, out _);
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (meta.Source == StatSource.Manual)
            {
                if (!pawn.TryGetComp(out Comp_PawnStats comp))
                {
                    return false;
                }

                if (!comp.TryGetStat(meta.TypeId, key, out var record) || record == null)
                {
                    return false;
                }

                if (meta.StatValueType == StatValueType.Float)
                {
                    floatValue = record.TotalFloat;
                }
                else
                {
                    intValue = record.TotalInt;
                }

                return true;
            }

            if (meta.Source == StatSource.CalculatedStat && meta.CalculatorType != null)
            {
                if (!pawn.TryGetComp(out Comp_PawnStats comp))
                {
                    return false;
                }

                var provider = CalculatedStatProviderCache.GetOrCreate(meta.CalculatorType) as ICalculatedKeyedStatProvider;
                if (provider == null)
                {
                    return false;
                }

                if (meta.StatValueType == StatValueType.Float)
                {
                    var keyed = provider.CalculateKeyedFloatTotals(meta.TypeId, comp);
                    return keyed != null && keyed.TryGetValue(key, out floatValue);
                }

                var keyedInt = provider.CalculateKeyedIntTotals(meta.TypeId, comp);
                return keyedInt != null && keyedInt.TryGetValue(key, out intValue);
            }

            return false;
        }

        private string GetLeaderboardDisplayValue(StatMeta meta, string key, StatValueType valueType, int intValue, float floatValue)
        {
            var transformer = StatValueTransformerResolver.GetTransformer(meta);
            if (transformer != null && transformer.TryTransformToString(meta, key, valueType, intValue, floatValue, out var transformed))
            {
                return transformed;
            }

            return StatValueFormatter.FormatValue(valueType, intValue, floatValue);
        }

        private void DrawLeaderboardGraph(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            if (leaderboardEntries.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "RimMetrics.UI.LeaderboardNoData".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            var maxValue = leaderboardEntries.Max(entry => entry.NumericValue);
            if (maxValue <= 0f)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect, "RimMetrics.UI.LeaderboardNoData".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            var contentRect = rect.ContractedBy(10f);
            var viewWidth = leaderboardEntries.Count * (LeaderboardBarWidth + LeaderboardBarSpacing) - LeaderboardBarSpacing;
            var viewRect = new Rect(0f, 0f, Mathf.Max(contentRect.width - 16f, viewWidth), contentRect.height);

            Widgets.BeginScrollView(contentRect, ref leaderboardScrollPos, viewRect);
            var cursorX = 0f;
            foreach (var entry in leaderboardEntries)
            {
                var columnRect = new Rect(cursorX, 0f, LeaderboardBarWidth, viewRect.height);
                DrawLeaderboardColumn(columnRect, entry, maxValue);
                cursorX += LeaderboardBarWidth + LeaderboardBarSpacing;
            }
            Widgets.EndScrollView();
        }

        private void DrawLeaderboardColumn(Rect rect, LeaderboardEntry entry, float maxValue)
        {
            if (entry?.Pawn == null)
            {
                return;
            }

            var headerHeight = LeaderboardPortraitSize + LeaderboardNameHeight + 4f;
            var headerRect = new Rect(rect.x, rect.y, rect.width, headerHeight);
            var portraitRect = new Rect(
                headerRect.x + (headerRect.width - LeaderboardPortraitSize) * 0.5f,
                headerRect.y,
                LeaderboardPortraitSize,
                LeaderboardPortraitSize);

            var portrait = PortraitsCache.Get(entry.Pawn, new Vector2(LeaderboardPortraitSize, LeaderboardPortraitSize), Rot4.South);
            GUI.DrawTexture(portraitRect, portrait);

            var nameRect = new Rect(headerRect.x, portraitRect.yMax + 2f, headerRect.width, LeaderboardNameHeight);
            var previousFont = Text.Font;
            var previousAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(nameRect, entry.Pawn.LabelShortCap);
            Text.Anchor = previousAnchor;
            Text.Font = previousFont;

            var valueRect = new Rect(rect.x, rect.yMax - LeaderboardValueHeight, rect.width, LeaderboardValueHeight);
            previousFont = Text.Font;
            previousAnchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(valueRect, entry.DisplayValue);
            Text.Anchor = previousAnchor;
            Text.Font = previousFont;

            var barRect = new Rect(rect.x, headerRect.yMax, rect.width, rect.height - headerRect.height - LeaderboardValueHeight);
            var normalized = entry.NumericValue <= 0f ? 0f : entry.NumericValue / maxValue;
            var barHeight = normalized <= 0f ? 0f : Mathf.Clamp(normalized * (barRect.height - 6f), 2f, barRect.height - 6f);
            var filledRect = new Rect(barRect.x, barRect.yMax - barHeight, barRect.width, barHeight);

            Widgets.DrawBoxSolid(barRect, new Color(0.17f, 0.18f, 0.2f, 1f));
            if (barHeight > 0f)
            {
                Widgets.DrawBoxSolid(filledRect, new Color(0.24f, 0.6f, 0.85f, 1f));
            }

            TooltipHandler.TipRegion(barRect, entry.DisplayValue);
        }

        private sealed class LeaderboardEntry
        {
            public Pawn Pawn { get; }
            public float NumericValue { get; }
            public string DisplayValue { get; }

            public LeaderboardEntry(Pawn pawn, float numericValue, string displayValue)
            {
                Pawn = pawn;
                NumericValue = numericValue;
                DisplayValue = displayValue ?? string.Empty;
            }
        }

        private sealed class LeaderboardStatOption
        {
            public string TypeId { get; }
            public string Key { get; }
            public string Label { get; }

            public LeaderboardStatOption(string typeId, string key, string label)
            {
                TypeId = typeId ?? string.Empty;
                Key = key ?? string.Empty;
                Label = label ?? string.Empty;
            }
        }
    }
}
