using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using GameStateQuery = StardewValley.GameStateQuery;

namespace CombatExtended.Framework.TriggersActionsGSQ.Abilities;

[TriggerActionGSQ_Holder]
public static class AbilityGameStateQueries
{
    
    public static readonly string AbilityIdItemKey = "Ability_Id".PrependId();
    public static readonly string AbilityContextTagsItemKey = "Ability_ContextTags".PrependId();

    [GSQ]
    private static bool ABILITY_ID(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var abilities, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }

        if (context.TargetItem?.modData.TryGetValue(AbilityIdItemKey, out var modDataAbilityId) ?? false)
        {
            return abilities.Contains(modDataAbilityId);
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain ability id");
    }

    [GSQ]
    private static bool ABILITY_CONTEXT_TAG(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var contextTags, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (context.TargetItem?.modData.TryGetValue(AbilityContextTagsItemKey, out var contextTagsStr) == true)
        {
            var abilityContextTags = ArgUtility.SplitBySpace(contextTagsStr);
            return contextTags.Any(abilityContextTags.Contains);
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain ability context tags");
    }

    [GSQ]
    private static bool ABILITY_ACTIVE(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var abilities, out var error))
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
        }
        return false;
    }
}
