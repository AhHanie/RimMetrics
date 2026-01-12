using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimMetrics;
using RimMetrics.Components;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(JobDriver_Fish), "<CompleteFishingToil>b__4_0")]
    public static class PawnFishingStatsPatch
    {
        public static void RecordFishingAttempt(Pawn pawn)
        {
            if (pawn.TryGetComp(out Comp_PawnStats comp))
            {
                comp.IncrementTotalInt(StatIds.PAWN_TIMES_FISHED);
            }
        }

        public static void RecordFishCaught(Pawn pawn, int count)
        {
            if (pawn.TryGetComp(out Comp_PawnStats comp))
            {
                comp.IncrementTotalInt(StatIds.PAWN_FISH_CAUGHT, count);
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var instructionList = instructions.ToList();

            var pawnField = AccessTools.Field(typeof(JobDriver), "pawn");
            var recordAttempt = AccessTools.Method(typeof(PawnFishingStatsPatch), nameof(RecordFishingAttempt));
            var recordCaught = AccessTools.Method(typeof(PawnFishingStatsPatch), nameof(RecordFishCaught));

            var insertedAttempt = false;
            instructionList.InsertRange(0, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, pawnField),
                new CodeInstruction(OpCodes.Call, recordAttempt)
            });
            insertedAttempt = true;

            var numLocalIndex = -1;
            object numLocalOperand = null;
            for (var i = 0; i + 1 < instructionList.Count; i++)
            {
                if (instructionList[i].opcode == OpCodes.Call && instructionList[i].operand is MethodInfo methodInfo
                    && methodInfo.Name == "Sum"
                    && IsStloc(instructionList[i + 1], out numLocalIndex, out numLocalOperand))
                {
                    break;
                }
            }

            var insertedCaught = false;
            if (numLocalIndex >= 0)
            {
                var insertPos = -1;
                for (var i = 0; i + 3 < instructionList.Count; i++)
                {
                    if (IsLdloc(instructionList[i])
                        && IsBrfalse(instructionList[i + 1])
                        && IsLdloc(instructionList[i + 2])
                        && IsBrfalse(instructionList[i + 3]))
                    {
                        insertPos = i + 2;
                        break;
                    }
                }

                if (insertPos >= 0)
                {
                    instructionList.InsertRange(insertPos, new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldfld, pawnField),
                        CreateLdloc(numLocalIndex, numLocalOperand),
                        new CodeInstruction(OpCodes.Call, recordCaught)
                    });
                    insertedCaught = true;
                }
            }

            if (!insertedAttempt || !insertedCaught)
            {
                Logger.Warning($"Fishing transpiler patch incomplete. Attempt:{insertedAttempt} Caught:{insertedCaught}");
            }
            else
            {
                Logger.Message("Fishing transpiler patch applied.");
            }

            return instructionList;
        }

        internal static bool IsBrfalse(CodeInstruction instruction)
        {
            return instruction.opcode == OpCodes.Brfalse || instruction.opcode == OpCodes.Brfalse_S;
        }

        internal static bool IsLdloc(CodeInstruction instruction)
        {
            return IsLdloc(instruction, out _);
        }

        internal static bool IsLdloc(CodeInstruction instruction, out int index)
        {
            index = -1;
            if (instruction.opcode == OpCodes.Ldloc_0)
            {
                index = 0;
                return true;
            }
            if (instruction.opcode == OpCodes.Ldloc_1)
            {
                index = 1;
                return true;
            }
            if (instruction.opcode == OpCodes.Ldloc_2)
            {
                index = 2;
                return true;
            }
            if (instruction.opcode == OpCodes.Ldloc_3)
            {
                index = 3;
                return true;
            }
            if (instruction.opcode == OpCodes.Ldloc_S || instruction.opcode == OpCodes.Ldloc)
            {
                index = GetLocalIndex(instruction.operand);
                return index >= 0;
            }

            return false;
        }

        internal static bool IsStloc(CodeInstruction instruction, out int index)
        {
            return IsStloc(instruction, out index, out _);
        }

        internal static bool IsStloc(CodeInstruction instruction, out int index, out object operand)
        {
            index = -1;
            operand = null;
            if (instruction.opcode == OpCodes.Stloc_0)
            {
                index = 0;
                operand = 0;
                return true;
            }
            if (instruction.opcode == OpCodes.Stloc_1)
            {
                index = 1;
                operand = 1;
                return true;
            }
            if (instruction.opcode == OpCodes.Stloc_2)
            {
                index = 2;
                operand = 2;
                return true;
            }
            if (instruction.opcode == OpCodes.Stloc_3)
            {
                index = 3;
                operand = 3;
                return true;
            }
            if (instruction.opcode == OpCodes.Stloc_S || instruction.opcode == OpCodes.Stloc)
            {
                operand = instruction.operand;
                index = GetLocalIndex(operand);
                return index >= 0;
            }

            return false;
        }

        internal static bool IsLdloca(CodeInstruction instruction, out int index)
        {
            index = -1;
            if (instruction.opcode == OpCodes.Ldloca_S || instruction.opcode == OpCodes.Ldloca)
            {
                index = GetLocalIndex(instruction.operand);
                return index >= 0;
            }

            return false;
        }

        internal static int GetLocalIndex(object operand)
        {
            if (operand is LocalBuilder localBuilder)
            {
                return localBuilder.LocalIndex;
            }

            if (operand is int intIndex)
            {
                return intIndex;
            }

            return -1;
        }

        internal static CodeInstruction CreateLdloc(int index, object operand = null)
        {
            if (operand is LocalBuilder localBuilder)
            {
                return new CodeInstruction(OpCodes.Ldloc, localBuilder);
            }

            if (index == 0)
            {
                return new CodeInstruction(OpCodes.Ldloc_0);
            }
            if (index == 1)
            {
                return new CodeInstruction(OpCodes.Ldloc_1);
            }
            if (index == 2)
            {
                return new CodeInstruction(OpCodes.Ldloc_2);
            }
            if (index == 3)
            {
                return new CodeInstruction(OpCodes.Ldloc_3);
            }
            if (index <= byte.MaxValue)
            {
                return new CodeInstruction(OpCodes.Ldloc_S, (byte)index);
            }

            return new CodeInstruction(OpCodes.Ldloc, index);
        }
    }

    [HarmonyPatch(typeof(JobDriver_FishAnimal), "<CompleteFishingToil>b__4_0")]
    public static class PawnFishingAnimalStatsPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var instructionList = instructions.ToList();
            var pawnField = AccessTools.Field(typeof(JobDriver), "pawn");
            var recordAttempt = AccessTools.Method(typeof(PawnFishingStatsPatch), nameof(PawnFishingStatsPatch.RecordFishingAttempt));
            var recordCaught = AccessTools.Method(typeof(PawnFishingStatsPatch), nameof(PawnFishingStatsPatch.RecordFishCaught));

            var insertedAttempt = false;
            instructionList.InsertRange(0, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, pawnField),
                new CodeInstruction(OpCodes.Call, recordAttempt)
            });
            insertedAttempt = true;

            var insertedCaught = false;
            var insertPos = -1;
            for (var i = 0; i + 2 < instructionList.Count; i++)
            {
                if (PawnFishingStatsPatch.IsLdloc(instructionList[i])
                    && PawnFishingStatsPatch.IsBrfalse(instructionList[i + 1])
                    && instructionList[i + 2].opcode == OpCodes.Ldarg_0)
                {
                    insertPos = i + 2;
                    break;
                }
            }

            if (insertPos >= 0)
            {
                instructionList.InsertRange(insertPos, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, pawnField),
                    new CodeInstruction(OpCodes.Ldloc_3),
                    new CodeInstruction(OpCodes.Call, recordCaught)
                });
                insertedCaught = true;
            }

            if (!insertedAttempt || !insertedCaught)
            {
                Logger.Warning($"Fishing animal transpiler patch incomplete. Attempt:{insertedAttempt} Caught:{insertedCaught}");
            }
            else
            {
                Logger.Message("Fishing animal transpiler patch applied.");
            }

            return instructionList;
        }
    }
}
