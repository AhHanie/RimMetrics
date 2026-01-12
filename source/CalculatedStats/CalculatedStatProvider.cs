using RimMetrics.Components;

namespace RimMetrics.CalculatedStats
{
    public abstract class CalculatedStatProvider
    {
        public virtual int CalculateInt(string statId)
        {
            return 0;
        }

        public virtual float CalculateFloat(string statId)
        {
            return 0f;
        }

        public virtual int CalculateInt(string statId, Comp_PawnStats stats)
        {
            return CalculateInt(statId);
        }

        public virtual float CalculateFloat(string statId, Comp_PawnStats stats)
        {
            return CalculateFloat(statId);
        }
    }
}
