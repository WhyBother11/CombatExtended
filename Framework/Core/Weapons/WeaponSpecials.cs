using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using HarmonyLib;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Monsters;
using StardewValley.Tools;
using StardewValley.Triggers;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.Core.Weapons;

[HarmonyPatch]
public static class WeaponSpecials
{

    [HarmonyPatch(typeof(MeleeWeapon), "doAnimateSpecialMove")]
    [HarmonyPrefix]
    private static bool MeleeWeapon_doAnimateSpecialMove_Prefix(MeleeWeapon __instance)
    {
        if (!WeaponExtensionDataAsset.SpecialExtensions.TryGetValue(__instance.ItemId, out var data)) return true;
        if (!data.PreventBase)
        {
            if (!__instance.lastUser.IsLocalPlayer) return true;
            switch (__instance.type.Value)
            {
                case MeleeWeapon.defenseSword:
                    MeleeWeapon.defenseCooldown = 1500;
                    if (__instance.lastUser.professions.Contains(28)) 
                        MeleeWeapon.defenseCooldown /= 2;
                    if (__instance.hasEnchantmentOfType<ArtfulEnchantment>()) 
                        MeleeWeapon.defenseCooldown /= 2;
                    break;
                case MeleeWeapon.club:
                    MeleeWeapon.clubCooldown = 6000;
                    if (__instance.lastUser.professions.Contains(28)) 
                        MeleeWeapon.clubCooldown /= 2;
                    if (__instance.hasEnchantmentOfType<ArtfulEnchantment>()) 
                        MeleeWeapon.clubCooldown /= 2;
                    break;
                case MeleeWeapon.dagger:
                    MeleeWeapon.daggerCooldown = 3000;
                    if (__instance.lastUser.professions.Contains(28)) 
                        MeleeWeapon.daggerCooldown /= 2;
                    if (__instance.hasEnchantmentOfType<ArtfulEnchantment>()) 
                        MeleeWeapon.daggerCooldown /= 2;
                    break;
            }
            return false;
        }
        return true;
    }
    
    [HarmonyPatch(typeof(MeleeWeapon), "doAnimateSpecialMove")]
    [HarmonyPostfix]
    private static void MeleeWeapon_doAnimateSpecialMove_Postfix(MeleeWeapon __instance)
    {
        TriggerActionManager.Raise(Triggers.SpecialUsed);
        if (!WeaponExtensionDataAsset.SpecialExtensions.TryGetValue(__instance.ItemId, out var data)) return;
        if (data.TriggerActionCondition is null || GameStateQuery.CheckConditions(data.TriggerActionCondition))
        {
            data.ActionsOnTrigger?.ForEach(action => Utility.RunAndLogAction(action, $"weapon '{__instance.ItemId}'"));
        }
    }

    [HarmonyPatch(typeof(Monster), nameof(Monster.parried))]
    [HarmonyPostfix]
    private static void Monster_parried_Postfix(Monster __instance, Farmer who)
    {
        var item = ItemRegistry.Create("(O)168");
        item.PackMonsterInfo(__instance);
        TriggerActionManager.Raise(Triggers.Parried, new object[] { __instance }, 
            location: __instance.currentLocation, player: who, targetItem: item);
    }
}
