using System;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(CompShield), nameof(CompShield.PostPreApplyDamage))]
    public static class PawnShieldAbsorbPatch
    {
        private static Pawn GetPawnOwner(CompShield shield)
        {
            if (shield?.parent is Pawn pawn)
            {
                return pawn;
            }

            if (shield?.parent is Apparel apparel)
            {
                return apparel.Wearer;
            }

            return null;
        }

        public static void Postfix(ref DamageInfo dinfo, ref bool absorbed, CompShield __instance)
        {
            if (!absorbed)
            {
                return;
            }

            var owner = GetPawnOwner(__instance);
            if (owner == null)
            {
                return;
            }

            if (!owner.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            var amount = (int)Math.Round(dinfo.Amount);
            if (amount <= 0)
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_SHIELD_HITS_ABSORBED);
            comp.IncrementTotalInt(StatIds.PAWN_SHIELD_DAMAGE_ABSORBED, amount);
        }
    }
}
