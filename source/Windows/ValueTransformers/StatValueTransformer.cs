namespace RimMetrics
{
    public abstract class StatValueTransformer
    {
        public virtual bool TryTransformToString(
            StatMeta meta,
            string key,
            StatValueType valueType,
            int intValue,
            float floatValue,
            out string value)
        {
            value = null;
            return false;
        }
    }
}
