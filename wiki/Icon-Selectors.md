# Icon Selectors

An Icon Selector determines which icon is displayed next to a stat in the UI. Each registered stat has an associated `IconSelectorType` that the grouping service consults when building the display list.

---

## The Abstract Base

```csharp
// source/Windows/IconSelectors/StatIconSelector.cs

public abstract class StatIconSelector
{
    public abstract bool TryGetIcon(StatMeta meta, string key, out StatIconData iconData);
}
```

| Parameter | Description |
|---|---|
| `meta` | The `StatMeta` for the stat being rendered. |
| `key` | The sub-key for keyed stats (e.g., a `ThingDef` name or a quality level). Empty string for non-keyed stats. |
| `iconData` | Output. A `StatIconData` instance if an icon was resolved, or `null` if not. |

Return `true` if an icon was successfully resolved, `false` otherwise.

---

## StatIconData

The data object returned by a selector:

```csharp
public sealed class StatIconData
{
    public readonly Texture2D Icon;     // The texture to render
    public readonly bool UseDefIcon;    // True when the icon came from a Def (affects rendering style)
    public readonly Def Def;            // The source Def, if any (used for tooltips)
    public readonly ThingDef StuffDef;  // The default stuff material, if the Def is MadeFromStuff
}
```

---

## Built-In Selectors

### SimpleIconSelector

**Default for non-keyed stats.** Returns the static `Icon` texture that was passed to `StatRegistry.Register()`. If no icon was provided at registration time, this selector returns `false` (no icon is shown).

```csharp
public sealed class SimpleIconSelector : StatIconSelector
{
    public override bool TryGetIcon(StatMeta meta, string key, out StatIconData iconData)
    {
        var icon = meta?.Icon;
        if (icon == null) { iconData = null; return false; }
        iconData = new StatIconData(icon, false, null, null);
        return true;
    }
}
```

Use this when your stat has a single, fixed icon regardless of context.

### DefIconSelector

**Default for keyed stats.** Treats the `key` string as a Def name and looks it up in RimWorld's `DefDatabase`. It checks `ThingDef` first, then `TerrainDef`. If the found `ThingDef` is `MadeFromStuff`, it also resolves the default stuff material so the icon can be rendered with correct material coloring.

```csharp
public sealed class DefIconSelector : StatIconSelector
{
    public override bool TryGetIcon(StatMeta meta, string key, out StatIconData iconData)
    {
        // Looks up ThingDef, then TerrainDef, by key name
        // Returns the Def's uiIcon with UseDefIcon = true
    }
}
```

This is appropriate for any keyed stat where the keys are `ThingDef` or `TerrainDef` names — weapons, items, floor types, etc.

### QualityIconSelector

**Used for quality-level keyed stats.** Maps the quality level name in the `key` to a bundled quality-badge texture from `ResourcesAssets`:

| Key | Icon |
|---|---|
| `"Awful"` | `QualityAwful` |
| `"Poor"` | `QualityPoor` |
| `"Normal"` | `QualityNormal` |
| `"Good"` | `QualityGood` |
| `"Excellent"` | `QualityExcellent` |
| `"Masterwork"` | `QualityMasterwork` |
| `"Legendary"` | `QualityLegendary` |

Currently used by `PAWN_CRAFTS_BY_QUALITY` and `PAWN_BUILDINGS_CONSTRUCTED_BY_QUALITY`.

---

## Resolution

Icons are resolved at display time by `StatIconSelectorResolver`:

```csharp
public static StatIconSelector GetSelector(StatMeta meta);
```

This caches selector instances by type (one instance per selector class, shared across all stats that use it). The resolver reads `meta.IconSelectorType` to determine which class to instantiate.

The default `IconSelectorType` assigned during registration is:
- `SimpleIconSelector` — when `hasKey` is `false`.
- `DefIconSelector` — when `hasKey` is `true` and no explicit `iconSelectorType` was provided.

---

## Writing a Custom Icon Selector

### 1. Create the class

```csharp
using RimMetrics;
using UnityEngine;

public sealed class MyCustomIconSelector : StatIconSelector
{
    public override bool TryGetIcon(StatMeta meta, string key, out StatIconData iconData)
    {
        // Your logic here. For example, map a key to a specific texture:
        Texture2D icon = key switch
        {
            "Fire"   => Resources.Load<Texture2D>("Textures/FireIcon"),
            "Ice"    => Resources.Load<Texture2D>("Textures/IceIcon"),
            _        => null
        };

        if (icon == null)
        {
            iconData = null;
            return false;
        }

        iconData = new StatIconData(icon, false, null, null);
        return true;
    }
}
```

### 2. Register it

Pass the type to `StatRegistry.Register()`:

```csharp
StatRegistry.Register(
    "PAWN_MY_KEYED_STAT",
    StatCategory.MISC_EVENTS,
    hasKey: true,
    iconSelectorType: typeof(MyCustomIconSelector));
```

That is all that is required. The resolver will instantiate your selector the first time it is needed and cache it.

---

## Fallback Behaviour

If a selector returns `false` (no icon resolved), the stat row is rendered without an icon. There is no secondary fallback — each stat gets exactly one selector, and if that selector cannot produce an icon for a given key, the icon slot is empty.
