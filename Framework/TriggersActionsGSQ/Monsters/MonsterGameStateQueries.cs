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
    public static readonly string MonsterStunTimeItemKey = "Monster_StunTime".PrependId();

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
        var maxHp = int.MaxValue;
        if (ArgUtility.TryGetInt(query, 1, out var argMaxHp, out _))
            maxHp = argMaxHp;
        if (context.TargetItem?.modData.TryGetValue(MonsterHealthItemKey, out var healthStr) ?? false)
        {
            var monsterHealth = int.Parse(healthStr);
            return monsterHealth >= minHp && monsterHealth <= maxHp;
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster health.");
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

    /// <summary>
    /// Check if monster's name matches one of provided arguments.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQueryContext"/>.</param>
    /// <returns>The GSQ evaluation result</returns>
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
    
    /// <summary>
    /// Check if monster has one of provided monster buffs.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQueryContext"/>.</param>
    /// <returns>The GSQ evaluation result</returns>
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
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain amount of monster buffs.");
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
        if (context.TargetItem?.modData.TryGetValue(MonsterStunTimeItemKey, out var modDataStunTime) ?? false)
        {
            var stunTime = int.Parse(modDataStunTime);
            if (query.Length == 0) return stunTime > 0;
            return stunTime >= min && stunTime <= max;
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster stun time.");
    }
}
