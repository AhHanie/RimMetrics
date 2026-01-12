using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(CompArt), "JustCreatedBy")]
    public static class PawnArtCreatedPatch
    {
        public static void Postfix(CompArt __instance, Pawn pawn)
        {
            if (!pawn.TryGetComp(out Comp_PawnStats comp) || !__instance.Active)
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_ART_CREATED);
        }
    }
}
