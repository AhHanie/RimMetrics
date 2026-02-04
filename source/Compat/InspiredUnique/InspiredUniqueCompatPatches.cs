using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimMetrics.Components;
using Verse;
using Verse.AI;

namespace RimMetrics.Patches
{
    [HarmonyPatch]
    public static class InspiredUniqueFinishRecipePatch
    {
        private const string DisplayClassTypeName =
            "SK_Inspired_Unique.Patches.Toils_RecipePatches+FinishRecipeAndStartStoringProduct+<>c__DisplayClass0_0";

        private static readonly FieldInfo OriginalToilField =
            AccessTools.Field(AccessTools.TypeByName(DisplayClassTypeName), "originalToil");

        private static readonly FieldInfo FinishingFlagField =
            AccessTools.Field(AccessTools.TypeByName("SK_Inspired_Unique.Patches.Toils_RecipePatches"), "FINISHING_PRODUCT_FLAG");

        public static bool Prepare()
        {
            return ModsConfig.IsActive("sk.inspiredunique")
                   && OriginalToilField != null
                   && FinishingFlagField != null
                   && AccessTools.TypeByName(DisplayClassTypeName) != null;
        }

        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName(DisplayClassTypeName);
            return type == null ? null : AccessTools.Method(type, "<Postfix>b__0");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);
            if (FinishingFlagField == null || OriginalToilField == null)
            {
                Logger.Warning("InspiredUnique transpiler skipped: required fields not found.");
                return list;
            }

            var inserted = false;
            for (var i = 1; i < list.Count; i++)
            {
                var instruction = list[i];
                if (instruction.opcode != System.Reflection.Emit.OpCodes.Stsfld
                    || !(instruction.operand is FieldInfo field)
                    || field != FinishingFlagField)
                {
                    continue;
                }

                var previous = list[i - 1];
                if (previous.opcode != System.Reflection.Emit.OpCodes.Ldc_I4_0)
                {
                    continue;
                }

                list.InsertRange(i + 1, new[]
                {
                    new CodeInstruction(System.Reflection.Emit.OpCodes.Ldarg_0),
                    new CodeInstruction(System.Reflection.Emit.OpCodes.Ldfld, OriginalToilField),
                    new CodeInstruction(System.Reflection.Emit.OpCodes.Call, AccessTools.Method(typeof(InspiredUniqueFinishRecipePatch), nameof(RecordInspiredUniqueWeapon)))
                });
                i += 3;
                inserted = true;
            }

            if (inserted)
            {
                Logger.Message("InspiredUnique transpiler applied.");
            }
            else
            {
                Logger.Warning("InspiredUnique transpiler failed to find injection point.");
            }

            return list;
        }

        public static void RecordInspiredUniqueWeapon(Toil originalToil)
        {
            var pawn = originalToil?.actor;
            if (pawn == null)
            {
                return;
            }

            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(InspiredUniqueCompat.PAWN_UNIQUE_WEAPONS_INSPIRED);
        }
    }
}
