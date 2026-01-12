using Verse;
using RimWorld;

namespace RimMetrics
{
    public sealed class DefIconSelector : StatIconSelector
    {
        public override bool TryGetIcon(StatMeta meta, string key, out StatIconData iconData)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                iconData = null;
                return false;
            }

            var def = DefDatabase<ThingDef>.GetNamedSilentFail(key);
            var terrainDef = DefDatabase<TerrainDef>.GetNamedSilentFail(key);

            if (def != null)
            {
                ThingDef stuffDef = null;
                if (def.MadeFromStuff)
                {
                    stuffDef = GenStuff.DefaultStuffFor(def);
                }

                iconData = new StatIconData(def.uiIcon, true, def, stuffDef);
                return true;
            }

            if (terrainDef != null)
            {
                iconData = new StatIconData(terrainDef.uiIcon, true, terrainDef, null);
                return true;
            }

            iconData = null;
            return false;
        }
    }
}
