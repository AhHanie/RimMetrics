using Verse;

namespace RimMetrics
{
    public sealed class WeaponMasteryCompat : ModCompat
    {
        public const string PAWN_WEAPON_BOND_MASTERIES_TOTAL = "PAWN_WEAPON_BOND_MASTERIES_TOTAL";
        public const string PAWN_TOTAL_WEAPONS_RENAMED = "PAWN_TOTAL_WEAPONS_RENAMED";
        public const string PAWN_WEAPON_CLASS_MASTERIES_TOTAL = "PAWN_WEAPON_CLASS_MASTERIES_TOTAL";
        public const string GAME_WEAPON_BOND_MASTERIES_TOTAL = "GAME_WEAPON_BOND_MASTERIES_TOTAL";
        public const string GAME_TOTAL_WEAPONS_RENAMED = "GAME_TOTAL_WEAPONS_RENAMED";
        public const string GAME_WEAPON_CLASS_MASTERIES_TOTAL = "GAME_WEAPON_CLASS_MASTERIES_TOTAL";

        public override bool IsEnabled()
        {
            return ModsConfig.IsActive("sk.weaponmastery");
        }

        public override string GetModPackageIdentifier()
        {
            return "sk.weaponmastery";
        }

        public override void Init()
        {
            if (!IsEnabled())
            {
                return;
            }

            StatRegistry.Register(PAWN_WEAPON_BOND_MASTERIES_TOTAL, StatCategory.EQUIPMENT);
            StatRegistry.Register(PAWN_TOTAL_WEAPONS_RENAMED, StatCategory.EQUIPMENT);
            StatRegistry.Register(PAWN_WEAPON_CLASS_MASTERIES_TOTAL, StatCategory.EQUIPMENT);
        }
    }
}
