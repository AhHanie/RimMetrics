using HarmonyLib;
using RimMetrics.Components;
using RimMetrics.Helpers;
using RimWorld;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentAdded")]
    public static class PawnEquipmentAddedPatch
    {
        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            var pawn = __instance.pawn;
            var weaponDefName = eq?.def.defName;
            if (pawn == null || weaponDefName == null)
            {
                return;
            }

            if (pawn.TryGetComp(out Comp_PawnStats comp))
            {
                comp.IncrementTotalInt(StatIds.PAWN_WEAPONS_EQUIPPED, weaponDefName);
                comp.IncrementTotalInt(StatIds.PAWN_WEAPONS_EQUIPPED_TOTAL);
                WeaponUsageTracker.RecordEquipped(comp, weaponDefName, Find.TickManager.TicksGame);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentRemoved")]
    public static class PawnEquipmentRemovedPatch
    {
        public static void Postfix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
        {
            var pawn = __instance.pawn;
            var weaponDefName = eq?.def.defName;
            if (pawn == null || weaponDefName == null)
            {
                return;
            }

            if (pawn.TryGetComp(out Comp_PawnStats comp))
            {
                comp.IncrementTotalInt(StatIds.PAWN_WEAPONS_UNEQUIPPED, weaponDefName);
                comp.IncrementTotalInt(StatIds.PAWN_WEAPONS_UNEQUIPPED_TOTAL);
                WeaponUsageTracker.RecordUnequipped(comp, Find.TickManager.TicksGame);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
    public static class PawnApparelAddedPatch
    {
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            var pawn = __instance.pawn;
            var apparelDefName = apparel?.def.defName;
            if (pawn == null || apparelDefName == null)
            {
                return;
            }

            if (pawn.TryGetComp(out Comp_PawnStats comp))
            {
                comp.IncrementTotalInt(StatIds.PAWN_APPAREL_EQUIPPED, apparelDefName);
                comp.IncrementTotalInt(StatIds.PAWN_APPAREL_EQUIPPED_TOTAL);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
    public static class PawnApparelRemovedPatch
    {
        public static void Postfix(Pawn_ApparelTracker __instance, Apparel apparel)
        {
            var pawn = __instance.pawn;
            var apparelDefName = apparel?.def.defName;
            if (pawn == null || apparelDefName == null)
            {
                return;
            }

            if (pawn.TryGetComp(out Comp_PawnStats comp))
            {
                comp.IncrementTotalInt(StatIds.PAWN_APPAREL_UNEQUIPPED, apparelDefName);
                comp.IncrementTotalInt(StatIds.PAWN_APPAREL_UNEQUIPPED_TOTAL);
            }
        }
    }
}
