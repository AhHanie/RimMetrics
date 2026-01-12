# Registering Stats

All stats must be registered with `StatRegistry` before they can be tracked or displayed. Registration declares everything the system needs to know about a stat: its identity, category, data type, source, icon, and display formatting.

---

## The Registration API

```csharp
public static void Register(
    string typeId,                          // Unique stat identifier (required)
    string category = null,                 // StatCategory constant
    StatType statType = StatType.PAWN,      // PAWN or GAME
    int displayOrder = 0,                   // Sort position within the category
    StatSource source = StatSource.Manual,  // Manual, RecordDef, or CalculatedStat
    string recordDefName = null,            // Vanilla RecordDef name (when source == RecordDef)
    StatValueType statValueType = StatValueType.Int,  // Int or Float
    bool hasKey = false,                    // True for keyed (sub-entry) stats
    System.Type calculatorType = null,      // CalculatedStatProvider subclass (when source == CalculatedStat)
    UnityEngine.Texture2D icon = null,      // Static icon texture
    System.Type iconSelectorType = null,    // StatIconSelector subclass (see Icon Selectors)
    System.Type valueTransformerType = null,// StatValueTransformer subclass (see Value Transformers)
    bool autoRegisterGameStat = true)       // Auto-create a GAME_ aggregate (see below)
```

---

## Parameter Details

### `typeId`
The unique string identifier for the stat. By convention, use `PAWN_` or `GAME_` prefixes. This string is what patches use to increment values and what the UI uses to look up labels (via localization keys).

### `category`
One of the `StatCategory` constants (e.g., `StatCategory.COMBAT`). If you pass a category string that has not been seen before, it is automatically registered with `StatCategoryRegistry` and appended to the end of the category list. To control its position explicitly, call `StatCategoryRegistry.RegisterCategory(id, displayOrder)` before or after.

### `statType`
`StatType.PAWN` for per-colonist stats, `StatType.GAME` for colony-wide stats. Game stats that are manually tracked (not aggregated from pawn stats) must be registered with `statType: StatType.GAME` and `autoRegisterGameStat: false`.

### `displayOrder`
Controls the vertical ordering of stats within a single category. Lower numbers appear first. Defaults to `0`, meaning all stats at the same order are then sorted alphabetically by `typeId`.

### `source`
Controls how the value is retrieved at read time:

- **`Manual`** — the value is written by your patch code via `IncrementTotalInt`, `IncrementTotalFloat`, or `SetTotalFloat`.
- **`RecordDef`** — the value is read from RimWorld's `pawn.records` system. Provide the `recordDefName` parameter. No runtime incrementing is required; the value is fetched directly from the game engine.
- **`CalculatedStat`** — the value is computed on-demand by a `CalculatedStatProvider`. Provide the `calculatorType` parameter.

### `recordDefName`
The string name of the vanilla `RecordDef` to read from. Only used when `source == RecordDef`. Examples: `"Kills"`, `"ShotsFired"`, `"ThingsHauled"`.

### `statValueType`
`Int` (default) for integer counters, `Float` for decimal values like market value, nutrition, or accuracy ratios.

### `hasKey`
Set to `true` if this stat contains per-key sub-entries (e.g., kills broken down by race, crafts broken down by item). Keyed stats are stored under the same `typeId` but with different `Key` values in the `StatKey`. They are displayed as expandable groups in the UI.

When `hasKey` is `true` and no `iconSelectorType` is specified, the default icon selector changes from `SimpleIconSelector` to `DefIconSelector`, which attempts to look up an icon from the key's `ThingDef` or `TerrainDef`.

### `calculatorType`
A `Type` that is a subclass of `CalculatedStatProvider`. Required when `source == CalculatedStat`. The provider is instantiated once and cached for the lifetime of the game. See [Calculated Stat Providers](Calculated-Stat-Providers).

### `icon`
A static `Texture2D` icon. Used by `SimpleIconSelector` for non-keyed stats. For keyed stats or stats that need context-dependent icons, use `iconSelectorType` instead.

### `iconSelectorType`
A `Type` that is a subclass of `StatIconSelector`. Overrides the default icon resolution logic. See [Icon Selectors](Icon-Selectors).

