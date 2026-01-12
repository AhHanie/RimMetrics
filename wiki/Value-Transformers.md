# Value Transformers

A Value Transformer intercepts a stat's raw numeric value and converts it into a custom display string before the UI renders it. If no transformer is registered for a stat, the default numeric formatter is used instead.

---

## The Abstract Base

```csharp
// source/Windows/ValueTransformers/StatValueTransformer.cs

public abstract class StatValueTransformer
{
    public virtual bool TryTransformToString(
        StatMeta meta,        // The stat's registration metadata
        string key,           // Sub-key for keyed stats; empty otherwise
        StatValueType valueType, // Int or Float
        int intValue,         // The raw int value (meaningful when valueType == Int)
        float floatValue,     // The raw float value (meaningful when valueType == Float)
        out string value)     // Output: the transformed display string
    {
        value = null;
        return false;
    }
}
```

Return `true` and set `value` to produce a custom string. Return `false` to fall through to default formatting.

---

## Built-In Transformer

### TimeTicksValueTransformer

Converts an in-game tick count into a human-readable duration string. This is the only built-in transformer and is used by every time-based stat.

```csharp
public sealed class TimeTicksValueTransformer : StatValueTransformer
{
    public override bool TryTransformToString(
        StatMeta meta, string key, StatValueType valueType,
        int intValue, float floatValue, out string value)
    {
        var ticks = valueType == StatValueType.Float ? floatValue : intValue;
        if (ticks <= 0f) { value = "0"; return true; }

        var ticksInt = ticks >= int.MaxValue ? int.MaxValue : (int)ticks;
        value = ticksInt.ToStringTicksToPeriod(
            allowSeconds: true,
            shortForm: false,
            canUseDecimals: true,
            allowYears: true,
            canUseDecimalsShortForm: false);
        return true;
    }
}
```

It delegates to RimWorld's built-in `ToStringTicksToPeriod` extension method, producing output like:

| Raw Ticks | Displayed As |
|---|---|
| 0 | `0` |
| 60000 | `1 hour` |
| 1440000 | `1 day` |
| 5760000 | `4 days` |
| 525600000 | `1 year` |

### Stats that use it

Every stat registered with `valueTransformerType: typeof(TimeTicksValueTransformer)`:

- `PAWN_TIME_AS_COLONIST_OR_COLONY_ANIMAL`
- `PAWN_TIME_AS_CHILD_IN_COLONY`
- `PAWN_TIME_AS_QUEST_LODGER`
- `PAWN_TIME_AS_PRISONER`
- `PAWN_TIME_IN_BED`
- `PAWN_TIME_IN_BED_FOR_MEDICAL_REASONS`
- `PAWN_TIME_DOWNED`
- `PAWN_TIME_DRAFTED`
- `PAWN_TIME_ON_FIRE`
- `PAWN_TIME_IN_MENTAL_STATE`
- `PAWN_TIME_GETTING_FOOD` / `JOY` / `UNDER_ROOF`
- All job-time stats (`HAULING`, `MINING`, `CONSTRUCTING`, etc.)
- `PAWN_TIME_WEAPON_USED_BY_DEF`

The corresponding auto-generated `GAME_` variants inherit the same transformer.

---

## Resolution

Transformers are resolved at display time by `StatValueTransformerResolver`:

```csharp
public static StatValueTransformer GetTransformer(StatMeta meta);
```

Like icon selectors, transformer instances are cached by type — one instance per transformer class, shared across all stats that use it. If `meta.ValueTransformerType` is `null`, the resolver returns `null` and the caller falls back to default number formatting.

---

## Writing a Custom Transformer

### 1. Create the class

```csharp
using RimMetrics;

public sealed class CurrencyValueTransformer : StatValueTransformer
{
    public override bool TryTransformToString(
        StatMeta meta, string key, StatValueType valueType,
        int intValue, float floatValue, out string value)
    {
        var amount = valueType == StatValueType.Float ? floatValue : (float)intValue;
        // Format as silver with one decimal place
        value = $"{amount:F1} silver";
        return true;
    }
}
```

### 2. Register it

```csharp
StatRegistry.Register(
    "PAWN_MY_CURRENCY_STAT",
    StatCategory.ECONOMY_TRADE,
    statValueType: StatValueType.Float,
    valueTransformerType: typeof(CurrencyValueTransformer));
```

### 3. How it is invoked

When the UI formats the stat for display, `StatValueFormatter` checks for a registered transformer via the resolver. If one is found, `TryTransformToString` is called with the raw values. If it returns `true`, the output string is used directly. If it returns `false` (or no transformer is registered), the default formatter renders the number as-is.

---

## Default Formatting (No Transformer)

When no transformer is registered:

- `Int` values are formatted with standard integer formatting.
- `Float` values are formatted with standard float formatting.

If you need different default behaviour for a specific stat without writing a full transformer class, consider whether a transformer is still the cleanest approach — transformers are lightweight and the resolver caches them, so there is no runtime cost concern.
