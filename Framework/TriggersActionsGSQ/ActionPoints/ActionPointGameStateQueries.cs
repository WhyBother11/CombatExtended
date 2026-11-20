using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.ActionPoints;

[TriggerActionGSQ_Holder]
public class ActionPointGameStateQueries
{

    [GSQ]
    private static bool ACTION_POINTS(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var id, out var error) ||
            !ArgUtility.TryGetInt(query, 2, out var min, out error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!ActionPointDataAsset.Exists(id))
            return GameStateQuery.Helpers.ErrorResult(query, $"Unknown action point id: {id}");
        
        ArgUtility.TryGetOptionalInt(query, 3, out var max, out _, int.MaxValue);
        var pointAmount = ActionPointManager.GetActionPointAmount(id);
        return pointAmount >= min && pointAmount <= max;
    }
    
    [GSQ]
    private static bool MAX_ACTION_POINTS(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var id, out var error) ||
            !ArgUtility.TryGetInt(query, 2, out var min, out error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!ActionPointDataAsset.Exists(id))
            return GameStateQuery.Helpers.ErrorResult(query, $"Unknown action point id: {id}");
        ArgUtility.TryGetOptionalInt(query, 3, out var max, out _, int.MaxValue);
        var maxPointAmount = ActionPointManager.ActionPoints[id].MaxAmount;
        return maxPointAmount >= min && maxPointAmount <= max;
    }
}
