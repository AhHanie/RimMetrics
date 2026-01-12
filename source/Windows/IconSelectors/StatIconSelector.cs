namespace RimMetrics
{
    public abstract class StatIconSelector
    {
        public abstract bool TryGetIcon(StatMeta meta, string key, out StatIconData iconData);
    }
}
