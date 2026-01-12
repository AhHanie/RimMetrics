using System.Collections.Generic;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Recipe_Surgery), "CheckSurgeryFail")]
    public static class PawnSurgeryBotchedPatch
    {
        public static void Postfix(bool __result, Pawn surgeon)
        {
            if (!__result)
            {
                return;
            }

            if (!surgeon.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_SURGERIES_BOTCHED);
        }
    }
}
