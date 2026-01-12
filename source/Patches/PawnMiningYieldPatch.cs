using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch]
    public static class PawnMiningYieldPatch
    {
        private static readonly FieldInfo ThingStackCountField = AccessTools.Field(typeof(Thing), "stackCount");

        public static IEnumerable<MethodBase> TargetMethods()
        {
            var method = AccessTools.Method(typeof(Mineable), "TrySpawnYield", new[] { typeof(Map), typeof(bool), typeof(Pawn) });
            if (method != null)
            {
                yield return method;
            }
        }

        public static void RecordMinedYield(Pawn pawn, Thing thing)
        {
            if (pawn == null || thing == null)
            {
                return;
            }

            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_CELLS_MINED_BY_ITEM, thing.def.defName, thing.stackCount);
        }

        private static void SetStackCountAndRecord(Thing thing, int count, Pawn pawn)
        {
            if (thing == null)
            {
                return;
            }

            thing.stackCount = count;
            if (pawn == null)
            {
                return;
            }

            RecordMinedYield(pawn, thing);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var instructionList = instructions.ToList();
            var makeThing = AccessTools.Method(typeof(ThingMaker), nameof(ThingMaker.MakeThing), new[] { typeof(ThingDef), typeof(ThingDef) });
            var setStackCountAndRecord = AccessTools.Method(typeof(PawnMiningYieldPatch), nameof(SetStackCountAndRecord));
            var replaced = false;

            for (var i = 0; i < instructionList.Count; i++)
            {
                if (instructionList[i].opcode != OpCodes.Stfld || !Equals(instructionList[i].operand, ThingStackCountField))
                {
                    continue;
                }

                var loadPawn = new CodeInstruction(OpCodes.Ldarg_3);
                loadPawn.labels.AddRange(instructionList[i].labels);
                loadPawn.blocks.AddRange(instructionList[i].blocks);
                instructionList[i].labels.Clear();
                instructionList[i].blocks.Clear();

                instructionList[i] = new CodeInstruction(OpCodes.Call, setStackCountAndRecord);
                instructionList.Insert(i, loadPawn);
                replaced = true;
                break;
            }

            if (!replaced)
            {
                Logger.Warning("Mining yield transpiler patch incomplete. Yield tracking disabled.");
            }
            else
            {
                Logger.Message("Mining yield transpiler patch applied.");
            }

            return instructionList;
        }
    }
}
