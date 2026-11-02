using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Monsters;

[TriggerActionGSQ_Holder]
public static class MonsterBuffGameStateQueries
{
    public static readonly string MonsterBuffIdItemKey = "MonsterBuff_Id".PrependId();
    public static readonly string MonsterBuffContextTagsItemKey = "MonsterBuff_ContextTags".PrependId();

    [GSQ]
    private static bool MONSTER_BUFF_ID(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var buffs, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (context.TargetItem?.modData.TryGetValue(MonsterBuffIdItemKey, out var id) ?? false)
        {
            return buffs.Contains(id);
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster buff id");
    }

    [GSQ]
    private static bool MONSTER_BUFF_CONTEXT_TAG(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var contextTags, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (context.TargetItem?.modData.TryGetValue(MonsterBuffContextTagsItemKey, out var contextTagsStr) == true)
        {
            var abilityContextTags = contextTagsStr.Split(' ');
            return contextTags.Any(abilityContextTags.Contains);
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster buff context tags");
    }
}
