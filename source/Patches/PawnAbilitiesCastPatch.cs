using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch]
    public static class PawnAbilitiesCastPatch
    {
        private static void RecordCast(Ability ability)
        {
            if (ability == null)
            {
                return;
            }

            var pawn = ability.ConstantCaster as Pawn;
            if (pawn == null)
            {
                return;
            }

            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_ABILITIES_CAST);
            var abilityDefName = ability.def?.defName;
            if (abilityDefName != null)
            {
                comp.IncrementTotalInt(StatIds.PAWN_ABILITIES_CAST_BY_TYPE, abilityDefName);
            }

            if (ability is Psycast || ability.def?.abilityClass == typeof(Psycast))
            {
                comp.IncrementTotalInt(StatIds.PAWN_PSYCASTS_CAST);
                if (abilityDefName != null)
                {
                    comp.IncrementTotalInt(StatIds.PAWN_PSYCASTS_CAST_BY_TYPE, abilityDefName);
                }
            }
        }

        [HarmonyPatch(typeof(Ability), "Activate", new[] { typeof(LocalTargetInfo), typeof(LocalTargetInfo) })]
        [HarmonyPostfix]
        public static void ActivateLocalTargetsPostfix(Ability __instance, bool __result)
        {
            if (!__result)
            {
                return;
            }

            RecordCast(__instance);
        }

        [HarmonyPatch(typeof(Ability), "Activate", new[] { typeof(GlobalTargetInfo) })]
        [HarmonyPostfix]
        public static void ActivateGlobalTargetPostfix(Ability __instance, bool __result)
        {
            if (!__result)
            {
                return;
            }

            RecordCast(__instance);
        }
    }
}
