using RimMetrics.Helpers;

namespace RimMetrics
{
    public sealed class QualityIconSelector : StatIconSelector
    {
        public override bool TryGetIcon(StatMeta meta, string key, out StatIconData iconData)
        {
            var icon = default(UnityEngine.Texture2D);

            switch (key)
            {
                case "Awful":
                    icon = ResourcesAssets.QualityAwful;
                    break;
                case "Poor":
                    icon = ResourcesAssets.QualityPoor;
                    break;
                case "Normal":
                    icon = ResourcesAssets.QualityNormal;
                    break;
                case "Good":
                    icon = ResourcesAssets.QualityGood;
                    break;
                case "Excellent":
                    icon = ResourcesAssets.QualityExcellent;
                    break;
                case "Mastework":
                    icon = ResourcesAssets.QualityMasterwork;
                    break;
                case "Legendary":
                    icon = ResourcesAssets.QualityLegendary;
                    break;
                default:
                    break;
            }

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
