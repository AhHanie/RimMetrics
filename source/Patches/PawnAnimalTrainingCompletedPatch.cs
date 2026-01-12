using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Pawn_TrainingTracker), nameof(Pawn_TrainingTracker.Train))]
    public static class PawnAnimalTrainingCompletedPatch
    {
        public static void Postfix(Pawn_TrainingTracker __instance, TrainableDef td, Pawn trainer)
        {
            if (!__instance.HasLearned(td) || trainer == null)
            {
                return;
            }

            if (!trainer.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_ANIMAL_TRAININGS_COMPLETED);
        }
    }
}
