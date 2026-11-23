using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley.Delegates;
using GameStateQuery = StardewValley.GameStateQuery;

namespace CombatExtended.Framework.TriggersActionsGSQ.Abilities;

[TriggerActionGSQ_Holder]
public static class AbilityGameStateQueries
{
    
    public static readonly string AbilityIdItemKey = "Ability_Id".PrependId();

    [GSQ]
    private static bool ABILITY_ID(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var abilities, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!RequireContextModData(query, context.TargetItem?.modData, AbilityIdItemKey, out var modDataAbilityId,
                "Ability")) return false;
        return abilities.Contains(modDataAbilityId);
    }

    [GSQ]
    private static bool ABILITY_CONTEXT_TAG(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var contextTags, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!RequireContextModData(query, context.TargetItem?.modData, AbilityIdItemKey, out var modDataAbilityId,
                "Ability")) return false;
        return contextTags.Any(tag => AbilityDataAsset.HasContextTag(modDataAbilityId, tag));
    }

    [GSQ]
    private static bool ABILITY_ACTIVE(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var abilities, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        foreach (var abilityId in abilities)
        {
            if (AbilityManager.WeaponAbilities.TryGetValue(abilityId, out var ability))
                if (ability is ContinuousAbility { IsActive: true }) return true;
            foreach (var weaponId in AbilityManager.PreviousWeaponAbilities.Keys)
            {
                if (AbilityManager.PreviousWeaponAbilities[weaponId].TryGetValue(abilityId, out ability))
                    if (ability is ContinuousAbility { IsActive: true }) return true;
            }
            if (AbilityManager.FarmerAbilities.TryGetValue(abilityId, out var playerAbility))
                if (playerAbility is ContinuousAbility { IsActive: true }) return true;
        }
        return false;
    }
}
