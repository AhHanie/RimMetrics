using Verse;
using RimWorld;

namespace RimMetrics { 

    [DefOf]
    public static class ModDefOf
    {
        public static RecipeDef ButcherCorpseFlesh;

        static ModDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ModDefOf));
        }
    }

}
