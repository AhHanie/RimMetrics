using System;
using Verse;

namespace RimMetrics
{
    public static class UserLogger
    {
        public static void Message(string message)
        {
            Log.Message("[RimMetrics] " + message);
        }

        public static void Warning(string message)
        {
            Log.Warning("[RimMetrics] " + message);
        }

        public static void Error(string message)
        {
            Log.Error("[RimMetrics] " + message);
        }

        public static void Exception(Exception exception, string context = null)
        {
            if (exception == null)
            {
                return;
            }

            var prefix = string.IsNullOrWhiteSpace(context) ? "[RimMetrics] " : "[RimMetrics] " + context + ": ";
            Log.Error(prefix + exception);
        }
    }
}
