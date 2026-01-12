using HarmonyLib;
using RimMetrics.Components;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimMetrics.Patches
{
    [HarmonyPatch(typeof(Bill_Production), "Notify_IterationCompleted")]
    public static class PawnHumansButcheredPatch
    {
        public static RecipeDef lastIterationRecipe;
        public static void Postfix(Bill_Production __instance, Pawn billDoer, List<Thing> ingredients)
        {
            if (!billDoer.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            bool ingredientsHasHuman = false;

            foreach (var ingredient in ingredients)
            {
                if (ingredient.def.race?.Humanlike == true)
                {
                    ingredientsHasHuman = true;
                }
            }

            if (__instance.recipe == ModDefOf.ButcherCorpseFlesh && ingredientsHasHuman)
            {
                comp.IncrementTotalInt(StatIds.PAWN_HUMANS_BUTCHERED);
            }

            lastIterationRecipe = __instance.recipe;
        }
    }

    [HarmonyPatch(typeof(RecordsUtility), "Notify_BillDone")]
    public static class PawnCraftsPatch
    {
        public static void Postfix(Pawn billDoer, List<Thing> products)
        {
            if (!billDoer.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            var totalCount = 0;
            var totalMarketValue = 0f;
            var nutritionTotal = 0f;
            for (var i = 0; i < products.Count; i++)
            {
                var product = products[i];
                if (product == null)
                {
                    continue;
                }

                var count = product.stackCount > 0 ? product.stackCount : 1;
                totalCount += count;

                var thingDef = product.def;
                if (product is MinifiedThing minified && minified.InnerThing != null)
                {
                    thingDef = minified.InnerThing.def;
                }

                comp.IncrementTotalInt(StatIds.PAWN_CRAFTS_BY_ITEM, thingDef.defName, count);
                totalMarketValue += product.GetStatValue(StatDefOf.MarketValue) * count;

                var qualityTarget = product is MinifiedThing minifiedQuality && minifiedQuality.InnerThing != null
                    ? minifiedQuality.InnerThing
                    : product;
                if (QualityUtility.TryGetQuality(qualityTarget, out var quality))
                {
                    comp.IncrementTotalInt(StatIds.PAWN_CRAFTS_BY_QUALITY, quality.ToString(), count);
                }

                if (PawnHumansButcheredPatch.lastIterationRecipe == ModDefOf.ButcherCorpseFlesh)
                {
                    var ingestible = product.def?.ingestible;
                    if (ingestible != null)
                    {
                        nutritionTotal += ingestible.CachedNutrition * count;
                    }
                }
               
                var categories = thingDef.thingCategories;
                if (categories == null)
                {
                    continue;
                }

                for (var j = 0; j < categories.Count; j++)
                {
                    var category = categories[j];
                    comp.IncrementTotalInt(StatIds.PAWN_CRAFTS_BY_THING_CATEGORIES, category.defName, count);
                }
            }

            if (totalCount > 0)
            {
                comp.IncrementTotalInt(StatIds.PAWN_CRAFTS, totalCount);
            }

            if (totalMarketValue > 0f)
            {
                comp.IncrementTotalFloat(StatIds.PAWN_CRAFTS_MARKET_VALUE, totalMarketValue);
            }

            if (nutritionTotal > 0f)
            {
                comp.IncrementTotalFloat(StatIds.PAWN_NUTRITION_PRODUCED, nutritionTotal);
            }

            PawnHumansButcheredPatch.lastIterationRecipe = null;
        }
    }
}
