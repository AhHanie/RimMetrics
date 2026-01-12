# Mod Compatibility

RimMetrics has a structured system for integrating with other mods. A compatibility module registers new stats, applies Harmony patches against the target mod's types, and conditionally loads its own localization — all without affecting anything when the target mod is absent.

---

## Architecture Overview

Each compatibility integration is split across three layers:

| Layer | Responsibility | Example |
|---|---|---|
| **Compat class** | Declares stat IDs, checks whether the target mod is active, registers stats during `Init()` | `Infusion2Compat` |
| **Patches class** | Contains `[HarmonyPatch]` classes that target the other mod's types and call into `Comp_PawnStats` | `Infusion2CompatPatches` |
| **Localization folder** | A conditional `LoadFolders.xml` entry that loads translation keys only when the target mod is active | `1.6/Infusion2/` |

---

## The ModCompat Base Class

All compat modules inherit from `ModCompat` (`source/Compat/ModCompat.cs`):

```csharp
public abstract class ModCompat
{
    public abstract bool IsEnabled();
    public abstract void Init();
    public abstract string GetModPackageIdentifier();
}
```

| Method | When it runs | What it should do |
|---|---|---|
| `IsEnabled()` | During mod startup, before anything else | Return `true` if the target mod is currently active. Typically a single `ModsConfig.IsActive("package.id")` call. |
| `GetModPackageIdentifier()` | After `IsEnabled()` returns `true` | Return the target mod's package ID string. Used to look up the mod's display name for the settings panel. |
| `Init()` | After `IsEnabled()` and registration | Register all stats this compat module adds via `StatRegistry.Register()`. Apply any additional setup if needed. |

---

## Discovery and Initialization

Compat modules are found automatically at startup via reflection. `Mod.InitCompat()` scans the assembly for every non-abstract type that inherits `ModCompat`:

```csharp
// source/Mod.cs — simplified
private static void InitCompat()
{
    foreach (var type in typeof(Mod).Assembly.GetTypes())
    {
        if (type.IsAbstract || !typeof(ModCompat).IsAssignableFrom(type))
            continue;

        var compat = Activator.CreateInstance(type) as ModCompat;
        if (compat == null || !compat.IsEnabled())
            continue;

        ModCompat.RegisterCompatMod(compat); // records display name for settings
        compat.Init();                        // registers stats
    }
}
```

This runs after `Harmony.PatchAll()`, so all patches (including the compat patches) are already applied by the time `Init()` is called. The patches guard themselves independently via their own `Prepare()` methods.

---

## Patch Guards

Every patch class in a compat module must include a `Prepare()` method that returns `false` when the target mod is inactive. This is the mechanism that prevents the patch from being applied at all when the mod is not loaded:

```csharp
public static bool Prepare()
{
    return ModsConfig.IsActive("target.mod.package.id");
}
```

For patches that target types resolved by string name (common in compat code), `Prepare()` also serves as the place to verify the type actually exists before Harmony tries to patch it.

---

## Conditional Localization

Stat labels for compat modules live in their own folder under `1.6/`, and `LoadFolders.xml` gates their loading on the target mod:

```xml
<!-- LoadFolders.xml -->
<loadFolders>
  <v1.6>
    <li>1.6/Base</li>
    <li IfModActive="sk.infusion">1.6/Infusion2</li>
    <li IfModActive="sk.weaponmastery">1.6/WeaponMastery</li>
    <li>/</li>
  </v1.6>
</loadFolders>
```

Each conditional folder contains only a `Languages/English/Keyed/` subtree with the translation keys for that module's stats. The key naming convention matches the rest of the mod:

```xml
<!-- 1.6/Infusion2/Languages/English/Keyed/inf2_stat_types_pawn.xml -->
<LanguageData>
  <RimMetrics.StatTypes.PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED>Pawn infused weapons equipped (total)</RimMetrics.StatTypes.PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED>
  ...
</LanguageData>
```

---

## Existing Modules

### Infusion2 (`sk.infusion`)

**Stats registered:**

| Stat ID | Category | Keyed? | What it tracks |
|---|---|---|---|
| `PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED` | Equipment | No | Total infused weapons equipped |
| `PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED` | Equipment | No | Total infused apparel equipped |
| `PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TYPE` | Equipment | Yes (infusion name) | Infused weapons by infusion type |
| `PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TYPE` | Equipment | Yes (infusion name) | Infused apparel by infusion type |
| `PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TIER` | Equipment | Yes (tier name) | Infused weapons by tier |
| `PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TIER` | Equipment | Yes (tier name) | Infused apparel by tier |
| `PAWN_INFUSION_EFFECTS_ACTIVATED` | Combat | No | Number of times an infusion effect triggered |

All `PAWN_` stats auto-generate matching `GAME_` aggregates.

**Patching approach:** Postfix patches on `Pawn_EquipmentTracker.Notify_EquipmentAdded` and `Pawn_ApparelTracker.Notify_ApparelAdded` to record infusions when gear is equipped. Effect activation is tracked via Postfix on `CompInfusionExtensions.ForOnHitWorkers` and `ForPreHitWorkers`.

**Reflection strategy:** Infusion2's internal types (`CompInfusion`, `InfusionDef`) are not publicly exposed. The module uses a dedicated `Infusion2Reflection` helper class that:
- Resolves `CompInfusion` by type name at class-load time.
- Caches all `FieldInfo` / `PropertyInfo` lookups in static dictionaries keyed by `Type`, so reflection cost is paid once per type and amortised across all subsequent calls.
- Tries multiple property names (`InfusionsRaw` first, then `Infusions`) to remain compatible across Infusion2 versions.
- Guards everything behind a `CanResolve` flag — if the target type cannot be found, all patches silently decline via `Prepare()`.

