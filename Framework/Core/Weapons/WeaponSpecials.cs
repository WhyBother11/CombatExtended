using CombatExtended.Framework.TriggersActionsGSQ.Util;
using HarmonyLib;
using StardewValley.Tools;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Core.Weapons;

[HarmonyPatch]
public static class WeaponSpecials
{

    [HarmonyPatch(typeof(MeleeWeapon), "doAnimateSpecialMove")]
    [HarmonyPostfix]
    private static void MeleeWeapon_doAnimateSpecialMove_Postfix()
    {
        TriggerActionManager.Raise(Triggers.SpecialMoveTriggered);
    }
}
