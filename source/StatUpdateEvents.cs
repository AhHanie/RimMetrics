using System;

namespace RimMetrics
{
    public readonly struct StatUpdateEvent
    {
        public readonly float Delta;
        public readonly StatRecord Record;

        public StatUpdateEvent(
            float delta,
            StatRecord record)
        {
            Delta = delta;
            Record = record;
        }
    }

    public static class StatUpdateEvents
    {
        public static event Action<StatUpdateEvent> StatUpdated;

        internal static void Raise(
            float delta,
            StatRecord record)
        {
            var handler = StatUpdated;
            if (handler == null)
            {
                return;
            }

            handler(new StatUpdateEvent(delta, record));
        }
    }
}
