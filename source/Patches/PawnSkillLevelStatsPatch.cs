using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(SkillRecord), nameof(SkillRecord.Learn))]
    public static class PawnSkillLevelStatsPatch
    {
        public static void Prefix(SkillRecord __instance, out int __state)
        {
            __state = __instance?.levelInt ?? 0;
        }

        public static void Postfix(SkillRecord __instance, int __state)
        {
            var gained = __instance.levelInt - __state;
            if (gained <= 0)
            {
                return;
            }

            if (!__instance.Pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_SKILL_LEVELS_GAINED, gained);
        }
    }
}
