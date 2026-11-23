using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
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
    public static readonly string MonsterStunTimeItemKey = "Monster_StunTime".PrependId();
    public static readonly string MonsterStatusesItemKey = "Monster_Statuses".PrependId();

    /// <summary>
    /// Check if monster's health is between provided arguments.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQueryContext"/>.</param>
    /// <returns>The GSQ evaluation result</returns>
    [GSQ]
    private static bool MONSTER_HEALTH(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGetInt(query, 1, out var minHp, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (minHp <= 0) minHp = int.MinValue;
        ArgUtility.TryGetOptionalInt(query, 2, out var maxHp, out _, int.MaxValue);
        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterHealthItemKey, out var healthStr,
                "Monster")) return false;
        var monsterHealth = int.Parse(healthStr);
        return monsterHealth >= minHp && monsterHealth <= maxHp;
    }

    /// <summary>
    /// Check if monster has one of provided monster tags.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQueryContext"/>.</param>
    /// <returns>The GSQ evaluation result</returns>
    [GSQ]
    private static bool MONSTER_HAS_TAG(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var tags, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterTagsItemKey, out var monsterTagsStr,
                "Monster")) return false;
        var monsterTags = ArgUtility.SplitBySpace(monsterTagsStr);
        return tags.Any(monsterTags.Contains);
    }

    /// <summary>
    /// Check if monster's name matches one of provided arguments.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQueryContext"/>.</param>
    /// <returns>The GSQ evaluation result</returns>
    [GSQ]
    private static bool MONSTER_NAME(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var monsterNames, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterNameItemKey, out var monsterName,
                "Monster")) return false;
        return monsterNames.Contains(monsterName);
    }
    
    /// <summary>
    /// Check if monster has one of provided monster buffs.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQueryContext"/>.</param>
    /// <returns>The GSQ evaluation result</returns>
    [GSQ]
    private static bool MONSTER_HAS_BUFF(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var buffs, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }

        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterBuffsItemKey, out var monsterBuffsStr,
                "Monster")) return false;
        var itemMonsterBuffs = ArgUtility.SplitBySpace(monsterBuffsStr);
        return buffs.Any(itemMonsterBuffs.Contains);
    }

    /// <summary>
    /// Check if monster has specified amount of stacks for provided monster buff.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQueryContext"/>.</param>
    /// <returns>The GSQ evaluation result</returns>
    [GSQ]
    private static bool MONSTER_BUFF_STACKS(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var monsterBuffId, out var error))
            return GameStateQuery.Helpers.ErrorResult(query, error);
        if (!ArgUtility.TryGetInt(query, 2, out var min, out error))
            return GameStateQuery.Helpers.ErrorResult(query, error);
        ArgUtility.TryGetOptionalInt(query, 3, out var max, out _, int.MaxValue);
        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterBuffStacksItemKey, out var monsterBuffsStacksStr,
                "Monster")) return false;
        var itemMonsterBuffStacks = monsterBuffsStacksStr.Split(' ');
        for (var i = 0; i < itemMonsterBuffStacks.Length; i+=2)
        {
            var itemMonsterBuffId = itemMonsterBuffStacks[i];
            var stackAmount = int.Parse(itemMonsterBuffStacks[i+1]);
            if (itemMonsterBuffId == monsterBuffId)
                return stackAmount >= min && stackAmount <= max;
        }
        return false;
    }

    /// <summary>
    /// Check if monster's stun time matches provided arguments.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQueryContext"/>.</param>
    /// <returns>The GSQ evaluation result</returns>
    [GSQ]
    private static bool MONSTER_STUN_TIME(string[] query, GameStateQueryContext context)
    {
        var min = int.MinValue;
        var max = int.MaxValue;
        if (ArgUtility.TryGetFloat(query, 1, out var argMin, out _))
            min = argMin.ToMilliseconds();
        if (ArgUtility.TryGetFloat(query, 2, out var argMax, out _))
            max = argMax.ToMilliseconds();
        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterStunTimeItemKey, out var modDataStunTime,
                "Monster")) return false;
        var stunTime = int.Parse(modDataStunTime);
        if (query.Length == 0) return stunTime > 0;
        return stunTime >= min && stunTime <= max;
    }

    private static bool MONSTER_HAS_STATUS(string[] query, GameStateQueryContext context)
    {
        if (!TryGetMultipleTillEnd(query, 1, out var statuses, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!RequireContextModData(query, context.TargetItem?.modData, MonsterStatusesItemKey, out var monsterStatusesStr,
                "Monster")) return false;
        var itemMonsterStatuses = ArgUtility.SplitBySpace(monsterStatusesStr);
        return statuses.Any(itemMonsterStatuses.Contains);
    }
}
