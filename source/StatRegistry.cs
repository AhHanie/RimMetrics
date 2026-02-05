using System.Collections.Generic;
using RimMetrics.CalculatedStats;
using RimMetrics.Helpers;
using Verse;

namespace RimMetrics
{
    public enum StatSource
    {
        Manual,
        RecordDef,
        CalculatedStat
    }

    public enum StatValueType
    {
        Int,
        Float
    }

    public sealed class StatMeta
    {
        public string TypeId;
        public string Category;
        public StatType StatType;
        public int DisplayOrder;
        public StatSource Source;
        public string RecordDefName;
        public StatValueType StatValueType;
        public System.Type CalculatorType;
        public bool HasKey;
        public UnityEngine.Texture2D Icon;
        public System.Type IconSelectorType;
        public System.Type ValueTransformerType;
    }

    public static class StatRegistry
    {
        private static readonly Dictionary<string, StatMeta> Stats = new Dictionary<string, StatMeta>();

        static StatRegistry()
        {
            RegisterBuiltIns();
        }

        public static void Register(
            string typeId,
            string category = null,
            StatType statType = StatType.PAWN,
            int displayOrder = 0,
            StatSource source = StatSource.Manual,
            string recordDefName = null,
            StatValueType statValueType = StatValueType.Int,
            bool hasKey = false,
            System.Type calculatorType = null,
            UnityEngine.Texture2D icon = null,
            System.Type iconSelectorType = null,
            System.Type valueTransformerType = null,
            bool autoRegisterGameStat = true)
        {
            if (string.IsNullOrWhiteSpace(typeId))
            {
                return;
            }

            var categoryId = category ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                StatCategoryRegistry.RegisterCategory(categoryId);
            }

            Stats[typeId] = new StatMeta
            {
                TypeId = typeId,
                Category = categoryId,
                StatType = statType,
                DisplayOrder = displayOrder,
                Source = source,
                RecordDefName = recordDefName ?? string.Empty,
                StatValueType = statValueType,
                CalculatorType = calculatorType,
                HasKey = hasKey,
                Icon = icon,
                IconSelectorType = iconSelectorType ?? (hasKey ? typeof(DefIconSelector) : typeof(SimpleIconSelector)),
                ValueTransformerType = valueTransformerType,
            };

            if (autoRegisterGameStat)
            {
                TryRegisterGameStat(typeId, Stats[typeId]);
            }
        }

        public static bool IsRegistered(string typeId)
        {
            return !string.IsNullOrWhiteSpace(typeId) && Stats.ContainsKey(typeId);
        }

        public static StatMeta GetMeta(string typeId)
        {
            if (string.IsNullOrWhiteSpace(typeId))
            {
                return null;
            }

            return Stats.TryGetValue(typeId, out var meta) ? meta : null;
        }

        public static IEnumerable<StatMeta> GetAllMetas()
        {
            return Stats.Values;
        }

        private static void TryRegisterGameStat(string typeId, StatMeta pawnMeta)
        {
            if (pawnMeta.StatType != StatType.PAWN)
            {
                return;
            }

            if (pawnMeta.Source == StatSource.CalculatedStat)
            {
                return;
            }

            var gameTypeId = GetGameStatId(typeId);
            if (string.IsNullOrWhiteSpace(gameTypeId))
            {
                return;
            }

            var calculatorType = pawnMeta.Source == StatSource.RecordDef
                ? typeof(ColonistRecordTotalStatProvider)
                : (pawnMeta.HasKey ? typeof(ColonistManualKeyedTotalStatProvider) : typeof(ColonistManualTotalStatProvider));

            Register(
                gameTypeId,
                pawnMeta.Category,
                StatType.GAME,
                pawnMeta.DisplayOrder,
                StatSource.CalculatedStat,
                pawnMeta.RecordDefName,
                pawnMeta.StatValueType,
                pawnMeta.HasKey,
                calculatorType,
                valueTransformerType: pawnMeta.ValueTransformerType,
                autoRegisterGameStat: false);
        }

