using Verse;

namespace RimMetrics
{
    public sealed class Infusion2Compat : ModCompat
    {
        public const string PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED = "PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED";
        public const string PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED = "PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED";
        public const string PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TYPE = "PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TYPE";
        public const string PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TYPE = "PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TYPE";
        public const string PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TIER = "PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TIER";
        public const string PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TIER = "PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TIER";
        public const string PAWN_INFUSION_EFFECTS_ACTIVATED = "PAWN_INFUSION_EFFECTS_ACTIVATED";
        public const string GAME_TOTAL_INFUSED_WEAPONS_EQUIPPED = "GAME_TOTAL_INFUSED_WEAPONS_EQUIPPED";
        public const string GAME_TOTAL_INFUSED_APPAREL_EQUIPPED = "GAME_TOTAL_INFUSED_APPAREL_EQUIPPED";
        public const string GAME_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TYPE = "GAME_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TYPE";
        public const string GAME_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TYPE = "GAME_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TYPE";
        public const string GAME_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TIER = "GAME_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TIER";
        public const string GAME_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TIER = "GAME_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TIER";
        public const string GAME_INFUSION_EFFECTS_ACTIVATED = "GAME_INFUSION_EFFECTS_ACTIVATED";

        public override bool IsEnabled()
        {
            return ModsConfig.IsActive("sk.infusion");
        }

        public override string GetModPackageIdentifier()
        {
            return "sk.infusion";
        }

        public override void Init()
        {
            if (!IsEnabled())
            {
                return;
            }

            StatRegistry.Register(PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED, StatCategory.EQUIPMENT);
            StatRegistry.Register(PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED, StatCategory.EQUIPMENT);
            StatRegistry.Register(PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TYPE, StatCategory.EQUIPMENT, hasKey: true);
            StatRegistry.Register(PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TYPE, StatCategory.EQUIPMENT, hasKey: true);
            StatRegistry.Register(PAWN_TOTAL_INFUSED_WEAPONS_EQUIPPED_BY_TIER, StatCategory.EQUIPMENT, hasKey: true);
            StatRegistry.Register(PAWN_TOTAL_INFUSED_APPAREL_EQUIPPED_BY_TIER, StatCategory.EQUIPMENT, hasKey: true);
            StatRegistry.Register(PAWN_INFUSION_EFFECTS_ACTIVATED, StatCategory.COMBAT);
        }
    }
}
