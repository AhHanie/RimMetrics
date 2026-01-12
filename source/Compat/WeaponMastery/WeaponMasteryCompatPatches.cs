using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimMetrics.Components;
using Verse;

namespace RimMetrics
{
    [HarmonyPatch]
    public static class WeaponMasteryCompatPatches
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("SK_WeaponMastery.MasteryWeaponComp+<>c__DisplayClass15_0");
            return AccessTools.Method(type, "<AddExp>b__0");
        }

        public static bool Prepare()
        {
            return ModsConfig.IsActive("sk.weaponmastery");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var displayClassType = AccessTools.TypeByName("SK_WeaponMastery.MasteryWeaponComp+<>c__DisplayClass15_0");
            var pawnField = AccessTools.Field(displayClassType, "pawn");
            var generateWeaponName = AccessTools.Method("SK_WeaponMastery.MasteryWeaponComp:GenerateWeaponName");
            var recordMastery = AccessTools.Method(typeof(WeaponMasteryCompatPatches), nameof(RecordMasteryLevelUp));
            var recordRename = AccessTools.Method(typeof(WeaponMasteryCompatPatches), nameof(RecordWeaponRenamed));
            var insertedMastery = false;
            var insertedRename = false;

            code.InsertRange(0, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, pawnField),
                new CodeInstruction(OpCodes.Call, recordMastery),
            });
            insertedMastery = true;

            for (var i = 0; i < code.Count; i++)
            {
                if (!code[i].Calls(generateWeaponName))
                {
                    continue;
                }

                code.InsertRange(i + 1, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, pawnField),
                    new CodeInstruction(OpCodes.Call, recordRename),
                });
                insertedRename = true;
                break;
            }

            if (insertedMastery && insertedRename)
            {
                Logger.Message("WeaponMastery compat patch applied.");
            }
            else
            {
                Logger.Warning("WeaponMastery compat patch failed to apply.");
            }

            return code;
        }

        private static void RecordMasteryLevelUp(Pawn pawn)
        {
            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(WeaponMasteryCompat.PAWN_WEAPON_BOND_MASTERIES_TOTAL);
        }

        private static void RecordWeaponRenamed(Pawn pawn)
        {
            if (!pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(WeaponMasteryCompat.PAWN_TOTAL_WEAPONS_RENAMED);
        }
    }

    [HarmonyPatch]
    public static class WeaponMasteryClassCompatPatches
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("SK_WeaponMastery.MasteryCompData:AddExp");
        }

        public static bool Prepare()
        {
            return ModsConfig.IsActive("sk.weaponmastery");
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);
            var invoke = AccessTools.Method(typeof(System.Action<int>), nameof(System.Action<int>.Invoke));
            var recordClass = AccessTools.Method(typeof(WeaponMasteryClassCompatPatches), nameof(RecordClassMasteryLevelUp));
            var inserted = false;

            for (var i = 0; i < code.Count; i++)
            {
                if (!code[i].Calls(invoke))
                {
                    continue;
                }

                code.InsertRange(i, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Call, recordClass),
                });
                inserted = true;
                break;
            }

            if (inserted)
            {
                Logger.Message("WeaponMastery class compat patch applied.");
            }
            else
            {
                Logger.Warning("WeaponMastery class compat patch failed to apply.");
            }

            return code;
        }

        private static void RecordClassMasteryLevelUp(System.Action<int> postLevelUp)
        {
            var pawn = TryGetPawnFromDelegate(postLevelUp);
            if (pawn == null || !pawn.TryGetComp(out Comp_PawnStats comp))
            {
                return;
            }

            comp.IncrementTotalInt(WeaponMasteryCompat.PAWN_WEAPON_CLASS_MASTERIES_TOTAL);
        }

        private static Pawn TryGetPawnFromDelegate(System.Action<int> postLevelUp)
        {
            if (postLevelUp == null)
            {
                return null;
            }

            var target = postLevelUp.Target;
            if (target == null)
            {
                return null;
            }

            var pawnField = AccessTools.Field(target.GetType(), "pawn");
            return pawnField?.GetValue(target) as Pawn;
        }
    }
}
