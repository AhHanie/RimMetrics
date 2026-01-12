using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using HarmonyLib;
using RimMetrics.Components;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch]
    public static class PawnPlantHarvestNutritionPatch
    {
        private static readonly System.Type DisplayClassType =
            AccessTools.TypeByName("RimWorld.JobDriver_PlantWork+<>c__DisplayClass11_0");

        public static System.Reflection.MethodBase TargetMethod()
        {
            return DisplayClassType != null
                ? AccessTools.Method(DisplayClassType, "<MakeNewToils>b__1")
                : null;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);
            var stackCountField = AccessTools.Field(typeof(Thing), nameof(Thing.stackCount));
            var hook = AccessTools.Method(typeof(PawnPlantHarvestNutritionPatch), nameof(RecordHarvestedThingNutrition));
            var inserted = false;

            for (var i = 0; i < list.Count; i++)
            {
                var fieldInfo = list[i].operand as FieldInfo;
                if (list[i].opcode != OpCodes.Stfld
                    || fieldInfo == null
                    || !ReferenceEquals(fieldInfo, stackCountField))
                {
                    continue;
                }

                // After setting thing.stackCount, capture nutrition using actor (loc.0) and thing (loc.5).
                list.InsertRange(i + 1, new[]
                {
                    new CodeInstruction(OpCodes.Ldloc_0),
                    new CodeInstruction(OpCodes.Ldloc_S, 5),
                    new CodeInstruction(OpCodes.Call, hook)
                });
                inserted = true;
                break;
            }

            if (inserted)
            {
                Logger.Message("Plant harvest nutrition transpiler applied.");
            }
            else
            {
                Logger.Warning("Plant harvest nutrition transpiler failed to find stackCount store.");
            }

            return list;
        }

        private static void RecordHarvestedThingNutrition(Pawn actor, Thing thing)
        {
            if (actor == null || thing == null)
            {
                return;
            }

            if (!actor.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            var ingestible = thing.def?.ingestible;
            if (ingestible == null)
            {
                return;
            }

            var count = thing.stackCount > 0 ? thing.stackCount : 1;
            var nutrition = ingestible.CachedNutrition * count;
            if (nutrition > 0f)
            {
                comp.IncrementTotalFloat(StatIds.PAWN_NUTRITION_PRODUCED, nutrition);
            }
        }
    }
}
