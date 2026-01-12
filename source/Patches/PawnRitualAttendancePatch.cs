using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(LordJob_Ritual), "RitualFinished")]
    public static class PawnRitualAttendancePatch
    {
        public static void Postfix(LordJob_Ritual __instance)
        {
            foreach (var pawn in __instance.PawnsToCountTowardsPresence)
            {
                if (!pawn.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                comp.IncrementTotalInt(StatIds.PAWN_RITUALS_ATTENDED);
                comp.IncrementTotalInt(StatIds.PAWN_RITUALS_ATTENDED_BY_TYPE, __instance.Ritual.def.defName);
            }
        }
    }

    [HarmonyPatch(typeof(LordToil_PsychicRitual), "RitualCompleted")]
    public static class PawnPyschicRitualAttendancePatch
    {
        public static void Postfix(LordToil_PsychicRitual __instance)
        {
            foreach (var pawn in __instance.assignments.AllAssignedPawns)
            {
                if (!pawn.TryGetComp(out Comp_PawnStats comp))
                {
                    continue;
                }

                comp.IncrementTotalInt(StatIds.PAWN_RITUALS_ATTENDED);
                comp.IncrementTotalInt(StatIds.PAWN_RITUALS_ATTENDED_BY_TYPE, __instance.RitualData.psychicRitual.def.defName);
            }
        }
    }
}
