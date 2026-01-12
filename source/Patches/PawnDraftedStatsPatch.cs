using HarmonyLib;
using RimMetrics.Components;
using Verse;
using RimWorld;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    public static class PawnDraftedStatsPatch
    {
        public static void Postfix(Pawn_DraftController __instance)
        {
            if (!__instance.Drafted)
            {
                return;
            }

            if (!__instance.pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_DRAFTED);
        }
    }
}
