using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(InteractionWorker), "Interacted")]
    public static class PawnSocialInteractionPatch
    {
        public static void Postfix(InteractionWorker __instance, Pawn initiator)
        {
            if (!initiator.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_SOCIAL_INTERACTIONS);
            comp.IncrementTotalInt(StatIds.PAWN_SOCIAL_INTERACTIONS_BY_TYPE, __instance.interaction.defName);
        }
    }
}
