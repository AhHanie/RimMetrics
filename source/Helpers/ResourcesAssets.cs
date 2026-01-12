using UnityEngine;
using Verse;

namespace RimMetrics.Helpers
{
    [StaticConstructorOnStartup]
    public static class ResourcesAssets
    {
        public static readonly CachedTexture BronzeStarFilled = new CachedTexture("RimMetrics/UI/BronzeStarFilled");
        public static readonly CachedTexture SilverStarFilled = new CachedTexture("RimMetrics/UI/SilverStarFilled");
        public static readonly CachedTexture GoldStarFilled = new CachedTexture("RimMetrics/UI/GoldStarFilled");
        public static readonly Texture2D QualityAwful = ContentFinder<Texture2D>.Get("RimMetrics/UI/AwfulQuality");
        public static readonly Texture2D QualityPoor = ContentFinder<Texture2D>.Get("RimMetrics/UI/PoorQuality");
        public static readonly Texture2D QualityNormal = ContentFinder<Texture2D>.Get("RimMetrics/UI/NormalQuality");
        public static readonly Texture2D QualityGood = ContentFinder<Texture2D>.Get("RimMetrics/UI/GoodQuality");
        public static readonly Texture2D QualityExcellent = ContentFinder<Texture2D>.Get("RimMetrics/UI/ExcellentQuality");
        public static readonly Texture2D QualityMasterwork = ContentFinder<Texture2D>.Get("RimMetrics/UI/MasterworkQuality");
        public static readonly Texture2D QualityLegendary = ContentFinder<Texture2D>.Get("RimMetrics/UI/LegendaryQuality");
    }
}
