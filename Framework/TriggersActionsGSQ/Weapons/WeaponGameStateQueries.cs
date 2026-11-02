using CombatExtended.Framework.Core.Weapons;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Tools;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.TriggersActionsGSQ.Weapons;

[TriggerActionGSQ_Holder]
public static class WeaponGameStateQueries
{

    /// <summary>
    /// Check if player's current weapon matches one of provided weapon ids.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQuery"/>.</param>
    /// <returns>The GSQ evaluation result.</returns>
    [GSQ]
    private static bool CURRENT_WEAPON(string[] query, GameStateQueryContext context)
    {
        var currentWeapon = Utility.CurrentWeapon();
        if (currentWeapon is null) return false;
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var weapons, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }

        return weapons.Any(id => currentWeapon.ItemId == id);
    }

    /// <summary>
    /// Check if player's current weapon type matches one of provided weapon types.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQuery"/>.</param>
    /// <returns>The GSQ evaluation result.</returns>
    [GSQ]
    private static bool CURRENT_WEAPON_TYPE(string[] query, GameStateQueryContext context)
    {
        var currentWeapon = Utility.CurrentWeapon();
        if (currentWeapon is null) return false;
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var weaponTypes, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }

        var currentWeaponStrType = currentWeapon.type.Value switch
        {
            MeleeWeapon.club => "club",
            MeleeWeapon.dagger => "dagger",
            _ => "sword"
        };
        return weaponTypes.Any(type => currentWeaponStrType == type);
    }

    [GSQ]
    private static bool WEAPON_STATE(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var weaponId, out var error) ||
            !TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 2, out var states, out error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }

        return states.Contains(WeaponStateEnabler.GetWeaponState(weaponId));
    }
}
