using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using RimMetrics.Components;
using Verse;

namespace RimMetrics
{
    public class Mod: Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "RimMetrics.LoadingLabel", doAsynchronously: true, null);
        }

        public void Init()
        {
            GetSettings<ModSettings>();
            new Harmony("sk.rimmetrics").PatchAll();
            AttachPawnStatsComp();
            InitCompat();
        }

        public override string SettingsCategory()
        {
            return "RimMetrics.SettingsTitle".Translate();
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect inRect)
        {
            ModSettingsWindow.Draw(inRect);
            base.DoSettingsWindowContents(inRect);
        }

        private static void AttachPawnStatsComp()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def?.race == null)
                {
                    continue;
                }

                if (def.comps == null)
                {
                    def.comps = new List<CompProperties>();
                }

                if (!def.comps.Any(c => c?.compClass == typeof(Comp_PawnStats)))
                {
                    def.comps.Add(new CompProperties_PawnStats());
                }
            }
        }

        private static void InitCompat()
        {
            var types = typeof(Mod).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (type == null || type.IsAbstract || !typeof(ModCompat).IsAssignableFrom(type))
                {
                    continue;
                }

                var compat = Activator.CreateInstance(type) as ModCompat;
                if (compat == null)
                {
                    continue;
                }

                if (!compat.IsEnabled())
                {
                    continue;
                }

                ModCompat.RegisterCompatMod(compat);

                compat.Init();
            }
        }
    }
}
