using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimMetrics.Patches
{
    public static class PawnMedicalItemsPatch
    {
        public static Dictionary<ThingDef, (int count, float value)> ingredientsCache = new Dictionary<ThingDef, (int count, float value)>();
        private static void RecordMedicalItems(Pawn patient)
        {
            if (!patient.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            foreach (var item in ingredientsCache)
            {
                comp.IncrementTotalInt(StatIds.PAWN_MEDICAL_ITEMS_USED_BY_TYPE, item.Key.defName, item.Value.count);
                if (item.Value.value > 0f)
                {
                    comp.IncrementTotalFloat(StatIds.PAWN_MEDICAL_ITEM_VALUE_USED, item.Value.value);
                }
            }

            
        }

        private static void RecordSingleMedicalItem(Pawn patient, Thing thing)
        {
            if (!patient.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_MEDICAL_ITEMS_USED_BY_TYPE, thing.def.defName, 1);

            var marketValue = thing.GetStatValue(StatDefOf.MarketValue);
            if (marketValue > 0f)
            {
                comp.IncrementTotalFloat(StatIds.PAWN_MEDICAL_ITEM_VALUE_USED, marketValue);
            }
        }

        [HarmonyPatch(typeof(TendUtility), "DoTend")]
        public static class TendUtilityPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionList = instructions.ToList();
                var recordMedicalItem = AccessTools.Method(typeof(PawnMedicalItemsPatch), nameof(RecordSingleMedicalItem));

                var insertPos = -1;
                var foundChecks = 0;
                for (var i = 0; i + 1 < instructionList.Count; i++)
                {
                    if (IsLdarg(instructionList[i], 2) && IsBrfalse(instructionList[i + 1]))
                    {
                        foundChecks++;
                        if (foundChecks == 2)
                        {
                            insertPos = i + 2;
                            break;
                        }
                    }
                }

                var inserted = false;
                if (insertPos >= 0)
                {
                    instructionList.InsertRange(insertPos, new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Call, recordMedicalItem)
                    });
                    inserted = true;
                }

                if (!inserted)
                {
                    Logger.Warning("Tend transpiler patch incomplete. Medical item record not inserted.");
                }
                else
                {
                    Logger.Message("Tend transpiler patch applied.");
                }

                return instructionList;
            }
        }

        [HarmonyPatch(typeof(Bill_Medical), "Notify_IterationCompleted")]
        public static class IterationCompletedPatch
        {
            public static void Postfix(Bill_Medical __instance)
            {
                if (!(__instance.recipe.Worker is Recipe_Surgery))
                {
                    return;
                }

                RecordMedicalItems(__instance.GiverPawn);
            }
        }

        [HarmonyPatch(typeof(Toils_Recipe), "ConsumeIngredients")]
        public static class ConsumeIngredientsPatch
        {
            public static void Prefix(List<Thing> ingredients)
            {
                ingredientsCache.Clear();

                foreach (Thing ingredient in ingredients)
                {
                    if (!ingredientsCache.ContainsKey(ingredient.def))
                    {
                        ingredientsCache.Add(ingredient.def, (ingredient.stackCount, ingredient.GetStatValue(StatDefOf.MarketValue) * ingredient.stackCount));
                    }
                    else
                    {
                        ingredientsCache[ingredient.def] = (ingredientsCache[ingredient.def].count + ingredient.stackCount, ingredientsCache[ingredient.def].value + ingredient.GetStatValue(StatDefOf.MarketValue) * ingredient.stackCount);
                    }
                }
            }
        }


        private static bool IsBrfalse(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Brfalse || instruction.opcode == OpCodes.Brfalse_S;
        }

        private static bool IsLdarg(CodeInstruction instruction, int index)
        {
            if (index == 0)
            {
                return instruction.opcode == OpCodes.Ldarg_0;
            }
            if (index == 1)
            {
                return instruction.opcode == OpCodes.Ldarg_1;
            }
            if (index == 2)
            {
                return instruction.opcode == OpCodes.Ldarg_2;
            }
            if (index == 3)
            {
                return instruction.opcode == OpCodes.Ldarg_3;
            }

            return false;
        }
    }
}