        private static string GetGameStatId(string pawnTypeId)
        {
            if (string.IsNullOrWhiteSpace(pawnTypeId))
            {
                return string.Empty;
            }

            if (pawnTypeId.Length <= "PAWN_".Length)
            {
                return string.Empty;
            }

            return "GAME_" + pawnTypeId.Substring("PAWN_".Length);
        }

        private static void RegisterBuiltIns()
        {
            Register(StatIds.PAWN_KILLS, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "Kills");
            Register(StatIds.PAWN_KILLS_WILDLIFE, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "KillsAnimals");
            Register(StatIds.PAWN_KILLS_RAIDERS, StatCategory.COMBAT);
            Register(StatIds.PAWN_KILLS_MECHANOID, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "KillsMechanoids");
            Register(StatIds.PAWN_KILLS_BY_RACE, StatCategory.COMBAT, hasKey: true);
            Register(StatIds.PAWN_KILLS_BY_XENOTYPE, StatCategory.COMBAT, hasKey: true);
            Register(StatIds.PAWN_KILLS_BY_WEAPON_DEF, StatCategory.COMBAT, hasKey: true);
            Register(StatIds.PAWN_WEAPONS_EQUIPPED, StatCategory.EQUIPMENT, hasKey: true);
            Register(StatIds.PAWN_WEAPONS_EQUIPPED_TOTAL, StatCategory.EQUIPMENT);
            Register(StatIds.PAWN_WEAPONS_UNEQUIPPED, StatCategory.EQUIPMENT, hasKey: true);
            Register(StatIds.PAWN_WEAPONS_UNEQUIPPED_TOTAL, StatCategory.EQUIPMENT);
            Register(StatIds.PAWN_APPAREL_EQUIPPED, StatCategory.EQUIPMENT, hasKey: true);
            Register(StatIds.PAWN_APPAREL_EQUIPPED_TOTAL, StatCategory.EQUIPMENT);
            Register(StatIds.PAWN_APPAREL_UNEQUIPPED, StatCategory.EQUIPMENT, hasKey: true);
            Register(StatIds.PAWN_APPAREL_UNEQUIPPED_TOTAL, StatCategory.EQUIPMENT);
            Register(StatIds.PAWN_ART_CREATED, StatCategory.CRAFTING_PRODUCTION);
            Register(StatIds.PAWN_TIME_WEAPON_USED_BY_DEF, StatCategory.EQUIPMENT, hasKey: true, valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_HITS_CAUSING_DOWNED, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "PawnsDowned");
            Register(StatIds.PAWN_HITS_CAUSING_DOWNED_HUMANLIKES, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "PawnsDownedHumanlikes");
            Register(StatIds.PAWN_HITS_CAUSING_DOWNED_ANIMALS, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "PawnsDownedAnimals");
            Register(StatIds.PAWN_HITS_CAUSING_DOWNED_MECHANOIDS, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "PawnsDownedMechanoids");
            Register(StatIds.PAWN_SHOTS_FIRED, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "ShotsFired");
            Register(StatIds.PAWN_SHOTS_HIT, StatCategory.COMBAT);
            Register(StatIds.PAWN_SHOTS_ACCURACY, StatCategory.COMBAT, statValueType: StatValueType.Float);
            Register(
                StatIds.GAME_SHOTS_ACCURACY,
                StatCategory.COMBAT,
                statType: StatType.GAME,
                source: StatSource.CalculatedStat,
                statValueType: StatValueType.Float,
                calculatorType: typeof(GameShotsAccuracyAverageStatProvider),
                autoRegisterGameStat: false);
            Register(
                StatIds.GAME_TRADE_PROFIT,
                StatCategory.ECONOMY_TRADE,
                statType: StatType.GAME,
                source: StatSource.CalculatedStat,
                statValueType: StatValueType.Float,
                calculatorType: typeof(GameTradeProfitStatProvider),
                autoRegisterGameStat: false);
            Register(StatIds.GAME_ORBITAL_TRADERS_VISITED, StatCategory.ECONOMY_TRADE, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_ORBITAL_TRADERS_VISITED_BY_TYPE, StatCategory.ECONOMY_TRADE, statType: StatType.GAME, hasKey: true, autoRegisterGameStat: false);
            Register(StatIds.GAME_TRADE_CARAVANS_VISITED, StatCategory.ECONOMY_TRADE, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_TRADE_CARAVANS_VISITED_BY_TYPE, StatCategory.ECONOMY_TRADE, statType: StatType.GAME, hasKey: true, autoRegisterGameStat: false);
            Register(StatIds.GAME_TRADE_CARAVANS_VISITED_PAWNS, StatCategory.ECONOMY_TRADE, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_VISITORS, StatCategory.MISC_EVENTS, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_TOTAL_INCIDENTS, StatCategory.MISC_EVENTS, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_TOTAL_INCIDENTS_BY_TYPE, StatCategory.MISC_EVENTS, statType: StatType.GAME, hasKey: true, iconSelectorType: typeof(SimpleIconSelector), autoRegisterGameStat: false);
            Register(StatIds.GAME_TOTAL_RAIDS, StatCategory.COMBAT, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_TOTAL_RAIDS_BY_FACTION, StatCategory.COMBAT, statType: StatType.GAME, hasKey: true, autoRegisterGameStat: false);
            Register(StatIds.GAME_COLONISTS_JOINED, StatCategory.SOCIAL_IDEOLOGY, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_COLONISTS_LOST, StatCategory.SOCIAL_IDEOLOGY, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_FACTIONS_ALLIED, StatCategory.SOCIAL_IDEOLOGY, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_FACTIONS_MADE_HOSTILE, StatCategory.SOCIAL_IDEOLOGY, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_ROOMS_BUILT, StatCategory.CONSTRUCTION, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_ROOMS_BUILT_BY_ROLE, StatCategory.CONSTRUCTION, statType: StatType.GAME, hasKey: true, autoRegisterGameStat: false);
            Register(StatIds.GAME_RESEARCH_PROJECTS_COMPLETED, StatCategory.RESEARCH, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_MARRIAGES, StatCategory.SOCIAL_IDEOLOGY, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_BREAKUPS, StatCategory.SOCIAL_IDEOLOGY, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_AFFAIRS, StatCategory.SOCIAL_IDEOLOGY, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_QUESTS_COMPLETED, StatCategory.MISC_EVENTS, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_QUESTS_FAILED, StatCategory.MISC_EVENTS, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_QUESTS_ACCEPTED, StatCategory.MISC_EVENTS, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_GRAVSHIPS_LAUNCHED, StatCategory.TRAVEL_MOVEMENT, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_SHUTTLES_LAUNCHED, StatCategory.TRAVEL_MOVEMENT, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.GAME_TRANSPORT_PODS_LAUNCHED, StatCategory.TRAVEL_MOVEMENT, statType: StatType.GAME, autoRegisterGameStat: false);
            Register(StatIds.PAWN_MELEE_ATTACKS, StatCategory.COMBAT);
            Register(StatIds.PAWN_MELEE_HITS, StatCategory.COMBAT);
            Register(StatIds.PAWN_MELEE_MISSES, StatCategory.COMBAT);
            Register(StatIds.PAWN_MELEE_DODGES, StatCategory.COMBAT);
            Register(StatIds.PAWN_HEADSHOTS, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "Headshots");
            Register(StatIds.PAWN_DAMAGE_DEALT, StatCategory.DAMAGE_DEFENSE, source: StatSource.RecordDef, recordDefName: "DamageDealt");
            Register(StatIds.PAWN_DAMAGE_DEALT_BY_TYPE, StatCategory.DAMAGE_DEFENSE, hasKey: true);
            Register(StatIds.PAWN_DAMAGE_TAKEN, StatCategory.DAMAGE_DEFENSE, source: StatSource.RecordDef, recordDefName: "DamageTaken");
            Register(StatIds.PAWN_DAMAGE_TAKEN_BY_TYPE, StatCategory.DAMAGE_DEFENSE, hasKey: true);
            Register(StatIds.PAWN_SHIELD_DAMAGE_ABSORBED, StatCategory.DAMAGE_DEFENSE);
            Register(StatIds.PAWN_SHIELD_HITS_ABSORBED, StatCategory.DAMAGE_DEFENSE);
            Register(StatIds.PAWN_CRAFTS, StatCategory.CRAFTING_PRODUCTION);
            Register(StatIds.PAWN_CRAFTS_BY_ITEM, StatCategory.CRAFTING_PRODUCTION, hasKey: true);
            Register(StatIds.PAWN_MEALS_COOKED, StatCategory.CRAFTING_PRODUCTION, source: StatSource.RecordDef, recordDefName: "MealsCooked");
            Register(StatIds.PAWN_CRAFTS_BY_THING_CATEGORIES, StatCategory.CRAFTING_PRODUCTION, hasKey: true);
            Register(StatIds.PAWN_CRAFTS_BY_QUALITY, StatCategory.CRAFTING_PRODUCTION, hasKey: true, iconSelectorType: typeof(QualityIconSelector));
            Register(StatIds.PAWN_CRAFTS_MARKET_VALUE, StatCategory.CRAFTING_PRODUCTION, statValueType: StatValueType.Float);
            Register(StatIds.PAWN_THINGS_CONSTRUCTED, StatCategory.CONSTRUCTION, source: StatSource.RecordDef, recordDefName: "ThingsConstructed");
            Register(StatIds.PAWN_BUILDINGS_CONSTRUCTED_BY_TYPE, StatCategory.CONSTRUCTION, hasKey: true);
            Register(StatIds.PAWN_BUILDINGS_CONSTRUCTED_BY_QUALITY, StatCategory.CONSTRUCTION, hasKey: true, iconSelectorType: typeof(QualityIconSelector));
            Register(StatIds.PAWN_BUILDINGS_DECONSTRUCTED, StatCategory.CONSTRUCTION, source: StatSource.RecordDef, recordDefName: "ThingsDeconstructed");
            Register(StatIds.PAWN_BUILDINGS_DECONSTRUCTED_BY_TYPE, StatCategory.CONSTRUCTION, hasKey: true);
            Register(StatIds.PAWN_TREES_CHOPPED, StatCategory.WORK_LABOR);
            Register(StatIds.PAWN_PLANTS_CUT, StatCategory.WORK_LABOR);
            Register(StatIds.PAWN_BLIGHTED_PLANTS_CUT, StatCategory.WORK_LABOR);
            Register(StatIds.PAWN_TIMES_IN_MENTAL_STATE, StatCategory.MENTAL_MOOD, source: StatSource.RecordDef, recordDefName: "TimesInMentalState");
            Register(StatIds.PAWN_MEMORY_THOUGHTS, StatCategory.MENTAL_MOOD);
            Register(StatIds.PAWN_MEMORY_THOUGHTS_BY_TYPE, StatCategory.MENTAL_MOOD, hasKey: true);
            Register(StatIds.PAWN_TIMES_ON_FIRE, StatCategory.MISC_EVENTS, source: StatSource.RecordDef, recordDefName: "TimesOnFire");
            Register(StatIds.PAWN_FIRES_EXTINGUISHED, StatCategory.MISC_EVENTS, source: StatSource.RecordDef, recordDefName: "FiresExtinguished");
            Register(StatIds.PAWN_OPERATIONS_RECEIVED, StatCategory.MEDICAL_HEALTH, source: StatSource.RecordDef, recordDefName: "OperationsReceived");
            Register(StatIds.PAWN_OPERATIONS_PERFORMED, StatCategory.MEDICAL_HEALTH, source: StatSource.RecordDef, recordDefName: "OperationsPerformed");
            Register(StatIds.PAWN_TIMES_TENDED_TO, StatCategory.MEDICAL_HEALTH, source: StatSource.RecordDef, recordDefName: "TimesTendedTo");
            Register(StatIds.PAWN_TIMES_TENDED_OTHER, StatCategory.MEDICAL_HEALTH, source: StatSource.RecordDef, recordDefName: "TimesTendedOther");
            Register(StatIds.PAWN_MEDICAL_ITEM_VALUE_USED, StatCategory.MEDICAL_HEALTH, statValueType: StatValueType.Float);
            Register(StatIds.PAWN_MEDICAL_ITEMS_USED_BY_TYPE, StatCategory.MEDICAL_HEALTH, hasKey: true);
            Register(StatIds.PAWN_SURGERIES_BOTCHED, StatCategory.MEDICAL_HEALTH);
            Register(StatIds.PAWN_PEOPLE_CAPTURED, StatCategory.SOCIAL_IDEOLOGY, source: StatSource.RecordDef, recordDefName: "PeopleCaptured");
            Register(StatIds.PAWN_PRISONERS_RECRUITED, StatCategory.SOCIAL_IDEOLOGY, source: StatSource.RecordDef, recordDefName: "PrisonersRecruited");
            Register(StatIds.PAWN_PRISONERS_CHATTED, StatCategory.SOCIAL_IDEOLOGY, source: StatSource.RecordDef, recordDefName: "PrisonersChatted");
            Register(StatIds.PAWN_ANIMALS_TAMED, StatCategory.ANIMALS, source: StatSource.RecordDef, recordDefName: "AnimalsTamed");
            Register(StatIds.PAWN_ANIMAL_TRAININGS_COMPLETED, StatCategory.ANIMALS);
            Register(StatIds.PAWN_ANIMALS_SLAUGHTERED, StatCategory.ANIMALS, source: StatSource.RecordDef, recordDefName: "AnimalsSlaughtered");
            Register(StatIds.PAWN_HUMANS_BUTCHERED, StatCategory.WORK_LABOR);
            Register(StatIds.PAWN_THINGS_INSTALLED, StatCategory.CONSTRUCTION, source: StatSource.RecordDef, recordDefName: "ThingsInstalled");
            Register(StatIds.PAWN_THINGS_REPAIRED, StatCategory.WORK_LABOR, source: StatSource.RecordDef, recordDefName: "ThingsRepaired");
            Register(StatIds.PAWN_THINGS_HAULED, StatCategory.WORK_LABOR, source: StatSource.RecordDef, recordDefName: "ThingsHauled");
            Register(StatIds.PAWN_PLANTS_SOWN, StatCategory.WORK_LABOR, source: StatSource.RecordDef, recordDefName: "PlantsSown");
            Register(StatIds.PAWN_PLANTS_HARVESTED, StatCategory.WORK_LABOR, source: StatSource.RecordDef, recordDefName: "PlantsHarvested");
            Register(StatIds.PAWN_CELLS_MINED, StatCategory.WORK_LABOR, source: StatSource.RecordDef, recordDefName: "CellsMined");
            Register(StatIds.PAWN_CELLS_MINED_BY_ITEM, StatCategory.WORK_LABOR, hasKey: true);
            Register(StatIds.PAWN_MESSES_CLEANED, StatCategory.WORK_LABOR, source: StatSource.RecordDef, recordDefName: "MessesCleaned");
            Register(StatIds.PAWN_RESEARCH_POINTS_RESEARCHED, StatCategory.RESEARCH, source: StatSource.RecordDef, recordDefName: "ResearchPointsResearched");
            Register(StatIds.PAWN_RESEARCH_SESSIONS, StatCategory.RESEARCH);
            Register(StatIds.PAWN_CORPSES_BURIED, StatCategory.MISC_EVENTS, source: StatSource.RecordDef, recordDefName: "CorpsesBuried");
            Register(StatIds.PAWN_NUTRITION_EATEN, StatCategory.NEEDS_SURVIVAL, source: StatSource.RecordDef, recordDefName: "NutritionEaten");
            Register(StatIds.PAWN_NUTRITION_EATEN_BY_TYPE, StatCategory.NEEDS_SURVIVAL, statValueType: StatValueType.Float, hasKey: true);
            Register(StatIds.PAWN_FOOD_EATEN_COUNT_BY_TYPE, StatCategory.NEEDS_SURVIVAL, hasKey: true);
            Register(StatIds.PAWN_BODIES_STRIPPED, StatCategory.MISC_EVENTS, source: StatSource.RecordDef, recordDefName: "BodiesStripped");
            Register(StatIds.PAWN_THINGS_UNINSTALLED, StatCategory.CONSTRUCTION, source: StatSource.RecordDef, recordDefName: "ThingsUninstalled");
            Register(StatIds.PAWN_ARTIFACTS_ACTIVATED, StatCategory.MISC_EVENTS, source: StatSource.RecordDef, recordDefName: "ArtifactsActivated");
            Register(StatIds.PAWN_CONTAINERS_OPENED, StatCategory.MISC_EVENTS, source: StatSource.RecordDef, recordDefName: "ContainersOpened");
            Register(StatIds.PAWN_SWITCHES_FLICKED, StatCategory.MISC_EVENTS, source: StatSource.RecordDef, recordDefName: "SwitchesFlicked");
            Register(StatIds.PAWN_TIME_AS_COLONIST_OR_COLONY_ANIMAL, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeAsColonistOrColonyAnimal", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_AS_QUEST_LODGER, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeAsQuestLodger", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_AS_PRISONER, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeAsPrisoner", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_IN_BED, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeInBed", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_IN_BED_FOR_MEDICAL_REASONS, StatCategory.MEDICAL_HEALTH, source: StatSource.RecordDef, recordDefName: "TimeInBedForMedicalReasons", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_DOWNED, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeDowned", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_GETTING_FOOD, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeGettingFood", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_GETTING_JOY, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeGettingJoy", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_UNDER_ROOF, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeUnderRoof", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_DRAFTED, StatCategory.COMBAT);
            Register(StatIds.PAWN_TIME_DRAFTED, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeDrafted", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_ON_FIRE, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeOnFire", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_IN_MENTAL_STATE, StatCategory.MENTAL_MOOD, source: StatSource.RecordDef, recordDefName: "TimeInMentalState", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_HAULING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeHauling", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_HANDLING_ANIMALS, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeHandlingAnimals", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_FIREFIGHTING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeFirefighting", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_WARDENING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeWardening", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_HUNTING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeHunting", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_CONSTRUCTING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeConstructing", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_TREATING_AND_FEEDING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeTreatingAndFeeding", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_RESEARCHING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeResearching", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_CLEANING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeCleaning", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_REPAIRING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeRepairing", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_MINING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeMining", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_TIME_SOWING_AND_HARVESTING, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeSowingAndHarvesting", valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(StatIds.PAWN_DISEASES_CONTRACTED, StatCategory.MEDICAL_HEALTH);
            Register(StatIds.PAWN_DISEASES_RECOVERED, StatCategory.MEDICAL_HEALTH);
            Register(StatIds.PAWN_DISEASES_CONTRACTED_BY_TYPE, StatCategory.MEDICAL_HEALTH, hasKey: true);
            Register(
                StatIds.PAWN_INFECTIONS_RECEIVED,
                StatCategory.MEDICAL_HEALTH,
                source: StatSource.CalculatedStat,
                calculatorType: typeof(PawnInfectionsReceivedStatProvider),
                autoRegisterGameStat: false);
            Register(
                StatIds.GAME_INFECTIONS_RECEIVED,
                StatCategory.MEDICAL_HEALTH,
                statType: StatType.GAME,
                source: StatSource.CalculatedStat,
                calculatorType: typeof(PawnInfectionsReceivedStatProvider),
                autoRegisterGameStat: false);
            Register(StatIds.PAWN_JOBS_STARTED, StatCategory.WORK_LABOR);
            Register(StatIds.PAWN_JOBS_STARTED_BY_TYPE, StatCategory.WORK_LABOR, hasKey: true);
            Register(StatIds.PAWN_JOBS_TOTAL_TIME_BY_TYPE, StatCategory.WORK_LABOR, hasKey: true, valueTransformerType: typeof(TimeTicksValueTransformer));
            Register(
                StatIds.PAWN_JOB_TIME_SHARE_BY_TYPE,
                StatCategory.WORK_LABOR,
                source: StatSource.CalculatedStat,
                statValueType: StatValueType.Float,
                hasKey: true,
                calculatorType: typeof(JobTimeShareByTypeStatProvider),
                valueTransformerType: typeof(PercentageValueTransformer),
                autoRegisterGameStat: false);
            Register(
                StatIds.GAME_JOB_TIME_SHARE_BY_TYPE,
                StatCategory.WORK_LABOR,
                statType: StatType.GAME,
                source: StatSource.CalculatedStat,
                statValueType: StatValueType.Float,
                hasKey: true,
                calculatorType: typeof(JobTimeShareByTypeStatProvider),
                valueTransformerType: typeof(PercentageValueTransformer),
                autoRegisterGameStat: false);
            Register(StatIds.PAWN_SOCIAL_FIGHTS, StatCategory.SOCIAL_IDEOLOGY);
            Register(StatIds.PAWN_SOCIAL_INTERACTIONS, StatCategory.SOCIAL_IDEOLOGY);
            Register(StatIds.PAWN_SOCIAL_INTERACTIONS_BY_TYPE, StatCategory.SOCIAL_IDEOLOGY, hasKey: true);
            Register(StatIds.PAWN_TIMES_IN_MENTAL_STATE_BY_TYPE, StatCategory.MENTAL_MOOD, hasKey: true);
            Register(StatIds.PAWN_PEOPLE_CONVERTED, StatCategory.SOCIAL_IDEOLOGY);
            Register(StatIds.PAWN_CARAVANS_JOINED, StatCategory.TRAVEL_MOVEMENT);
            Register(StatIds.PAWN_NEGOTIATED_TRADES, StatCategory.ECONOMY_TRADE);
            Register(StatIds.PAWN_ITEMS_BOUGHT_BY_TYPE, StatCategory.ECONOMY_TRADE, hasKey: true);
            Register(StatIds.PAWN_ITEMS_SOLD_BY_TYPE, StatCategory.ECONOMY_TRADE, hasKey: true);
            Register(
                StatIds.PAWN_TRADE_PROFIT,
                StatCategory.ECONOMY_TRADE,
                source: StatSource.CalculatedStat,
                statValueType: StatValueType.Float,
                calculatorType: typeof(PawnTradeProfitStatProvider),
                autoRegisterGameStat: false);
            Register(StatIds.PAWN_TRADES_INITIATED, StatCategory.ECONOMY_TRADE);
            Register(StatIds.PAWN_TRADES_EARNED, StatCategory.ECONOMY_TRADE, statValueType: StatValueType.Float);
            Register(StatIds.PAWN_TRADES_PAID, StatCategory.ECONOMY_TRADE, statValueType: StatValueType.Float);
            Register(StatIds.PAWN_INSPIRATIONS, StatCategory.MENTAL_MOOD);
            Register(StatIds.PAWN_INSPIRATIONS_BY_TYPE, StatCategory.MENTAL_MOOD, hasKey: true);
            Register(StatIds.PAWN_RITUALS_ATTENDED, StatCategory.RITUALS_ABILITIES);
            Register(StatIds.PAWN_RITUALS_ATTENDED_BY_TYPE, StatCategory.RITUALS_ABILITIES, hasKey: true);
            Register(StatIds.PAWN_RESCUES_PERFORMED, StatCategory.MEDICAL_HEALTH);
            Register(StatIds.PAWN_RESCUES_RECEIVED, StatCategory.MEDICAL_HEALTH);
            Register(StatIds.PAWN_FRIENDLY_FIRE, StatCategory.COMBAT);
            Register(StatIds.PAWN_FRIENDLY_FIRE_HITS, StatCategory.COMBAT);
            Register(StatIds.PAWN_MEALS_AT_TABLE, StatCategory.NEEDS_SURVIVAL);
            Register(StatIds.PAWN_MEALS_WITHOUT_TABLE, StatCategory.NEEDS_SURVIVAL);
            Register(StatIds.PAWN_NEAR_DEATH_EVENTS, StatCategory.MEDICAL_HEALTH);
            Register(StatIds.PAWN_DAYS_SICK, StatCategory.MEDICAL_HEALTH);
            Register(StatIds.PAWN_WALLS_BUILT, StatCategory.CONSTRUCTION);
            Register(StatIds.PAWN_FLOORS_LAID, StatCategory.CONSTRUCTION);
            Register(StatIds.PAWN_FLOORS_LAID_BY_TYPE, StatCategory.CONSTRUCTION, hasKey: true);
            Register(StatIds.PAWN_FLOORS_SMOOTHED, StatCategory.CONSTRUCTION);
            Register(StatIds.PAWN_WALLS_SMOOTHED, StatCategory.CONSTRUCTION);
            Register(StatIds.PAWN_NUTRITION_PRODUCED, StatCategory.NEEDS_SURVIVAL, statValueType: StatValueType.Float);
            Register(StatIds.PAWN_PLANTS_CUT_BY_TYPE, StatCategory.WORK_LABOR, hasKey: true);
            Register(StatIds.PAWN_ABILITIES_CAST, StatCategory.RITUALS_ABILITIES);
            Register(StatIds.PAWN_PSYCASTS_CAST, StatCategory.RITUALS_ABILITIES);
            Register(StatIds.PAWN_ABILITIES_CAST_BY_TYPE, StatCategory.RITUALS_ABILITIES, hasKey: true);
            Register(StatIds.PAWN_PSYCASTS_CAST_BY_TYPE, StatCategory.RITUALS_ABILITIES, hasKey: true);
            Register(StatIds.PAWN_DOWNED, StatCategory.COMBAT);
            Register(StatIds.PAWN_SKILL_LEVELS_GAINED, StatCategory.WORK_LABOR);

            if (ModsConfig.BiotechActive)
            {
                Register(StatIds.PAWN_TIME_AS_CHILD_IN_COLONY, StatCategory.TIME_ACTIVITY, source: StatSource.RecordDef, recordDefName: "TimeAsChildInColony", valueTransformerType: typeof(TimeTicksValueTransformer));
                Register(StatIds.PAWN_GENES_GAINED, StatCategory.MEDICAL_HEALTH);
                Register(StatIds.PAWN_GENES_GAINED_BY_TYPE, StatCategory.MEDICAL_HEALTH, hasKey: true);
            }

            if (ModsConfig.AnomalyActive)
            {
                Register(StatIds.PAWN_KILLS_ENTITY, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "KillsEntities");
                Register(StatIds.PAWN_HITS_CAUSING_DOWNED_ENTITIES, StatCategory.COMBAT, source: StatSource.RecordDef, recordDefName: "PawnsDownedEntities");
            }

            if (ModsConfig.OdysseyActive)
            {
                Register(StatIds.PAWN_TIMES_FISHED, StatCategory.WORK_LABOR);
                Register(StatIds.PAWN_FISH_CAUGHT, StatCategory.WORK_LABOR);
            }

            if (ModsConfig.RoyaltyActive)
            {
                Register(StatIds.PAWN_KILLS_EMPIRE, StatCategory.COMBAT);
            }
        }
    }
}
