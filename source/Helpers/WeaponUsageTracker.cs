using RimMetrics.Components;
using Verse;

namespace RimMetrics.Helpers
{
    public static class WeaponUsageTracker
    {
        public static void SeedForPawn(Pawn pawn, Comp_PawnStats comp, int currentTick)
        {
            var weaponDefName = pawn.equipment?.Primary?.def.defName;
            if (weaponDefName == null)
            {
                return;
            }

            comp.CachedWeaponDefName = weaponDefName;
            comp.CachedWeaponUpdatedTick = currentTick;
        }

        public static void RecordEquipped(Comp_PawnStats comp, string weaponDefName, int currentTick)
        {
            if (comp.CachedWeaponDefName == weaponDefName)
            {
                FlushUsage(comp, currentTick);
                comp.CachedWeaponUpdatedTick = currentTick;
                return;
            }

            comp.CachedWeaponDefName = weaponDefName;
            comp.CachedWeaponUpdatedTick = currentTick;
        }

        public static void RecordUnequipped(Comp_PawnStats comp, int currentTick)
        {
            FlushUsage(comp, currentTick);
            comp.CachedWeaponDefName = null;
        }

        public static void SyncDaily(Pawn pawn, Comp_PawnStats comp, int currentTick)
        {
            var currentWeaponDefName = pawn.equipment?.Primary?.def.defName;
            if (currentWeaponDefName == null)
            {
                return; 
            }

            if (comp.CachedWeaponDefName == currentWeaponDefName)
            {
                FlushUsage(comp, currentTick);
                comp.CachedWeaponUpdatedTick = currentTick;
                return;
            }

            comp.CachedWeaponDefName = currentWeaponDefName;
            comp.CachedWeaponUpdatedTick = currentTick;
        }

        private static void FlushUsage(Comp_PawnStats comp, int currentTick)
        {
            var elapsedTicks = currentTick - comp.CachedWeaponUpdatedTick;
            if (elapsedTicks <= 0)
            {
                return;
            }

            comp.IncrementTotalInt(StatIds.PAWN_TIME_WEAPON_USED_BY_DEF, comp.CachedWeaponDefName, elapsedTicks);
        }
    }
}
