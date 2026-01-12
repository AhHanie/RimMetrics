using System;
using System.Collections.Generic;

namespace RimMetrics
{
    public static class StatValueTransformerResolver
    {
        private static readonly Dictionary<Type, StatValueTransformer> Cache = new Dictionary<Type, StatValueTransformer>();

        public static StatValueTransformer GetTransformer(StatMeta meta)
        {
            if (meta?.ValueTransformerType == null)
            {
                return null;
            }

            if (Cache.TryGetValue(meta.ValueTransformerType, out var transformer))
            {
                return transformer;
            }

            transformer = Activator.CreateInstance(meta.ValueTransformerType) as StatValueTransformer;
            Cache[meta.ValueTransformerType] = transformer;
            return transformer;
        }
    }
}
