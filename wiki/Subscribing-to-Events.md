# Subscribing to Events

RimMetrics exposes a single event that fires every time any stat value changes. This lets external mods react to stat updates without polling.

---

## StatUpdateEvents

Defined in `StatUpdateEvents.cs`.

```csharp
public static class StatUpdateEvents
{
    public static event Action<StatUpdateEvent> StatUpdated;
}
```

`StatUpdated` fires whenever a `Manual`-source stat is written to on either `Comp_PawnStats` or `GameComponent_GameStats`. It does **not** fire for `RecordDef` stats (those are read directly from the pawn and are never "written" by RimMetrics) or for `CalculatedStat` stats (those are computed on-demand).

---

## StatUpdateEvent

The event payload is a lightweight readonly struct:

```csharp
public readonly struct StatUpdateEvent
{
    public readonly float Delta;      // The change in value (positive or negative)
    public readonly StatRecord Record; // The StatRecord that was modified
}
```

| Field | Description |
|---|---|
| `Delta` | The amount the stat changed by. For `IncrementTotalInt` and `IncrementTotalFloat` this equals the `amount` argument. For `SetTotalFloat` it equals `newValue - previousValue`. |
| `Record` | The full `StatRecord` after the update. Use `Record.TypeId` to identify the stat, `Record.Key` to identify the sub-entry (for keyed stats), and `Record.TotalInt` / `Record.TotalFloat` for the new cumulative value. |

---

## Subscribing

Subscribe to `StatUpdateEvents.StatUpdated` like any standard C# event:

```csharp
using RimMetrics;

public static class MyStatListener
{
    public static void Initialize()
    {
        StatUpdateEvents.StatUpdated += OnStatUpdated;
    }

    private static void OnStatUpdated(StatUpdateEvent evt)
    {
        // Filter to a specific stat
        if (evt.Record.TypeId == StatIds.PAWN_KILLS)
        {
            Log.Message($"A colonist got a kill! Total now: {evt.Record.TotalInt}");
        }
    }
}
```

Call `Initialize()` from your mod's startup (e.g., inside a `LongEventHandler` callback or a Harmony `Postfix` on mod loading).

### Unsubscribing

Unsubscribe when your mod or listener is no longer needed to avoid memory leaks:

```csharp
StatUpdateEvents.StatUpdated -= OnStatUpdated;
```

---

## When the Event Fires

The event is raised internally by the following methods on both `Comp_PawnStats` and `GameComponent_GameStats`:

| Method | Delta value |
|---|---|
| `IncrementTotalInt(typeId, amount)` | `amount` (cast to float) |
| `IncrementTotalInt(typeId, key, amount)` | `amount` (cast to float) |
| `IncrementTotalFloat(typeId, amount)` | `amount` |
| `IncrementTotalFloat(typeId, key, amount)` | `amount` |
| `SetTotalFloat(typeId, value)` | `value - previousValue` |
| `SetTotalFloat(typeId, key, value)` | `value - previousValue` |

The event fires *after* the value has been updated on the record, so `Record.TotalInt` and `Record.TotalFloat` reflect the new state.

---

## Identifying the Source

The `StatRecord` in the event carries full metadata. You can use it to determine scope and category without additional lookups:

```csharp
private static void OnStatUpdated(StatUpdateEvent evt)
{
    var record = evt.Record;

    if (record.StatType == StatType.PAWN && record.Category == StatCategory.COMBAT)
    {
        // A pawn combat stat changed
    }

    // For keyed stats, record.Key is non-empty
    if (!string.IsNullOrEmpty(record.Key))
    {
        // This is a keyed sub-entry update
    }
}
```

For deeper introspection (e.g., to find the calculator type or icon selector), call `StatRegistry.GetMeta(record.TypeId)` to retrieve the full `StatMeta`.
