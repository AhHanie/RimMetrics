using HarmonyLib;
using RimMetrics.Components;
using Verse;
using RimWorld;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Plant), "PlantCollected")]
    public static class PawnTreesChoppedPatch
    {
        public static void Postfix(Plant __instance, Pawn by, PlantDestructionMode plantDestructionMode)
        {
            if (!by.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_PLANTS_CUT);
            comp.IncrementTotalInt(StatIds.PAWN_PLANTS_CUT_BY_TYPE, __instance.def.defName);

            if (__instance.Blighted)
            {
                comp.IncrementTotalInt(StatIds.PAWN_BLIGHTED_PLANTS_CUT);
            }

            if (__instance.def.plant?.IsTree != true)
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_TREES_CHOPPED);
        }
    }
}
