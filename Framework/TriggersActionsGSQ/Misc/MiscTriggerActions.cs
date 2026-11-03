using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;

namespace CombatExtended.Framework.TriggersActionsGSQ.Misc;

[TriggerActionGSQ_Holder]
public static class MiscTriggerActions
{
    [TriggerAction]
    private static bool CooldownAction(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var id, out error) ||
            !ArgUtility.TryGetFloat(args, 2, out var time, out error) ||
            !ArgUtility.TryGet(args, 3, out var action, out error))
            return false;
        if (!CooldownManager.Timers.TryGetValue(id, out _))
        {
            CooldownManager.Add(id, time);
            if (!TriggerActionManager.TryRunAction(action, out error, out _))
                return false;
        }
        return true;
    }
}
