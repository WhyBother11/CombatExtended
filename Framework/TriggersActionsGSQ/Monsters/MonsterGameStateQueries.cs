using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Monsters;

[TriggerActionGSQ_Holder]
public static class MonsterGameStateQueries
{
    public static readonly string MonsterNameItemKey = "Monster_Name".PrependId();
    public static readonly string MonsterHealthItemKey = "Monster_Health".PrependId();
    public static readonly string MonsterTagsItemKey = "Monster_Tags".PrependId();
    public static readonly string MonsterBuffsItemKey = "Monster_Buffs".PrependId();
    public static readonly string MonsterBuffStacksItemKey = "Monster_Buffs_Stacks".PrependId();

    /// <summary>
    /// Check monster's health from <paramref name="context"/>'s <see cref="GameStateQueryContext.CustomFields"/>.
    /// </summary>
    /// <param name="query">GSQ query.</param>
    /// <param name="context">See <see cref="GameStateQuery"/>.</param>
    /// <returns>GSQ evaluation result</returns>
    [GSQ]
    private static bool MONSTER_HEALTH(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var arg1, out var error) ||
            !ArgUtility.TryGet(query, 2, out var arg2, out error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        var minHp = arg1 == "_" ? int.MinValue : int.Parse(arg1);
        var maxHp = arg2 == "_" ? int.MaxValue : int.Parse(arg2);
        if (context.TargetItem?.modData.TryGetValue(MonsterHealthItemKey, out var healthStr) ?? false)
        {
            var monsterHealth = int.Parse(healthStr);
            return monsterHealth >= minHp && monsterHealth <= maxHp;
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster health.");
    }

    /// <summary>
    /// Check monster's tags from <paramref name="context"/>'s <see cref="GameStateQueryContext.CustomFields"/>.
    /// </summary>
    /// <param name="query">GSQ</param>
    /// <param name="context"><see cref="GameStateQuery"/></param>
    /// <returns>GSQ result</returns>
    [GSQ]
    private static bool MONSTER_HAS_TAG(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var tags, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (context.TargetItem?.modData.TryGetValue(MonsterTagsItemKey, out var monsterTagsStr) ?? false)
        {
            var monsterTags = ArgUtility.SplitBySpace(monsterTagsStr);
            return tags.Any(monsterTags.Contains);
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster tags.");
    }

    [GSQ]
    private static bool MONSTER_NAME(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var monsterNames, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (context.TargetItem?.modData.TryGetValue(MonsterNameItemKey, out var monsterName) ?? false)
        {
            return monsterNames.Contains(monsterName);
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster name.");
    }
    
    [GSQ]
    private static bool MONSTER_HAS_BUFF(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var buffs, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (context.TargetItem?.modData.TryGetValue(MonsterBuffsItemKey, out var monsterBuffsStr) ?? false)
        {
            var itemMonsterBuffs = monsterBuffsStr.Split(' ');
            return buffs.Any(itemMonsterBuffs.Contains);
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster buffs.");
    }

    [GSQ]
    private static bool MONSTER_BUFF_STACKS(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var monsterBuffId, out var error))
            return GameStateQuery.Helpers.ErrorResult(query, error);
        if (!ArgUtility.TryGetInt(query, 1, out var min, out error))
            return GameStateQuery.Helpers.ErrorResult(query, error);
        var max = int.MaxValue;
        if (ArgUtility.TryGetInt(query, 1, out var argMax, out error))
            max = argMax;
        if (context.TargetItem?.modData.TryGetValue(MonsterBuffStacksItemKey, out var monsterBuffsStacksStr) ?? false)
        {
            var itemMonsterBuffStacks = monsterBuffsStacksStr.Split(' ');
            for (var i = 0; i < itemMonsterBuffStacks.Length; i+=2)
            {
                var itemMonsterBuffId = itemMonsterBuffStacks[i];
                var stackAmount = int.Parse(itemMonsterBuffStacks[i+1]);
                if (itemMonsterBuffId == monsterBuffId)
                    return stackAmount >= min && stackAmount <= max;
            }
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain amount monster buffs.");
    }
}
