using System.Reflection;
using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    

    [HarmonyPatch(typeof(Frame), "CompleteConstruction")]
    public static class PawnBuildingConstructedPatch
    {
        public static void Prefix(Frame __instance, ref IntVec3 __state)
        {
            __state = __instance.Position;
        }

        public static void Postfix(Frame __instance, Pawn worker, IntVec3 __state)
        {
            if (!worker.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            var thingDef = __instance.def.entityDefToBuild as ThingDef;

            var terrainDef = __instance.def.entityDefToBuild as TerrainDef;
            if (terrainDef != null)
            {
                comp.IncrementTotalInt(StatIds.PAWN_FLOORS_LAID);
                comp.IncrementTotalInt(StatIds.PAWN_FLOORS_LAID_BY_TYPE, terrainDef.defName);
            }

            if (thingDef == null || thingDef.category != ThingCategory.Building)
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_BUILDINGS_CONSTRUCTED_BY_TYPE, thingDef.defName);
            if (thingDef.building != null && thingDef.building.isWall)
            {
                comp.IncrementTotalInt(StatIds.PAWN_WALLS_BUILT);
            }

            var builtThing = GetConstructedThing(__state, worker.Map, thingDef);

            if (builtThing != null && QualityUtility.TryGetQuality(builtThing, out var quality))
            {
                comp.IncrementTotalInt(StatIds.PAWN_BUILDINGS_CONSTRUCTED_BY_QUALITY, quality.ToString());
            }
        }

        private static Thing GetConstructedThing(IntVec3 framePosition, Map map, ThingDef thingDef)
        {
            var things = framePosition.GetThingList(map);
            for (var i = 0; i < things.Count; i++)
            {
                var thing = things[i];
                if (thing.def == thingDef)
                {
                    return thing;
                }
            }

            return null;
        }
    }

    [HarmonyPatch]
    public static class PawnBuildingDeconstructedPatch
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(JobDriver_Deconstruct), "FinishedRemoving");
        }

        public static void Postfix(JobDriver_Deconstruct __instance)
        {
            var pawn = __instance.pawn;
            var target = __instance.job?.targetA.Thing;
            if (target == null)
            {
                return;
            }

            if (target.def.category != ThingCategory.Building)
            {
                return;
            }

            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_BUILDINGS_DECONSTRUCTED_BY_TYPE, target.def.defName);
        }
    }
}
