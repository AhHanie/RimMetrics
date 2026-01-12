using System;
using System.Collections.Generic;

namespace RimMetrics
{
    public static class StatIconSelectorResolver
    {
        private static readonly Dictionary<Type, StatIconSelector> Cache = new Dictionary<Type, StatIconSelector>();

        public static StatIconSelector GetSelector(StatMeta meta)
        {
            if (meta?.IconSelectorType == null)
            {
                return null;
            }

            if (Cache.TryGetValue(meta.IconSelectorType, out var renderer))
            {
                return renderer;
            }

            renderer = Activator.CreateInstance(meta.IconSelectorType) as StatIconSelector;
            Cache[meta.IconSelectorType] = renderer;
            return renderer;
        }
    }
}