### Weapon Mastery (`sk.weaponmastery`)

**Stats registered:**

| Stat ID | Category | What it tracks |
|---|---|---|
| `PAWN_WEAPON_BOND_MASTERIES_TOTAL` | Equipment | Bond mastery level-ups |
| `PAWN_WEAPON_CLASS_MASTERIES_TOTAL` | Equipment | Weapon-class mastery level-ups |
| `PAWN_TOTAL_WEAPONS_RENAMED` | Equipment | Weapons renamed via mastery |

All auto-generate `GAME_` aggregates.

**Patching approach:** IL transpilation. Weapon Mastery's level-up logic lives inside compiler-generated closure classes (e.g., `MasteryWeaponComp+<>c__DisplayClass15_0`). These cannot be patched with standard Prefix/Postfix because the method signatures and call graph are generated by the C# compiler. The transpiler injects calls to `RecordMasteryLevelUp` and `RecordWeaponRenamed` directly into the IL instruction stream at the correct points.

A second transpiler targets `MasteryCompData.AddExp` for class-level masteries. Because the pawn reference is only available via a captured delegate, `TryGetPawnFromDelegate` uses reflection to walk the closure's captured fields and extract it.

---

## Writing a Compat Module

### Step 1 — Create the compat class

```csharp
// source/Compat/MyMod/MyModCompat.cs
using Verse;

namespace RimMetrics
{
    public sealed class MyModCompat : ModCompat
    {
        // Declare all stat IDs as constants here so your patch class can reference them
        public const string PAWN_MY_MOD_THING = "PAWN_MY_MOD_THING";
        public const string PAWN_MY_MOD_THING_BY_TYPE = "PAWN_MY_MOD_THING_BY_TYPE";

        public override bool IsEnabled()
        {
            return ModsConfig.IsActive("author.mymod");
        }

        public override string GetModPackageIdentifier()
        {
            return "author.mymod";
        }

        public override void Init()
        {
            StatRegistry.Register(PAWN_MY_MOD_THING, StatCategory.MISC_EVENTS);
            StatRegistry.Register(PAWN_MY_MOD_THING_BY_TYPE, StatCategory.MISC_EVENTS, hasKey: true);
        }
    }
}
```

### Step 2 — Create the patches class

```csharp
// source/Compat/MyMod/MyModCompatPatches.cs
using HarmonyLib;
using RimMetrics.Components;
using Verse;

namespace RimMetrics
{
    [HarmonyPatch(typeof(SomeTargetType), "SomeMethod")]
    public static class MyModCompatPatches
    {
        // Guard: patch is skipped entirely if the mod is not active
        public static bool Prepare()
        {
            return ModsConfig.IsActive("author.mymod");
        }

        public static void Postfix(Pawn pawn, string thingName)
        {
            if (!pawn.TryGetComp(out Comp_PawnStats stats))
                return;

            stats.IncrementTotalInt(MyModCompat.PAWN_MY_MOD_THING);
            stats.IncrementTotalInt(MyModCompat.PAWN_MY_MOD_THING_BY_TYPE, thingName);
        }
    }
}
```

If the target type is not public or must be resolved by string, use `TargetMethod()` instead of the attribute:

```csharp
[HarmonyPatch]
public static class MyModCompatPatches
{
    public static MethodBase TargetMethod()
    {
        return AccessTools.Method("mymod.namespace.TargetClass:TargetMethod");
    }

    public static bool Prepare()
    {
        return ModsConfig.IsActive("author.mymod");
    }
    // ...
}
```

### Step 3 — Add the source files to the .csproj

Add `<Compile>` entries for both new files in `source/RimMetrics.csproj`:

```xml
<Compile Include="Compat\MyMod\MyModCompat.cs" />
<Compile Include="Compat\MyMod\MyModCompatPatches.cs" />
```

### Step 4 — Add localization

Create the folder structure:

```
1.6/MyMod/Languages/English/Keyed/mymod_stat_types_pawn.xml
```

```xml
<?xml version="1.0" encoding="utf-8"?>
<LanguageData>
  <RimMetrics.StatTypes.PAWN_MY_MOD_THING>My mod thing (total)</RimMetrics.StatTypes.PAWN_MY_MOD_THING>
  <RimMetrics.StatTypes.PAWN_MY_MOD_THING_BY_TYPE>My mod thing by type</RimMetrics.StatTypes.PAWN_MY_MOD_THING_BY_TYPE>
</LanguageData>
```

### Step 5 — Gate the folder in LoadFolders.xml

```xml
<li IfModActive="author.mymod">1.6/MyMod</li>
```

Add this line alongside the existing conditional entries.

---

## Choosing a Patching Strategy

| Situation | Strategy |
|---|---|
| Target method is on a public type with a stable signature | Standard `[HarmonyPatch]` attribute with Postfix or Prefix |
| Target type is internal or must be resolved at runtime | `TargetMethod()` returning `AccessTools.Method("namespace.Type:Method")` |
| Target method is a compiler-generated closure or delegate | IL Transpiler — insert `CodeInstruction` calls at the right points in the instruction stream |
| Target mod exposes no public API at all | Reflection helper class (see the Infusion2 pattern): resolve types once, cache all `FieldInfo`/`PropertyInfo`, and guard with a `CanResolve` flag |
