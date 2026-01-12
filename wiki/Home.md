# RimMetrics

RimMetrics is a lightweight telemetry and statistics framework for RimWorld. It collects detailed metrics on pawns and game events — combat, crafting, social interactions, time spent on jobs, and much more — and presents them in a dedicated in-game tab. The framework is also designed to be consumed by other mods as a dependency.

**Package ID:** `sk.rimmetrics`
**Supported Versions:** RimWorld 1.6
**Required Dependency:** [Harmony](https://github.com/pardeike/HarmonyRimWorld) (`brrainz.harmony`)

---

## What It Tracks

| Category              | Examples                                                          |
| --------------------- | ----------------------------------------------------------------- |
| Combat                | Kills, shots fired/hit, melee attacks, headshots, friendly fire   |
| Damage & Defense      | Damage dealt/taken by type, shield absorption                     |
| Equipment             | Weapons/apparel equipped, time spent with each weapon             |
| Crafting & Production | Crafts by item/quality, market value, meals cooked                |
| Construction          | Buildings constructed/deconstructed by type and quality           |
| Work & Labor          | Trees chopped, plants harvested, cells mined, things repaired     |
| Animals               | Animals tamed, trained, slaughtered                               |
| Medical & Health      | Operations, surgeries botched, diseases, near-death events        |
| Social & Ideology     | Relationships, fights, ideology conversions, prisoner recruitment |
| Mental & Mood         | Mental states, memory thoughts, inspirations                      |
| Rituals & Abilities   | Abilities cast, psycasts used, rituals attended                   |
| Research              | Research sessions, points researched                              |
| Economy & Trade       | Trade profit, items bought/sold                                   |
| Travel & Movement     | Caravans joined, transport pods/shuttles/gravships launched       |
| Needs & Survival      | Nutrition eaten/produced, meals at/without table                  |
| Time & Activity       | Time spent on every job type, time downed, drafted, on fire       |
| Misc Events           | Incidents, quests, raids, corpses buried                          |

Stats come in two scopes: **Pawn** stats are tracked per-colonist, and **Game** stats represent colony-wide totals. Most Pawn stats automatically generate a corresponding Game-level aggregate.

---

## Setting Up as a Development Dependency

This section covers how to reference RimMetrics from another mod's C# project so you can read and write stats programmatically.

### 1. Add the Mod Dependency in `About.xml`

Declare RimMetrics as a required dependency so the mod loader ensures it is active before your mod initializes:

```xml
<modDependencies>
    <li>
        <packageId>sk.rimmetrics</packageId>
        <displayName>RimMetrics</displayName>
    </li>
</modDependencies>
```

### 2. Reference the Assembly

Add a project reference to `RimMetrics.dll`. The DLL ships inside the mod's output folder:

```
<YourModRoot>/1.6/Base/Assemblies/RimMetrics.dll
```

In your `.csproj`, add:

```xml
<Reference Include="RimMetrics">
    <HintPath>..\path\to\RimMetrics\1.6\Base\Assemblies\RimMetrics.dll</HintPath>
    <Private>False</Private>
</Reference>
```

Set `Private` to `False` — RimMetrics is loaded at runtime by the mod loader, so it must not be bundled into your own output.

### 3. Build Configuration

RimMetrics targets **.NET Framework 4.7.2**, which matches the RimWorld runtime. Ensure your project uses the same target framework. The project also depends on Harmony (`0Harmony.dll`), which is already loaded globally by the Harmony mod.

### 4. Quick Smoke Test

After referencing the assembly, verify the dependency is wired correctly by reading a stat in a Harmony `Postfix`:

```csharp
using RimMetrics;
using RimMetrics.Components;

[HarmonyPatch(typeof(SomeTargetClass), "SomeMethod")]
public static class MyPatch
{
    static void Postfix(Pawn pawn)
    {
        if (pawn.TryGetComp(out Comp_PawnStats stats))
        {
            if (stats.TryGetStat(StatIds.PAWN_KILLS, out var record))
            {
                Log.Message($"{pawn.LabelCap} has {record.TotalInt} kills.");
            }
        }
    }
}
```

If this compiles and logs correctly in-game, the dependency is set up properly. See the remaining wiki pages for the full API surface.

---

## Wiki Pages

- **[Stats — Types, Data, and Information](Stats-Types-and-Data)** — Core data model: `StatType`, `StatSource`, `StatKey`, `StatRecord`, categories, and the full stat ID catalogue.
- **[Stats Catalog](Stats-Catalog)** Complete list of stat IDs with display names and brief descriptions.
- **[Registering Stats](Registering-Stats)** — How to call `StatRegistry.Register()` and what each parameter controls.
- **[Subscribing to Events](Subscribing-to-Events)** — The `StatUpdateEvents` system and how to react to stat changes in real time.
- **[Calculated Stat Providers](Calculated-Stat-Providers)** — Writing and registering derived/computed stats via the provider pattern.
- **[Icon Selectors](Icon-Selectors)** — Controlling the icon displayed next to each stat in the UI.
- **[Value Transformers](Value-Transformers)** — Customising how raw stat values are formatted for display.
- **[Mod Compatibility](Mod-Compatibility)** — Writing compat modules: the ModCompat base class, patch guards, conditional localization, and worked examples (Infusion2, Weapon Mastery).
