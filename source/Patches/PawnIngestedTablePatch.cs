using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    public static class PawnIngestedTablePatch
    {
        private const string FinalizeIngestInnerMethod = "RimWorld.Toils_Ingest+<>c__DisplayClass14_0:<FinalizeIngest>b__0";

        [HarmonyPatch]
        public static class FinalizeIngestTranspiler
        {
            public static System.Reflection.MethodBase TargetMethod()
            {
                return AccessTools.Method(FinalizeIngestInnerMethod);
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var matcher = new CodeMatcher(instructions);
                var ingesterField = AccessTools.Field(AccessTools.TypeByName("RimWorld.Toils_Ingest+<>c__DisplayClass14_0"), "ingester");
                var ingestedMethod = AccessTools.Method(typeof(Thing), nameof(Thing.Ingested));
                var incrementMethod = AccessTools.Method(typeof(PawnIngestedTablePatch), nameof(IncrementTableStats));

                // Find the call to thing.Ingested(ingester, num)
                matcher.MatchStartForward(
                    new CodeMatch(OpCodes.Callvirt, ingestedMethod));

                if (!matcher.IsValid)
                {
                    Logger.Error("Failed to find Ingested call in FinalizeIngest transpiler patch.");
                    return instructions;
                }

                // Move to after the Ingested call
                matcher.Advance(1);

                // Insert our check: IncrementTableStats(actor, ingester, thing)
                // At this point: stack has num2 (result of Ingested), locals have actor (0), thing (2)
                matcher.Insert(
                    new CodeInstruction(OpCodes.Ldloc_0),      // Load actor
                    new CodeInstruction(OpCodes.Ldarg_0),       // Load this (DisplayClass14_0)
                    new CodeInstruction(OpCodes.Ldfld, ingesterField),  // Load ingester
                    new CodeInstruction(OpCodes.Ldloc_2),       // Load thing
                    new CodeInstruction(OpCodes.Call, incrementMethod));

                Logger.Message("FinalizeIngest transpiler patch applied.");
                return matcher.InstructionEnumeration();
            }
        }

        private static void IncrementTableStats(Pawn actor, Pawn ingester, Thing thing)
        {
            if (actor == null || ingester == null || thing?.def?.ingestible == null)
            {
                return;
            }

            if (!ingester.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            Logger.Message("Tracking meal consumption");

            var withoutTable =
                !(ingester.Position + ingester.Rotation.FacingCell).HasEatSurface(actor.Map)
                && ingester.GetPosture() == PawnPosture.Standing
                && !ingester.IsWildMan()
                && thing.def.ingestible.tableDesired;

            if (withoutTable)
            {
                comp.IncrementTotalInt(StatIds.PAWN_MEALS_WITHOUT_TABLE);
            }
            else
            {
                comp.IncrementTotalInt(StatIds.PAWN_MEALS_AT_TABLE);
            }
        }
    }
}