### `valueTransformerType`
A `Type` that is a subclass of `StatValueTransformer`. When set, the formatter calls this transformer to convert the raw numeric value into a display string before falling back to default formatting. See [Value Transformers](Value-Transformers).

### `autoRegisterGameStat`
When `true` (the default) and the stat being registered is a `PAWN` stat with a `Manual` or `RecordDef` source, the registry *automatically* creates a corresponding `GAME_` stat. The auto-generated Game stat:

- Has its `typeId` derived by replacing the `PAWN_` prefix with `GAME_`.
- Uses `source: CalculatedStat`.
- Is assigned a calculator that sums the stat across all living free colonists:
  - `Manual` + non-keyed → `ColonistManualTotalStatProvider`
  - `Manual` + keyed → `ColonistManualKeyedTotalStatProvider`
  - `RecordDef` → `ColonistRecordTotalStatProvider`

Set `autoRegisterGameStat: false` when:
- The stat is already a `GAME`-scoped stat.
- The stat is a `CalculatedStat` (aggregation is already handled by the calculator).
- You need a custom aggregation strategy and will register the Game stat yourself.

---

## Examples

### Simple manual counter

```csharp
StatRegistry.Register(
    "PAWN_MY_CUSTOM_EVENT",
    StatCategory.MISC_EVENTS);
// Automatically creates GAME_MY_CUSTOM_EVENT as a sum across colonists.
```

Increment it in a patch:

```csharp
if (pawn.TryGetComp(out Comp_PawnStats stats))
{
    stats.IncrementTotalInt("PAWN_MY_CUSTOM_EVENT");
}
```

### Float stat backed by a vanilla record

```csharp
StatRegistry.Register(
    StatIds.PAWN_NUTRITION_EATEN,
    StatCategory.NEEDS_SURVIVAL,
    source: StatSource.RecordDef,
    recordDefName: "NutritionEaten");
```

No runtime code is needed to track this — the value is read directly from the pawn's records.

### Keyed stat with a custom icon selector

```csharp
StatRegistry.Register(
    StatIds.PAWN_CRAFTS_BY_QUALITY,
    StatCategory.CRAFTING_PRODUCTION,
    hasKey: true,
    iconSelectorType: typeof(QualityIconSelector));
```

Each sub-entry's key is a quality level name (`"Normal"`, `"Legendary"`, etc.), and `QualityIconSelector` maps those to the appropriate quality badge icon.

### Manually registered Game stat with a custom calculator

```csharp
StatRegistry.Register(
    StatIds.GAME_SHOTS_ACCURACY,
    StatCategory.COMBAT,
    statType: StatType.GAME,
    source: StatSource.CalculatedStat,
    statValueType: StatValueType.Float,
    calculatorType: typeof(GameShotsAccuracyAverageStatProvider),
    autoRegisterGameStat: false);
```

This registers a Game stat whose value is the *average* shot accuracy across all colonists, computed live by `GameShotsAccuracyAverageStatProvider`.

### Time-based stat with a value transformer

```csharp
StatRegistry.Register(
    StatIds.PAWN_TIME_HAULING,
    StatCategory.TIME_ACTIVITY,
    source: StatSource.RecordDef,
    recordDefName: "TimeHauling",
    valueTransformerType: typeof(TimeTicksValueTransformer));
```

The raw value is in game ticks. `TimeTicksValueTransformer` converts it to a human-readable duration like `"3 days, 2 hours"` in the UI.

---

## When to Register

Stats are registered in two ways in the codebase:

1. **Built-in stats** — registered in the `StatRegistry` static constructor via `RegisterBuiltIns()`. This runs the first time the class is accessed, which happens during mod initialization.
2. **Compatibility mod stats** — registered inside the `Init()` method of a `ModCompat` subclass. This runs after Harmony patching, so compatibility mods can conditionally register stats only when the target mod is active.

If you are writing a separate mod that depends on RimMetrics, register your stats during your own mod's `Init` / `LongEventHandler` callback — the `StatRegistry` class will already be available at that point.
