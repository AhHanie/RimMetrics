using System;
using System.Collections.Generic;

namespace RimMetrics.CalculatedStats
{
    public static class CalculatedStatProviderCache
    {
        private static readonly Dictionary<Type, CalculatedStatProvider> Providers = new Dictionary<Type, CalculatedStatProvider>();
        public static CalculatedStatProvider GetOrCreate(Type providerType)
        {
            if (providerType == null)
            {
                return null;
            }

            if (!Providers.TryGetValue(providerType, out var provider))
            {
                provider = (CalculatedStatProvider)Activator.CreateInstance(providerType);
                Providers[providerType] = provider;
            }

            return provider;
        }
    }
}
