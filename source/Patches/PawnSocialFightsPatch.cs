using HarmonyLib;
using RimMetrics.Components;
using Verse.AI;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(MentalState_SocialFighting), "PostEnd")]
    public static class PawnSocialFightsPatch
    {
        public static void Postfix(MentalState_SocialFighting __instance)
        {
            var pawn = __instance.pawn;
            if (pawn == null)
            {
                return;
            }

            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_SOCIAL_FIGHTS);
        }
    }
}
