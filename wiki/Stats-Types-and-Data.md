# Stats — Types, Data, and Information

This page covers every data type, enumeration, and storage mechanism that makes up a stat in RimMetrics.

---

## StatType

Defined in `StatsData.cs`. Determines the scope of a stat.

| Value | Meaning |
|---|---|
| `PAWN` | Tracked per individual pawn. Values are stored on `Comp_PawnStats`. |
| `GAME` | A colony-wide aggregate. Values are stored on `GameComponent_GameStats` or computed on-the-fly by a Calculated Stat Provider. |

Most `PAWN_` stats automatically generate a matching `GAME_` stat (see [Registering Stats](Registering-Stats) — `autoRegisterGameStat`).

---

## StatSource

Defined in `StatRegistry.cs`. Describes *where* the value for a stat originates.

| Value | Meaning | Storage |
|---|---|---|
| `Manual` | The mod or a patch explicitly increments/sets the value via `Comp_PawnStats` or `GameComponent_GameStats`. | Dictionary on the component |
| `RecordDef` | The value is read directly from RimWorld's built-in `Pawn.records` system, identified by a `RecordDefName` string. No manual increment is needed at runtime. | `pawn.records` (vanilla) |
| `CalculatedStat` | The value is computed on-demand each time it is read, by a `CalculatedStatProvider` subclass. | Not stored — computed live |

---

## StatValueType

Defined in `StatRegistry.cs`. Controls whether a stat stores an `int` or a `float`.

| Value | Use |
|---|---|
| `Int` | Default. Counters such as kills, crafts, buildings. |
| `Float` | Accumulated values like market value, nutrition, trade profit, or shot accuracy. |

---

## StatCategory

A static class of `const string` identifiers (`StatsData.cs`). Categories control grouping and display order in the UI. The 17 built-in categories, in their default display order:

| # | Constant | Displayed As |
|---|---|---|
| 1 | `COMBAT` | Combat |
| 2 | `DAMAGE_DEFENSE` | Damage & Defense |
| 3 | `EQUIPMENT` | Equipment |
| 4 | `CRAFTING_PRODUCTION` | Crafting & Production |
| 5 | `CONSTRUCTION` | Construction |
| 6 | `WORK_LABOR` | Work & Labor |
| 7 | `ANIMALS` | Animals |
| 8 | `MEDICAL_HEALTH` | Medical & Health |
| 9 | `SOCIAL_IDEOLOGY` | Social & Ideology |
| 10 | `MENTAL_MOOD` | Mental & Mood |
| 11 | `RITUALS_ABILITIES` | Rituals & Abilities |
| 12 | `RESEARCH` | Research |
| 13 | `ECONOMY_TRADE` | Economy & Trade |
| 14 | `TRAVEL_MOVEMENT` | Travel & Movement |
| 15 | `NEEDS_SURVIVAL` | Needs & Survival |
| 16 | `TIME_ACTIVITY` | Time & Activity |
| 17 | `MISC_EVENTS` | Misc Events |

Categories are managed by `StatCategoryRegistry`. Calling `StatRegistry.Register()` with a new category string automatically registers it; you can also call `StatCategoryRegistry.RegisterCategory(id, displayOrder)` explicitly to control ordering.

---

## StatKey

Defined in `StatsData.cs`. An immutable value type used as a dictionary key for stat storage. It combines two strings:

| Field | Description |
|---|---|
| `TypeId` | The stat identifier, e.g. `"PAWN_KILLS"`. |
| `Key` | An optional sub-key for *keyed stats*. Empty string for non-keyed stats. Example: a race name like `"Human"` for `PAWN_KILLS_BY_RACE`. |

`StatKey` implements `IEquatable<StatKey>` and overrides `GetHashCode` so it can be used as a dictionary key directly. It also implements `IExposable` for save/load serialisation.

---

## StatRecord

Defined in `StatsData.cs`. The value object stored for each stat on a pawn or the game component.

| Field | Type | Description |
|---|---|---|
| `TypeId` | `string` | The stat identifier. Metadata (category/source/order/etc.) is resolved from `StatRegistry` when needed. |
| `Key` | `string` | Sub-key for keyed stats; empty otherwise. |
| `TotalInt` | `int` | The accumulated integer value. |
| `TotalFloat` | `float` | The accumulated float value. |

`StatRecord` implements `IExposable`; only `TypeId`, `Key`, `TotalInt`, and `TotalFloat` are persisted across save/load cycles.

---

## StatMeta

Defined in `StatRegistry.cs`. The *registration-time* descriptor for a stat. One instance exists per registered stat, stored in the registry. This is the object returned by `StatRegistry.GetMeta(typeId)`.

