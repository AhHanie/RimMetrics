using System;
using System.Collections.Generic;
using Verse;

namespace RimMetrics
{
    public abstract class ModCompat
    {
        private static readonly List<string> RegisteredCompatModNames = new List<string>();

        public abstract bool IsEnabled();
        public abstract void Init();
        public abstract string GetModPackageIdentifier();

        public static IReadOnlyList<string> GetRegisteredCompatModNames()
        {
            return RegisteredCompatModNames;
        }

        public static void RegisterCompatMod(ModCompat compat)
        {
            var packageId = compat.GetModPackageIdentifier();
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }

            var meta = ModLister.GetActiveModWithIdentifier(packageId);
            if (meta == null)
            {
                return;
            }

            var displayName = meta?.Name;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return;
            }

            if (!RegisteredCompatModNames.Contains(displayName))
            {
                RegisteredCompatModNames.Add(displayName);
            }
        }
    }
}
