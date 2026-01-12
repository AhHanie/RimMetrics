using UnityEngine;
using Verse;

namespace RimMetrics
{
    public sealed class StatIconData
    {
        public readonly Texture2D Icon;
        public readonly bool UseDefIcon;
        public readonly Def Def;
        public readonly ThingDef StuffDef;

        public StatIconData(Texture2D icon, bool useDefIcon, Def def, ThingDef stuffDef)
        {
            Icon = icon;
            UseDefIcon = useDefIcon;
            Def = def;
            StuffDef = stuffDef;
        }
    }
}