| Field | Type | Description |
|---|---|---|
| `TypeId` | `string` | Unique identifier. |
| `Category` | `string` | Which `StatCategory` this stat belongs to. |
| `StatType` | `StatType` | `PAWN` or `GAME`. |
| `DisplayOrder` | `int` | Sort order within its category. |
| `Source` | `StatSource` | Where the value comes from. |
| `RecordDefName` | `string` | Vanilla record name (only when `Source == RecordDef`). |
| `StatValueType` | `StatValueType` | `Int` or `Float`. |
| `CalculatorType` | `Type` | The `CalculatedStatProvider` subclass to use (only when `Source == CalculatedStat`). |
| `HasKey` | `bool` | Whether this stat is keyed (has per-key sub-entries). |
| `Icon` | `Texture2D` | A static icon texture (used by `SimpleIconSelector`). |
| `IconSelectorType` | `Type` | The `StatIconSelector` subclass that resolves the icon. Defaults to `SimpleIconSelector` for non-keyed stats, `DefIconSelector` for keyed stats. |
| `ValueTransformerType` | `Type` | An optional `StatValueTransformer` subclass that reformats the display value. |

---

## Keyed Stats

Many stats are *keyed*: the same stat type contains multiple sub-entries, each distinguished by a string key. Examples:

| Stat | Key meaning |
|---|---|
| `PAWN_KILLS_BY_RACE` | The race name (e.g. `"Human"`, `"Tribal"`) |
| `PAWN_CRAFTS_BY_ITEM` | The `ThingDef` defName of the crafted item |
| `PAWN_DAMAGE_DEALT_BY_TYPE` | The damage type (e.g. `"Blunt"`, `"Cut"`) |
| `PAWN_CRAFTS_BY_QUALITY` | The quality level name (e.g. `"Normal"`, `"Legendary"`) |

Keyed stats are stored under the same `TypeId` but with different `Key` values in the `StatKey`. They are retrieved in bulk via `TryGetKeyedStats()` and displayed as expandable groups in the UI.

---

## Stat ID Catalogue

All built-in stat IDs are declared as constants in `StatIds.cs`. They follow a strict naming convention:

- **`PAWN_`** prefix — per-colonist stat.
- **`GAME_`** prefix — colony-wide stat.
- **`_BY_TYPE`** / **`_BY_RACE`** / **`_BY_ITEM`** etc. suffix — keyed variant.

A selection of IDs by category:

**Combat:** `PAWN_KILLS`, `PAWN_SHOTS_FIRED`, `PAWN_SHOTS_HIT`, `PAWN_SHOTS_ACCURACY`, `PAWN_MELEE_ATTACKS`, `PAWN_HEADSHOTS`, `PAWN_DOWNED`, `PAWN_KILLS_BY_RACE`, `PAWN_KILLS_BY_WEAPON_DEF`

**Damage & Defense:** `PAWN_DAMAGE_DEALT`, `PAWN_DAMAGE_TAKEN`, `PAWN_DAMAGE_DEALT_BY_TYPE`, `PAWN_SHIELD_DAMAGE_ABSORBED`

**Equipment:** `PAWN_WEAPONS_EQUIPPED`, `PAWN_APPAREL_EQUIPPED_TOTAL`, `PAWN_TIME_WEAPON_USED_BY_DEF`

**Crafting:** `PAWN_CRAFTS`, `PAWN_CRAFTS_BY_ITEM`, `PAWN_CRAFTS_BY_QUALITY`, `PAWN_CRAFTS_MARKET_VALUE`

**Time:** `PAWN_TIME_HAULING`, `PAWN_TIME_MINING`, `PAWN_TIME_DRAFTED`, `PAWN_TIME_IN_BED` (all use `TimeTicksValueTransformer`)

**Economy:** `PAWN_TRADE_PROFIT`, `PAWN_TRADES_EARNED`, `PAWN_TRADES_PAID`, `PAWN_ITEMS_BOUGHT_BY_TYPE`

**Game-only:** `GAME_TOTAL_RAIDS`, `GAME_TOTAL_INCIDENTS`, `GAME_ROOMS_BUILT`, `GAME_RESEARCH_PROJECTS_COMPLETED`, `GAME_SHOTS_ACCURACY`

---

## Storage Components

| Component | Scope | Type |
|---|---|---|
| `Comp_PawnStats` | Per pawn | `ThingComp` — attached to every pawn `ThingDef` at mod load time |
| `GameComponent_GameStats` | Per game | `GameComponent` — a singleton that lives for the lifetime of a save |

Both components store stats as `Dictionary<StatKey, StatRecord>` and maintain a secondary index (`keyedStatsByType`) for fast bulk retrieval of keyed stats. Both implement `IExposable` for full save/load support.
