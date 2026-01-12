using System.Collections.Generic;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(IncidentWorker_Raid), "TryGenerateRaidInfo")]
    public static class RaidTaggingPatch
    {
        public static void Postfix(bool __result, List<Pawn> pawns)
        {
            if (!__result || pawns == null)
            {
                return;
            }

            for (int i = 0; i < pawns.Count; i++)
            {
                var pawn = pawns[i];
                if (pawn == null)
                {
                    continue;
                }

                if (pawn.TryGetComp(out Comp_PawnStats comp))
                {
                    comp.IsRaider = true;
                }
            }
        }
    }
}
