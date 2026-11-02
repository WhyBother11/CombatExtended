using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Buffs;

[TriggerActionGSQ_Holder]
public class BuffGameStateQueries
{

    [GSQ]
    private static bool BUFF_STACKS(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var buffId, out var error) ||
            !ArgUtility.TryGetInt(query, 2, out var min, out error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error); 
        }

        var max = int.MaxValue;
        if (ArgUtility.TryGetInt(query, 3, out var argMax, out _))
            max = argMax;
        var stacks = StackableBuffManager.GetStacks(buffId);
        return stacks >= min && stacks <= max;
    }
    
    [GSQ]
    private static bool BUFF_MAX_STACKS(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var buffId, out var error) ||
            !ArgUtility.TryGetInt(query, 2, out var min, out error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error); 
        }

        var max = int.MaxValue;
        if (ArgUtility.TryGetInt(query, 3, out var argMax, out _))
            max = argMax;
        var maxStacks = StackableBuffManager.GetMaxStacks(buffId);
        return maxStacks >= min && maxStacks <= max;
    }
}
