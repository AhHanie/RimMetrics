using System;
using System.Diagnostics;
using Verse;

namespace RimMetrics
{
    public static class Logger
    {
        [Conditional("DEBUG")]
        public static void Message(string message)
        {
            Log.Message("[RimMetrics] " + message);
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            Log.Warning("[RimMetrics] " + message);
        }

        [Conditional("DEBUG")]
        public static void Error(string message)
        {
            Log.Error("[RimMetrics] " + message);
        }

        [Conditional("DEBUG")]
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
