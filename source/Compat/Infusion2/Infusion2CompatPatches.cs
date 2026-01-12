using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentAdded")]
    public static class Infusion2WeaponEquippedPatch
    {
        public static bool Prepare()
        {
            return ModsConfig.IsActive("sk.infusion") && Infusion2Reflection.CanResolve;
        }

        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            Infusion2CompatHelpers.RecordInfusions(
                comp,
                eq,
                Infusion2Compat.PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED,
                Infusion2Compat.PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TYPE,
                Infusion2Compat.PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TIER);
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    public static class Infusion2ApparelEquippedPatch
    {
        public static bool Prepare()
        {
            return ModsConfig.IsActive("sk.infusion") && Infusion2Reflection.CanResolve;
        }

        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            Infusion2CompatHelpers.RecordInfusions(
                comp,
                apparel,
                Infusion2Compat.PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED,
                Infusion2Compat.PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TYPE,
                Infusion2Compat.PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TIER);
        }
    }

    [HarmonyPatch]
    public static class Infusion2OnHitEffectsPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("Infusion.CompInfusionExtensions:ForOnHitWorkers");
        }

        public static bool Prepare()
        {
            return ModsConfig.IsActive("sk.infusion") && Infusion2Reflection.CanResolve;
        }

        public static void Postfix(ThingWithComps thing, ref object __result)
        {
            Infusion2CompatHelpers.RecordEffectActivations(thing, __result);
        }
    }

    [HarmonyPatch]
    public static class Infusion2PreHitEffectsPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("Infusion.CompInfusionExtensions:ForPreHitWorkers");
        }

        public static bool Prepare()
        {
            return ModsConfig.IsActive("sk.infusion") && Infusion2Reflection.CanResolve;
        }

        public static void Postfix(ThingWithComps thing, ref object __result)
        {
            Infusion2CompatHelpers.RecordEffectActivations(thing, __result);
        }
    }

    internal static class Infusion2CompatHelpers
    {
        public static void RecordInfusions(
            Comp_PawnStats stats,
            ThingWithComps thing,
            string totalStatId,
            string byTypeStatId,
            string byTierStatId)
        {
            var comp = Infusion2Reflection.GetInfusionComp(thing);
            if (comp == null)
            {
                return;
            }

            var infusionsList = Infusion2Reflection.GetInfusions(comp);
            if (infusionsList.Count == 0)
            {
                return;
            }

            stats.IncrementTotalInt(totalStatId);

            foreach (var infusion in infusionsList)
            {
                var infusionLabel = Infusion2Reflection.GetDefLabel(infusion);
                stats.IncrementTotalInt(byTypeStatId, infusionLabel);

                var tier = Infusion2Reflection.GetTier(infusion);
                var tierLabel = Infusion2Reflection.GetDefLabel(tier);
                stats.IncrementTotalInt(byTierStatId, tierLabel);
            }
        }

        public static void RecordEffectActivations(ThingWithComps thing, object __result)
        {
            if (__result == null)
            {
                return;
            }

            var effectCount = Infusion2Reflection.GetEffectCountFromResult(__result);
            if (effectCount <= 0)
            {
                return;
            }

            var pawn = Infusion2Reflection.GetPawnFromThing(thing);
            if (pawn == null || !pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(Infusion2Compat.PAWN_INFUSION_EFFECTS_ACTIVATED, effectCount);
        }
    }

    internal static class Infusion2Reflection
    {
        private const string CompInfusionTypeName = "Infusion.CompInfusion";
        private const string InfusionsRawPropertyName = "InfusionsRaw";
        private const string InfusionsPropertyName = "Infusions";
        private const string TierFieldName = "tier";
        private const string LabelFieldName = "label";
        private const string DefNameFieldName = "defName";

        private static readonly Type CompInfusionType = AccessTools.TypeByName(CompInfusionTypeName);
        private static readonly MethodInfo GetCompMethod = AccessTools.Method(typeof(ThingWithComps), nameof(ThingWithComps.GetComp));
        private static readonly PropertyInfo InfusionsRawProperty = CompInfusionType == null ? null : AccessTools.Property(CompInfusionType, InfusionsRawPropertyName);
        private static readonly PropertyInfo InfusionsProperty = CompInfusionType == null ? null : AccessTools.Property(CompInfusionType, InfusionsPropertyName);
        private static readonly FieldInfo TierField = CompInfusionType == null ? null : AccessTools.Field(AccessTools.TypeByName("Infusion.InfusionDef"), TierFieldName);
        private static readonly MethodInfo GetCompGenericMethod = CompInfusionType == null || GetCompMethod == null
            ? null
            : GetCompMethod.MakeGenericMethod(CompInfusionType);
        private static readonly Dictionary<Type, FieldInfo> LabelFieldCache = new Dictionary<Type, FieldInfo>();
        private static readonly Dictionary<Type, FieldInfo> DefNameFieldCache = new Dictionary<Type, FieldInfo>();
        private static readonly Dictionary<Type, FieldInfo> TierFieldCache = new Dictionary<Type, FieldInfo>();
        private static readonly Dictionary<Type, FieldInfo> ParentFieldCache = new Dictionary<Type, FieldInfo>();
        private static readonly PropertyInfo ApparelWearerProperty = AccessTools.Property(typeof(Apparel), nameof(Apparel.Wearer));
        private static readonly FieldInfo CompEquippablePrimaryVerbField = AccessTools.Field(typeof(CompEquippable), "primaryVerb");
        private static readonly PropertyInfo VerbCasterPawnProperty = AccessTools.Property(typeof(Verb), nameof(Verb.CasterPawn));
        private static readonly Dictionary<Type, PropertyInfo> HasValuePropertyCache = new Dictionary<Type, PropertyInfo>();
        private static readonly Dictionary<Type, PropertyInfo> ValuePropertyCache = new Dictionary<Type, PropertyInfo>();
        private static readonly Dictionary<Type, FieldInfo> TupleItem1Cache = new Dictionary<Type, FieldInfo>();

        public static bool CanResolve => CompInfusionType != null && GetCompMethod != null;

        public static object GetInfusionComp(ThingWithComps thing)
        {
            return GetCompGenericMethod.Invoke(thing, null);
        }

        public static List<object> GetInfusions(object comp)
        {
            var results = new List<object>();

            try
            {
                var raw = InfusionsRawProperty?.GetValue(comp, null) as IEnumerable;
                if (raw != null)
                {
                    foreach (var item in raw)
                    {
                        if (item != null)
                        {
                            results.Add(item);
                        }
                    }

                    return results;
                }

                var list = InfusionsProperty?.GetValue(comp, null) as IEnumerable;
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        if (item != null)
                        {
                            results.Add(item);
                        }
                    }
                }
            }
            catch
            {
                return results;
            }

            return results;
        }

        public static object GetTier(object infusion)
        {
            var type = infusion.GetType();
            if (!TierFieldCache.TryGetValue(type, out var tierField))
            {
                tierField = TierField ?? AccessTools.Field(type, TierFieldName);
                TierFieldCache[type] = tierField;
            }
            return tierField?.GetValue(infusion);
        }

        public static string GetDefLabel(object def)
        {
            var type = def.GetType();
            if (!LabelFieldCache.TryGetValue(type, out var labelField))
            {
                labelField = AccessTools.Field(type, LabelFieldName);
                LabelFieldCache[type] = labelField;
            }

            var label = labelField?.GetValue(def) as string;
            if (!string.IsNullOrWhiteSpace(label))
            {
                return label;
            }

            if (!DefNameFieldCache.TryGetValue(type, out var defNameField))
            {
                defNameField = AccessTools.Field(type, DefNameFieldName);
                DefNameFieldCache[type] = defNameField;
            }

            var defName = defNameField?.GetValue(def) as string;
            return defName;
        }

        public static Pawn GetPawnFromThing(ThingWithComps thing)
        {
            return thing.ParentHolder is Pawn_EquipmentTracker eq
                ? eq.pawn
                : null;
        }

        public static int GetEffectCountFromResult(object result)
        {
            if (result == null)
            {
                return 0;
            }

            if (TryGetItem1List(result, out var list))
            {
                return list?.Count ?? 0;
            }

            var resultType = result.GetType();
            if (!HasValuePropertyCache.TryGetValue(resultType, out var hasValueProperty))
            {
                hasValueProperty = AccessTools.Property(resultType, "HasValue");
                HasValuePropertyCache[resultType] = hasValueProperty;
            }

            if (hasValueProperty == null || !(hasValueProperty.GetValue(result, null) is bool hasValue) || !hasValue)
            {
                return 0;
            }

            if (!ValuePropertyCache.TryGetValue(resultType, out var valueProperty))
            {
                valueProperty = AccessTools.Property(resultType, "Value");
                ValuePropertyCache[resultType] = valueProperty;
            }

            var value = valueProperty?.GetValue(result, null);
            if (value == null)
            {
                return 0;
            }

            if (TryGetItem1List(value, out list))
            {
                return list?.Count ?? 0;
            }

            return 0;
        }

        private static bool TryGetItem1List(object value, out IList list)
        {
            list = null;
            if (value == null)
            {
                return false;
            }

            var valueType = value.GetType();
            if (!TupleItem1Cache.TryGetValue(valueType, out var item1Field))
            {
                item1Field = AccessTools.Field(valueType, "Item1");
                TupleItem1Cache[valueType] = item1Field;
            }

            if (item1Field != null)
            {
                list = item1Field.GetValue(value) as IList;
                return list != null;
            }

            return false;
        }
    }
}
