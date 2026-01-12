using System.Reflection;
using HarmonyLib;
using RimMetrics.Components;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class PawnRangedShotsFiredPatch
    {
        public static void Postfix(Verb_LaunchProjectile __instance, bool __result)
        {
            if (!__result)
            {
                return;
            }

            var shooter = __instance.CasterPawn;
            if (shooter == null)
            {
                return;
            }

            if (!shooter.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            RangedAccuracyHelper.UpdateAccuracy(shooter, comp);
        }
    }

    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class PawnRangedShotsHitPatch
    {
        public static void Postfix(Projectile __instance, Thing hitThing)
        {
            if (hitThing == null)
            {
                return;
            }

            var launcher = __instance.launcher as Pawn;
            if (launcher == null)
            {
                return;
            }

            if (__instance.intendedTarget.Thing != hitThing)
            {
                return;
            }

            if (!launcher.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_SHOTS_HIT);
            RangedAccuracyHelper.UpdateAccuracy(launcher, comp);
        }
    }

    internal static class RangedAccuracyHelper
    {
        public static void UpdateAccuracy(Pawn pawn, Comp_PawnStats comp)
        {
            var fired = 0;
            var firedMeta = StatRegistry.GetMeta(StatIds.PAWN_SHOTS_FIRED);
            if (firedMeta.Source == StatSource.RecordDef)
            {
                fired = RecordValueReader.GetRecordValueInt(pawn, firedMeta.RecordDefName);
            }

            if (fired <= 0)
            {
                comp.SetTotalFloat(StatIds.PAWN_SHOTS_ACCURACY, 0f);
                return;
            }

            comp.TryGetStat(StatIds.PAWN_SHOTS_HIT, out var hitRecord);
            var hits = hitRecord?.TotalInt ?? 0;
            var accuracy = (float)hits / fired * 100f;
            comp.SetTotalFloat(StatIds.PAWN_SHOTS_ACCURACY, accuracy);
        }
    }
}
