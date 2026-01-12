using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(InspirationHandler), "TryStartInspiration")]
    public static class PawnInspirationPatch
    {
        public static void Postfix(bool __result, InspirationHandler __instance, InspirationDef def)
        {
            if (!__result)
            {
                return;
            }

            var pawn = __instance.pawn;
            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_INSPIRATIONS);
            comp.IncrementTotalInt(StatIds.PAWN_INSPIRATIONS_BY_TYPE, def.defName);
        }
    }
}
