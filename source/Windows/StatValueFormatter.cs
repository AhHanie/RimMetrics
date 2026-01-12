namespace RimMetrics
{
    public static class StatValueFormatter
    {
        public static string FormatValue(StatValueType valueType, int intValue, float floatValue)
        {
            return valueType == StatValueType.Float ? floatValue.ToString("0.##") : intValue.ToString();
        }
    }
}
