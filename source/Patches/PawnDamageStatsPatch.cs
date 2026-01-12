using System;
using HarmonyLib;
using RimMetrics.Components;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class PawnDamageStatsPatch
    {
        private const int TicksPerHour = 2500;
        private const int NearDeathBleedThresholdTicks = 6 * TicksPerHour;

        public static void Postfix(Thing __instance, DamageInfo dinfo, DamageWorker.DamageResult __result)
        {
            var amount = (int)Math.Round(__result.totalDamageDealt);
            if (amount <= 0)
            {
                return;
            }

            var damageDefName = dinfo.Def?.defName;

            var instigatorPawn = dinfo.Instigator as Pawn;
            if (instigatorPawn != null && instigatorPawn.TryGetComp(out Comp_PawnStats instigatorComp) && damageDefName != null)
            {
                instigatorComp.IncrementTotalInt(StatIds.PAWN_DAMAGE_DEALT_BY_TYPE, damageDefName, amount);
            }

            var victimPawn = __instance as Pawn;
            if (victimPawn != null && victimPawn.TryGetComp(out Comp_PawnStats victimComp) && damageDefName != null)
            {
                victimComp.IncrementTotalInt(StatIds.PAWN_DAMAGE_TAKEN_BY_TYPE, damageDefName, amount);
            }

            if (victimPawn != null && victimPawn.TryGetComp(out Comp_PawnStats nearDeathComp))
            {
                var ticksUntilDeath = HealthUtility.TicksUntilDeathDueToBloodLoss(victimPawn);
        
                if (ticksUntilDeath <= NearDeathBleedThresholdTicks && !nearDeathComp.NearDeathBleedActive)
                {
                    nearDeathComp.NearDeathBleedActive = true;
                    nearDeathComp.IncrementTotalInt(StatIds.PAWN_NEAR_DEATH_EVENTS);
                }
            }

            if (victimPawn != null
                && instigatorPawn != null
                && instigatorPawn.Faction != null
                && instigatorPawn.Faction == victimPawn.Faction
                && instigatorPawn.TryGetComp(out Comp_PawnStats friendlyFireComp))
            {
                friendlyFireComp.IncrementTotalInt(StatIds.PAWN_FRIENDLY_FIRE, amount);
                friendlyFireComp.IncrementTotalInt(StatIds.PAWN_FRIENDLY_FIRE_HITS);
            }
        }
    }
}
