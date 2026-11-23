using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Monsters;

[TriggerActionGSQ_Holder]
public static class MonsterBuffGameStateQueries
{
    public static readonly string MonsterBuffIdItemKey = "MonsterBuff_Id".PrependId();

    [GSQ]
    private static bool MONSTER_BUFF_ID(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var buffs, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterBuffIdItemKey, out var id,
                "Monster buff")) return false;
        return buffs.Contains(id);
    }

    [GSQ]
    private static bool MONSTER_BUFF_CONTEXT_TAG(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var contextTags, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterBuffIdItemKey, out var id,
                "Monster buff")) return false;
        return contextTags.Any(tag => MonsterBuffDataAsset.HasContextTag(id, tag));
    }
}
