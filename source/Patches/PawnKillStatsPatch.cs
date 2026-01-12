using HarmonyLib;
using RimMetrics.Components;
using Verse;
using RimWorld;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(RecordsUtility), "Notify_PawnKilled")]
    public static class PawnKillStatsPatch
    {
        public static void Postfix(Pawn killed, Pawn killer)
        {
            var gameStats = Current.Game.GetComponent<GameComponent_GameStats>();
            if (gameStats != null && killed.IsFreeColonist)
            {
                gameStats.IncrementTotalInt(StatIds.GAME_COLONISTS_LOST);
            }

            if (!killer.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            if (killed.TryGetComp(out Comp_PawnStats victimComp) && victimComp.IsRaider)
            {
                comp.IncrementTotalInt(StatIds.PAWN_KILLS_RAIDERS);
            }

            if (killed.Faction?.def == FactionDefOf.Empire)
            {
                comp.IncrementTotalInt(StatIds.PAWN_KILLS_EMPIRE);
            }

            var raceKey = killed.def.defName;
            comp.IncrementTotalInt(StatIds.PAWN_KILLS_BY_RACE, raceKey);

            var xenotypeKey = killed.genes?.Xenotype.defName;
            if (xenotypeKey != null)
            {
                comp.IncrementTotalInt(StatIds.PAWN_KILLS_BY_XENOTYPE, xenotypeKey);
            }

            var weaponDefName = killer.equipment?.Primary?.def.defName;
            if (weaponDefName != null)
            {
                comp.IncrementTotalInt(StatIds.PAWN_KILLS_BY_WEAPON_DEF, weaponDefName);
            }
        }
    }
}
