using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Buffs;

[TriggerActionGSQ_Holder]
public static class BuffTriggerActions
{
    /// <summary>
    /// Trigger action to change maximum buff stacks for provided buff to provided limit, or to indefinite if <c>-2</c> provided.
    /// </summary>
    /// <param name="args">Trigger actions args.</param>
    /// <param name="ctx"><see cref="TriggerActionContext"/>.</param>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <returns>Whether trigger action was ran successfully.</returns>
    [TriggerAction]
    private static bool ChangeBuffStackLimit(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var buffId, out error) ||
            !ArgUtility.TryGetInt(args, 2, out var limit, out error))
        {
            return false;
        }

        if (limit != -2 && limit < 0)
        {
            error = "Stack limit is negative while not being equal to -2";
            return false;
        }
        if (limit == -2)
            StackableBuffManager.ChangeStackLimit(buffId, int.MaxValue);
        else StackableBuffManager.ChangeStackLimit(buffId, limit);
        return true;
    }

    /// <summary>
    /// Trigger action to add provided armour class amount to current player until the end of the day.
    /// </summary>
    /// <param name="args">Trigger actions args.</param>
    /// <param name="ctx"><see cref="TriggerActionContext"/>.</param>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <returns>Whether trigger action was ran successfully.</returns>
    [TriggerAction]
    private static bool AddArmourClass(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGetInt(args, 1, out var ac, out error))
        {
            return false;
        }

        ArmourClass.AddFarmerArmourClass(ac);

        return true;
    }
}
