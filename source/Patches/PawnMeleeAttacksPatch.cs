using HarmonyLib;
using RimMetrics.Components;
using Verse;
using RimWorld;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Verb_MeleeAttack), "SoundMiss")]
    public static class PawnMeleeMissesPatch
    {
        public static void Postfix(Verb_MeleeAttack __instance)
        {
            if (__instance.CasterPawn.TryGetComp(out Comp_PawnStats casterComp))
            {
                casterComp.IncrementTotalInt(StatIds.PAWN_MELEE_ATTACKS);
            }

            if (!(__instance.CurrentTarget.Thing is Pawn targetPawn))
            {
                return;
            }

            if (!targetPawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_MELEE_MISSES);
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttack), "SoundDodge")]
    public static class PawnMeleeDodgesPatch
    {
        public static void Postfix(Verb_MeleeAttack __instance)
        {
            if (__instance.CasterPawn.TryGetComp(out Comp_PawnStats casterComp))
            {
                casterComp.IncrementTotalInt(StatIds.PAWN_MELEE_ATTACKS);
            }

            if (!(__instance.CurrentTarget.Thing is Pawn targetPawn))
            {
                return;
            }

            if (!targetPawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_MELEE_DODGES);
        }
    }

    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    public static class PawnMeleeHitsPatch
    {
        public static void Postfix(Verb_MeleeAttackDamage __instance, DamageWorker.DamageResult __result)
        {
            if (__instance.CasterPawn.TryGetComp(out Comp_PawnStats casterComp))
            {
                casterComp.IncrementTotalInt(StatIds.PAWN_MELEE_ATTACKS);
            }

            if (__result.totalDamageDealt <= 0f)
            {
                return;
            }

            var caster = __instance.CasterPawn;
            if (caster == null)
            {
                return;
            }

            if (!caster.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_MELEE_HITS);
        }
    }
}
