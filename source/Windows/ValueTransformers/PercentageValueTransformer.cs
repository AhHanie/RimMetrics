namespace RimMetrics
{
    public class PercentageValueTransformer : StatValueTransformer
    {
        public override bool TryTransformToString(
            StatMeta meta,
            string key,
            StatValueType valueType,
            int intValue,
            float floatValue,
            out string value)
        {
            value = null;
            if (valueType != StatValueType.Float)
            {
                return false;
            }

            value = $"{floatValue:0.##}%";
            return true;
        }
    }
}
