using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.ActionPoints;

[TriggerActionGSQ_Holder]
public class ActionPointTriggerActions
{

    [TriggerAction]
    private static bool AddActionPoints(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var actionPointId, out error) ||
            !ArgUtility.TryGetInt(args, 2, out var amount, out error))
        {
            return false;
        }

        if (!ActionPointDataAsset.Exists(actionPointId))
        {
            error = $"Unknown action point ID {actionPointId}";
            return false;
        }
        ActionPointManager.AddActionPoint(actionPointId, amount);
        return true;
    }

    [TriggerAction]
    private static bool EnableActionPoint(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var actionPointId, out error))
        {
            return false;
        }
        if (!ActionPointDataAsset.Exists(actionPointId))
        {
            error = $"Unknown action point ID {actionPointId}";
            return false;
        }
        ActionPointManager.EnableActionPoint(actionPointId);
        return true;
    }
    
    [TriggerAction]
    private static bool DisableActionPoint(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var actionPointId, out error))
        {
            return false;
        }
        if (!ActionPointDataAsset.Exists(actionPointId))
        {
            error = $"Unknown action point ID {actionPointId}";
            return false;
        }
        ActionPointManager.DisableActionPoint(actionPointId);
        return true;
    }

    #region AlterActionsPoints
    [TriggerAction]
    private static bool SetReplenishTime(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var actionPointId, out error) ||
            !ArgUtility.TryGetFloat(args, 2, out var replenishTime, out error)) return false;

        if (!ActionPointDataAsset.Data.ContainsKey(actionPointId))
        {
            error = $"Unknown action point ID {actionPointId}";
            return false;
        }
        if (replenishTime < 0)
        {
            error = $"Negative replenish time {replenishTime}";
            return false;
        }
        ActionPointManager.SetReplenishTime(actionPointId, replenishTime);
        return true;
    }

    [TriggerAction]
    private static bool RestoreReplenishTime(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var actionPointId, out error))
            return false;
        if (!ActionPointDataAsset.Data.ContainsKey(actionPointId))
        {
            error = $"Unknown action point ID {actionPointId}";
            return false;
        }
        ActionPointManager.RestoreReplenishTime(actionPointId);
        return true;
    }
    
    [TriggerAction]
    private static bool SetMaxAmount(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var actionPointId, out error) ||
            !ArgUtility.TryGetInt(args, 2, out var maxAmount, out error))
        {
            return false;
        }
        if (!ActionPointDataAsset.Exists(actionPointId))
        {
            error = $"Unknown action point ID {actionPointId}";
            return false;
        }
        ActionPointManager.SetMaxAmount(actionPointId, maxAmount);
        return true;
    }
    
    [TriggerAction]
    private static bool RestoreMaxAmount(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var actionPointId, out error))
        {
            return false;
        }
        if (!ActionPointDataAsset.Exists(actionPointId))
        {
            error = $"Unknown action point ID {actionPointId}";
            return false;
        }
        ActionPointManager.RestoreMaxAmount(actionPointId);
        return true;
    }
    #endregion
}
