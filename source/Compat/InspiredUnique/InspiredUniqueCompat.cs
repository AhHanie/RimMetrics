using Verse;

namespace RimMetrics
{
    public sealed class InspiredUniqueCompat : ModCompat
    {
        public const string PAWN_UNIQUE_WEAPONS_INSPIRED = "PAWN_UNIQUE_WEAPONS_INSPIRED";

        public override bool IsEnabled()
        {
            return ModsConfig.IsActive("sk.inspiredunique");
        }

        public override string GetModPackageIdentifier()
        {
            return "sk.inspiredunique";
        }

        public override void Init()
        {
            if (!IsEnabled())
            {
                return;
            }

            StatRegistry.Register(PAWN_UNIQUE_WEAPONS_INSPIRED, StatCategory.CRAFTING_PRODUCTION);
        }
    }
}
