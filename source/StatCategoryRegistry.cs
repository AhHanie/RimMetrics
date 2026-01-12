using System.Collections.Generic;

namespace RimMetrics
{
    public static class StatCategoryRegistry
    {
        private static readonly Dictionary<string, int> Orders = new Dictionary<string, int>();
        private static readonly List<string> Categories = new List<string>();

        static StatCategoryRegistry()
        {
            RegisterDefaults();
        }

        public static void RegisterCategory(string categoryId, int displayOrder = -1)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                return;
            }

            if (displayOrder < 0)
            {
                displayOrder = Orders.Count;
            }

            if (!Orders.ContainsKey(categoryId))
            {
                Categories.Add(categoryId);
            }

            Orders[categoryId] = displayOrder;
        }

        public static bool IsRegistered(string categoryId)
        {
            return !string.IsNullOrWhiteSpace(categoryId) && Orders.ContainsKey(categoryId);
        }

        public static int GetOrder(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                return int.MaxValue;
            }

            return Orders.TryGetValue(categoryId, out var order) ? order : int.MaxValue;
        }

        public static IEnumerable<string> GetAllCategories()
        {
            return Categories;
        }

        public static int Compare(string left, string right)
        {
            var orderComparison = GetOrder(left).CompareTo(GetOrder(right));
            if (orderComparison != 0)
            {
                return orderComparison;
            }

            return string.CompareOrdinal(left ?? string.Empty, right ?? string.Empty);
        }

        private static void RegisterDefaults()
        {
            RegisterCategory(StatCategory.COMBAT);
            RegisterCategory(StatCategory.DAMAGE_DEFENSE);
            RegisterCategory(StatCategory.EQUIPMENT);
            RegisterCategory(StatCategory.CRAFTING_PRODUCTION);
            RegisterCategory(StatCategory.CONSTRUCTION);
            RegisterCategory(StatCategory.WORK_LABOR);
            RegisterCategory(StatCategory.ANIMALS);
            RegisterCategory(StatCategory.MEDICAL_HEALTH);
            RegisterCategory(StatCategory.SOCIAL_IDEOLOGY);
            RegisterCategory(StatCategory.MENTAL_MOOD);
            RegisterCategory(StatCategory.RITUALS_ABILITIES);
            RegisterCategory(StatCategory.RESEARCH);
            RegisterCategory(StatCategory.ECONOMY_TRADE);
            RegisterCategory(StatCategory.TRAVEL_MOVEMENT);
            RegisterCategory(StatCategory.NEEDS_SURVIVAL);
            RegisterCategory(StatCategory.TIME_ACTIVITY);
            RegisterCategory(StatCategory.MISC_EVENTS);
        }
    }
}
