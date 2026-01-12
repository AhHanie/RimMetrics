using RimWorld;

namespace RimMetrics
{
    public sealed class TimeTicksValueTransformer : StatValueTransformer
    {
        public override bool TryTransformToString(
            StatMeta meta,
            string key,
            StatValueType valueType,
            int intValue,
            float floatValue,
            out string value)
        {
            var ticks = valueType == StatValueType.Float ? floatValue : intValue;
            if (ticks <= 0f)
            {
                value = "0";
                return true;
            }

            var ticksInt = ticks >= int.MaxValue ? int.MaxValue : (int)ticks;
            value = ticksInt.ToStringTicksToPeriod(
                allowSeconds: true,
                shortForm: false,
                canUseDecimals: true,
                allowYears: true,
                canUseDecimalsShortForm: false);
            return true;
        }
    }
}
