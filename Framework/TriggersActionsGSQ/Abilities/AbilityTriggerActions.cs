using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Abilities;

[TriggerActionGSQ_Holder]
public static class AbilityTriggerActions
{
    [TriggerAction]
    private static bool AddWeaponAbility(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var abilityId, out error) ||
            !ArgUtility.TryGet(args, 2, out var weaponId, out error))
        {
            return false;
        }

        if (!AbilityDataAsset.Exists(abilityId))
        {
            error = $"Unknown ability id {abilityId}.";
            return false;
        }

        AbilityManager.AddWeaponAbility(weaponId, Ability.Create(abilityId));
        return true;
    }

    [TriggerAction]
    private static bool RemoveWeaponAbility(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var abilityId, out error) ||
            !ArgUtility.TryGet(args, 2, out var weaponId, out error))
        {
            return false;
        }
        if (!AbilityDataAsset.Exists(abilityId))
        {
            error = $"Unknown ability id {abilityId}";
            return false;
        }
        AbilityManager.RemoveWeaponAbility(weaponId, abilityId);
        return true;
    }

    #region AlterAbilities
    
    [TriggerAction]
    private static bool SetAbilityCooldown(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var abilityId, out error) ||
            !ArgUtility.TryGetFloat(args, 2, out var cooldown, out error))
        {
            return false;
        }
        if (!AbilityDataAsset.Exists(abilityId))
        {
            error = $"Unknown ability id {abilityId}";
            return false;
        }
        if (cooldown < 0)
        {
            error = "Negative cooldown provided";
            return false;
        }
        
        AbilityManager.SetCooldown(abilityId, cooldown);
        return true;
    }
    
    [TriggerAction]
    private static bool RestoreAbilityCooldown(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var abilityId, out error))
        {
            return false;
        }
        if (!AbilityDataAsset.Exists(abilityId))
        {
            error = $"Unknown ability id {abilityId}";
            return false;
        }
        
        AbilityManager.RestoreCooldown(abilityId);
        return true;
    }
    
    [TriggerAction]
    private static bool SetAbilityDuration(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var abilityId, out error) ||
            !ArgUtility.TryGetFloat(args, 2, out var duration, out error))
        {
            return false;
        }
        if (!AbilityDataAsset.Exists(abilityId))
        {
            error = $"Unknown ability id {abilityId}";
            return false;
        }
        
        AbilityManager.SetDuration(abilityId, duration);
        return true;
    }
    
    [TriggerAction]
    private static bool RestoreAbilityDuration(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var abilityId, out error))
        {
            return false;
        }
        if (!AbilityDataAsset.Exists(abilityId))
        {
            error = $"Unknown ability id {abilityId}";
            return false;
        }
        
        AbilityManager.RestoreDuration(abilityId);
        return true;
    }
    #endregion
}
