using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Locations;

namespace CombatExtended.Framework.TriggersActionsGSQ.Misc;

[TriggerActionGSQ_Holder]
public static class MiscGameStateQueries
{

    [GSQ]
    private static bool MINE_LEVEL(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGetInt(query, 1, out var min, out var error))
            return GameStateQuery.Helpers.ErrorResult(query, error);
        var max = int.MaxValue;
        if (ArgUtility.TryGetInt(query, 2, out var argMax, out _))
            max = argMax;
        if (Game1.player.currentLocation is MineShaft mine)
            return mine.mineLevel >= min && mine.mineLevel <= max;
        return false;
    }
    
    [GSQ]
    private static bool VOLCANO_DUNGEON_LEVEL(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGetInt(query, 1, out var min, out var error))
            return GameStateQuery.Helpers.ErrorResult(query, error);
        var max = int.MaxValue;
        if (ArgUtility.TryGetInt(query, 2, out var argMax, out _))
            max = argMax;
        if (Game1.player.currentLocation is VolcanoDungeon volcano)
            return volcano.level.Value >= min && volcano.level.Value <= max;
        return false;
    }
}
