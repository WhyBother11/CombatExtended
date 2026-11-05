using CombatExtended.Framework.Events.Args;
using CombatExtended.Framework.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Tools;

namespace CombatExtended.Framework.Managers;

/// <summary>
/// Manager to handle player weapons.
/// </summary>
public class WeaponManager
{
    public static WeaponManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<WeaponManager> PerScreenInstance = new(()  => new WeaponManager());
    
    private const int BaseTicks = 5;

    private MeleeWeapon? _weapon;
    
    public static void Register(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += PerScreenInstance.Value.OnUpdateTicked_HandleCurrentWeapon;
    }
    
    public static void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= PerScreenInstance.Value.OnUpdateTicked_HandleCurrentWeapon;
    }

    /// <summary>
    /// Check current player weapon and determine if the player's weapon was equipped, unequipped or switched and raise
    /// <see cref="Event.OnWeaponEquipped"/>, <see cref="Event.OnWeaponUnequipped"/> and <see cref="Event.OnWeaponSwitched"/> accordingly.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e"><see cref="UpdateTickedEventArgs"/>.</param>
    private void OnUpdateTicked_HandleCurrentWeapon(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree) return;
        
        var weapon = Utility.CurrentWeapon();
        if (weapon is null && _weapon is not null)
        {
            var args = new WeaponUnequippedEventArgs(_weapon);
            Event.RaiseWeaponUnequipped(args);
            _weapon = null;
        }
        else if (weapon is not null && _weapon is null)
        {
            var args = new WeaponEquippedEventArgs(weapon);
            Event.RaiseWeaponEquipped(args);
            _weapon = weapon;
        } else if (weapon is not null && _weapon is not null && weapon.ItemId != _weapon.ItemId)
        {
            var args = new WeaponSwitchedEventArgs(weapon, _weapon);
            Event.RaiseWeaponSwitched(args);
            _weapon = weapon;
        }
    }
}
