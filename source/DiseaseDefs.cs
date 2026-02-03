using System.Collections.Generic;
using Verse;

namespace RimMetrics
{
    public static class DiseaseDefs
    {
        private static readonly HashSet<string> BaseDiseaseDefNames = new HashSet<string>
        {
            "Flu",
            "WoundInfection",
            "LungRot",
            "Malaria",
            "Plague",
            "SleepingSickness",
            "FibrousMechanites",
            "GutWorms",
            "MuscleParasites",
            "SensoryMechanites",
            "Scaria"
        };

        private static readonly HashSet<string> RoyaltyDiseaseDefNames = new HashSet<string>
        {
            "BloodRot",
            "ParalyticAbasia"
        };

        private static readonly HashSet<string> BiotechDiseaseDefNames = new HashSet<string>
        {
            "InfantIllness"
        };

        private static HashSet<HediffDef> cachedDiseases;

        public static HashSet<HediffDef> GetAllDiseaseDefs()
        {
            if (cachedDiseases != null)
            {
                return cachedDiseases;
            }

            var diseases = new HashSet<HediffDef>();

            AddDefs(BaseDiseaseDefNames, diseases);

            if (ModsConfig.RoyaltyActive)
            {
                AddDefs(RoyaltyDiseaseDefNames, diseases);
            }

            if (ModsConfig.BiotechActive)
            {
                AddDefs(BiotechDiseaseDefNames, diseases);
            }

            cachedDiseases = diseases;
            return cachedDiseases;
        }

        public static bool IsDisease(HediffDef def)
        {
            if (def == null)
            {
                return false;
            }

            return GetAllDiseaseDefs().Contains(def);
        }

        private static void AddDefs(HashSet<string> defNames, HashSet<HediffDef> defs)
        {
            foreach (var defName in defNames)
            {
                var def = DefDatabase<HediffDef>.GetNamedSilentFail(defName);
                if (def != null)
                {
                    defs.Add(def);
                }
            }
        }
    }
}
