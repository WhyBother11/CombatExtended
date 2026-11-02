using CombatExtended.Framework.Core.Weapons;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Tools;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.TriggersActionsGSQ.Weapons;

[TriggerActionGSQ_Holder]
public static class WeaponTriggerActions
{
    
    /// <summary>
    /// Trigger action to change weapon state for provided weapon ID to provided value, or restore default if <c>_default</c> was provided.
    /// </summary>
    /// <param name="args">Trigger actions args.</param>
    /// <param name="ctx"><see cref="TriggerActionContext"/>.</param>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <returns>Whether trigger action was ran successfully.</returns>
    [TriggerAction]
    private static bool SetWeaponState(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var weaponId, out error) ||
            !ArgUtility.TryGet(args, 2, out var state, out error))
        {
            return false;
        }
        var weapons = Game1.player.Items.OfType<MeleeWeapon>()
            .Where(weapon => weapon.ItemId == weaponId).ToList();
        if (state == "_default")
            weapons.ForEach(WeaponStateEnabler.RestoreToDefault);
        else
            weapons.ForEach(weapon =>
            {
                Utility.TryWithWarning<ArgumentException>(Log, () =>
                {
                    var stateData = WeaponStateEnabler.GetWeaponStateData(weapon.ItemId, state);
                    WeaponStateEnabler.ApplyState(weapon, stateData); 
                });
            });
        return true;
    }
}
