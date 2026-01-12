# Calculated Stat Providers

A Calculated Stat Provider computes a stat's value on-demand rather than storing it. This is the mechanism behind derived stats like shot accuracy averages and trade profit.

---

## Base Class

All providers inherit from `CalculatedStatProvider` (`source/CalculatedStats/CalculatedStatProvider.cs`):

```csharp
public abstract class CalculatedStatProvider
{
    // Game-scoped overloads — no pawn context
    public virtual int   CalculateInt(string statId)   { return 0; }
    public virtual float CalculateFloat(string statId) { return 0f; }

    // Pawn-scoped overloads — receive the pawn's stats component
    public virtual int   CalculateInt(string statId, Comp_PawnStats stats)   { return CalculateInt(statId); }
    public virtual float CalculateFloat(string statId, Comp_PawnStats stats) { return CalculateFloat(statId); }
}
```

The two-argument overloads (with `Comp_PawnStats`) are called when the stat is `PAWN`-scoped. They default to delegating to the single-argument overloads, so you only need to override the ones relevant to your stat's scope and value type.

---

## Provider Cache

Providers are instantiated once and cached for the lifetime of the game by `CalculatedStatProviderCache`:

```csharp
public static CalculatedStatProvider GetOrCreate(Type providerType);
```

You do not need to call this yourself — `StatValueFetcher` handles it. Providers must have a parameterless constructor.

---

## Built-In Providers

### ColonistManualTotalStatProvider

**Used for:** auto-generated `GAME_` aggregates of non-keyed `Manual` pawn stats.

Iterates all living free colonists and sums `TotalInt` or `TotalFloat` from each pawn's `Comp_PawnStats` for the given stat ID. This is the most common provider — it backs the majority of Game-level stats.

```
GAME_KILLS          = sum of PAWN_KILLS across all colonists
GAME_CRAFTS         = sum of PAWN_CRAFTS across all colonists
```

### ColonistRecordTotalStatProvider

**Used for:** auto-generated `GAME_` aggregates of `RecordDef` pawn stats.

Same as above, but reads values from RimWorld's `pawn.records` using the registered `RecordDefName` instead of from `Comp_PawnStats`.

```
GAME_THINGS_HAULED  = sum of pawn.records["ThingsHauled"] across all colonists
```

### ColonistManualKeyedTotalStatProvider

**Used for:** auto-generated `GAME_` aggregates of keyed `Manual` pawn stats.

Sums across all colonists like the others, but also provides two extra methods for per-key aggregation:

- `CalculateKeyedIntTotals(statId)` → `Dictionary<string, int>` — per-key int totals across all colonists.
- `CalculateKeyedFloatTotals(statId)` → `Dictionary<string, float>` — per-key float totals.

```
GAME_KILLS_BY_RACE["Human"]   = sum of PAWN_KILLS_BY_RACE["Human"] across all colonists
GAME_KILLS_BY_RACE["Tribal"]  = sum of PAWN_KILLS_BY_RACE["Tribal"] across all colonists
```

### GameShotsAccuracyAverageStatProvider

**Used for:** `GAME_SHOTS_ACCURACY` only.

Computes the *average* of `PAWN_SHOTS_ACCURACY` across all living free colonists, rounded to two decimal places. This is a manually registered Game stat (not auto-generated) because averaging is not the default aggregation strategy.

### PawnTradeProfitStatProvider

**Used for:** `PAWN_TRADE_PROFIT` (pawn-scoped).

Overrides the pawn-scoped `CalculateFloat` to compute `PAWN_TRADES_EARNED - PAWN_TRADES_PAID` for the given pawn. This is a `PAWN`-scoped calculated stat — the value is derived from two other stats on the same pawn.

### GameTradeProfitStatProvider

**Used for:** `GAME_TRADE_PROFIT` (game-scoped).

Delegates to `ColonistManualTotalStatProvider` to sum `PAWN_TRADE_PROFIT` across all colonists. Since `PAWN_TRADE_PROFIT` is itself a calculated stat, this produces the correct colony-wide total without double-reading individual trade records.

---

## Writing a Custom Provider

### 1. Create the class

```csharp
using RimMetrics.CalculatedStats;
using RimMetrics.Components;

public class MyAverageStatProvider : CalculatedStatProvider
{
    public override float CalculateFloat(string statId)
    {
        var total = 0f;
        var count = 0;

        foreach (var colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists)
        {
            count++;
            if (!colonist.TryGetComp(out Comp_PawnStats comp))
                continue;

            if (comp.TryGetStat("PAWN_MY_STAT", out var record))
                total += record.TotalFloat;
        }

        return count > 0 ? total / count : 0f;
    }
}
```

### 2. Register the stat

```csharp
StatRegistry.Register(
    "GAME_MY_STAT_AVERAGE",
    StatCategory.MISC_EVENTS,
    statType: StatType.GAME,
    source: StatSource.CalculatedStat,
    statValueType: StatValueType.Float,
    calculatorType: typeof(MyAverageStatProvider),
    autoRegisterGameStat: false);
```

Key points:
- Set `statType` to `GAME` (or `PAWN` if the derivation is per-colonist).
- Set `source` to `CalculatedStat`.
- Pass your provider class as `calculatorType`.
- Set `autoRegisterGameStat: false` — the stat you're registering *is* already the Game stat (or the pawn stat you want calculated), so no further auto-generation is needed.

### 3. How it gets invoked

When the UI or any code calls `StatValueFetcher.TryGetValue()` for this stat, the fetcher:

1. Looks up the `StatMeta` from the registry.
2. Sees `source == CalculatedStat`.
3. Retrieves (or creates) the provider via `CalculatedStatProviderCache.GetOrCreate(calculatorType)`.
4. Calls `CalculateFloat(statId)` for Game stats, or `CalculateFloat(statId, comp)` for Pawn stats.

No manual storage or incrementing is involved.

---

## Choosing the Right Scope

| Your stat computes... | Use scope | Override |
|---|---|---|
| A colony-wide aggregate (sum, average, etc.) | `GAME` | `CalculateInt(statId)` or `CalculateFloat(statId)` |
| A per-pawn derived value from that pawn's other stats | `PAWN` | `CalculateInt(statId, Comp_PawnStats)` or `CalculateFloat(statId, Comp_PawnStats)` |
| A per-pawn value that requires iterating *other* pawns | `PAWN` | `CalculateInt(statId, Comp_PawnStats)` — ignore the `stats` arg and iterate yourself |
