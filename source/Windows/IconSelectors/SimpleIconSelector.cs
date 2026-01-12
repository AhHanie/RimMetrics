namespace RimMetrics
{
    public sealed class SimpleIconSelector : StatIconSelector
    {
        public override bool TryGetIcon(StatMeta meta, string key, out StatIconData iconData)
        {
            var icon = meta?.Icon;
            if (icon == null)
            {
                iconData = null;
                return false;
            }

            iconData = new StatIconData(icon, false, null, null);
            return true;
        }
    }
}
