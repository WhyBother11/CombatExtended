using System.Diagnostics.CodeAnalysis;
using System.Text;
using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.Core.Misc.Projectiles;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Monsters.Buffs;
using CombatExtended.Framework.Core.Monsters.Statuses;
using CombatExtended.Framework.Core.Util;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Abilities;
using CombatExtended.Framework.TriggersActionsGSQ.Monsters;
using CombatExtended.Framework.TriggersActionsGSQ.Projectiles;
using StardewValley;
using StardewValley.Mods;
using StardewValley.Monsters;

namespace CombatExtended.Framework.TriggersActionsGSQ.Util;

public static class TriggerActionGSQUtils
{
    /// <summary>
    /// Append <paramref name="monster"/>'s info into <paramref name="item"/>'s modData for use in related game state queries. 
    /// </summary>
    /// <param name="item">The target or input item.</param>
    /// <param name="monster">The monster whose info should be handled.</param>
    public static void PackMonsterInfo(this Item item, Monster monster)
    {
        item.modData[MonsterGameStateQueries.MonsterHealthItemKey] = monster.Health.ToString();
        item.modData[MonsterGameStateQueries.MonsterNameItemKey] = monster.Name;
        item.modData[MonsterGameStateQueries.MonsterTagsItemKey] = string.Join(' ', monster.GetTags().Select(tag => tag.Id));
        if (monster.modData.TryGetValue(MonsterBuff.MonsterModDataBuffsKey, out var monsterBuffs))
            item.modData[MonsterGameStateQueries.MonsterBuffsItemKey] = monsterBuffs;
        else item.modData[MonsterGameStateQueries.MonsterBuffsItemKey] = "";
        var monsterBuffStacksSb = new StringBuilder();
        var first = true;
        foreach (var modDataKey in monster.modData.Keys)
        {
            var parts = modDataKey.Split('_');
            if (parts.Length >= 2 && parts[0] == Id && parts[1] == "Stacks")
            {
                if (!first)
                    monsterBuffStacksSb.Append(' ');
                else first = false;
                var monsterBuffId = string.Join("_", parts.Skip(2));
                monsterBuffStacksSb.Append(monsterBuffId);
                monsterBuffStacksSb.Append(' ');
                monsterBuffStacksSb.Append(monster.modData[modDataKey]);
            }
        }
        item.modData[MonsterGameStateQueries.MonsterBuffStacksItemKey] = monsterBuffStacksSb.ToString();
        item.modData[MonsterGameStateQueries.MonsterStunTimeItemKey] = monster.stunTime.Value.ToString();
        if (monster.modData.TryGetValue(MonsterStatus.MonsterModDataStatusesKey, out var monsterStatuses))
            item.modData[MonsterGameStateQueries.MonsterStatusesItemKey] = monsterStatuses;
        else item.modData[MonsterGameStateQueries.MonsterStatusesItemKey] = "";
    }

    /// <summary>
    /// Append <paramref name="monsterBuff"/>'s info into <paramref name="item"/>'s modData for use in related game state queries. 
    /// </summary>
    /// <param name="item">The target or input item.</param>
    /// <param name="monsterBuff">The monster buff whose info should be handled.</param>
    public static void PackMonsterBuffInfo(this Item item, MonsterBuff monsterBuff)
    {
        item.modData[MonsterBuffGameStateQueries.MonsterBuffIdItemKey] = monsterBuff.Id;
    }

    /// <summary>
    /// Append <paramref name="ability"/>'s info into <paramref name="item"/>'s modData for use in related game state queries. 
    /// </summary>
    /// <param name="item">The target or input item.</param>
    /// <param name="ability">The ability whose info should be handled.</param>
    public static void PackAbilityInfo(this Item item, Ability ability)
    {
        item.modData[AbilityGameStateQueries.AbilityIdItemKey] = ability.Id;
    }

    public static void PackProjectileInfo(this Item item, CustomProjectile projectile)
    {
        item.modData[ProjectileGameStateQueries.ProjectileIdItemKey] = projectile.Id;
        item.modData[ProjectileGameStateQueries.ProjectilePiercesItemKey] = projectile.piercesLeft.ToString();
    }
    
    /// <summary>
    /// Get multiple arguments beginning from <paramref name="initialIndex"/>, taking all arguments until end of args array.
    /// </summary>
    /// <param name="array">The array of arguments to read.</param>
    /// <param name="initialIndex">The index, beginning from which arguments should be taken.</param>
    /// <param name="args">The arguments, if found at least one for <paramref name="initialIndex"/>.</param>
    /// <param name="error">An error phrase indicating why getting the argument failed (like 'required index X not found'), if applicable.</param>
    /// <returns>Returns whether the argument was successfully found and is valid.</returns>
    public static bool TryGetMultipleTillEnd(string[] array, int initialIndex, out List<string> args, out string error)
    {
        var result = new List<string>();
        if (!ArgUtility.TryGet(array, initialIndex, out var arg, out error))
        {
            args = new List<string>();
            return false;
        }
        result.Add(arg);
        for (var i = initialIndex + 1; i < array.Length; i++)
        {
            if (!ArgUtility.TryGet(array, i, out arg, out _)) continue;
            result.Add(arg);
        }
        args = result;
        return true;
    }
    
    /// <summary>
    /// Get multiple integer arguments beginning from <paramref name="initialIndex"/>, taking all arguments until end of args array.
    /// </summary>
    /// <param name="array">The array of arguments to read.</param>
    /// <param name="initialIndex">The index, beginning from which arguments should be taken.</param>
    /// <param name="args">The arguments, if found at least one for <paramref name="initialIndex"/>.</param>
    /// <param name="error">An error phrase indicating why getting the argument failed (like 'required index X not found'), if applicable.</param>
    /// <returns>Returns whether the argument was successfully found and is valid.</returns>
    public static bool TryGetMultipleIntsTillEnd(string[] array, int initialIndex, out List<int> args, out string error)
    {
        var result = new List<int>();
        if (!ArgUtility.TryGetInt(array, initialIndex, out var arg, out error))
        {
            args = new List<int>();
            return false;
        }
        result.Add(arg);
        for (var i = initialIndex + 1; i < array.Length; i++)
        {
            if (!ArgUtility.TryGetInt(array, i, out arg, out error))
            {
                args = new List<int>();
                return false;
            }
            result.Add(arg);
        }
        args = result;
        return true;
    }

    /// <summary>
    /// Require given <paramref name="modDataKey"/> to be presented in <paramref name="modData"/>.
    /// </summary>
    /// <param name="query">The GSQ arguments.</param>
    /// <param name="modData">The <see cref="ModDataDictionary"/> where context key should be checked.</param>
    /// <param name="modDataKey">The key of context value in <paramref name="modData"/>.</param>
    /// <param name="result">The context value, if any.</param>
    /// <param name="contextEntity">The name of context entity that will be logged if result is <c>false</c>.</param>
    /// <returns>Whether <paramref name="modDataKey"/> is present in <paramref name="modData"/>.</returns>
    public static bool RequireContextModData(string[] query, ModDataDictionary? modData, string modDataKey, 
        [MaybeNullWhen(false)] out string result, string contextEntity)
    {
        if (modData?.TryGetValue(modDataKey, out result) == true)
        {
            return true;
        }

        result = null;
        return GameStateQuery.Helpers.ErrorResult(query, $"No value for {modDataKey} present in '{contextEntity} context'");
    }

    /// <summary>
    /// Require given context to include <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The context value to check.</param>
    /// <param name="error">The error if no context value is present.</param>
    /// <param name="valueName">The name of context value that will be used in <paramref name="error"/>.</param>
    /// <param name="contextName">The name of context that will be used in <paramref name="error"/>.</param>
    /// <typeparam name="TV">The type of context value.</typeparam>
    /// <returns>Whether the context contains <paramref name="value"/>.</returns>
    public static bool RequireContext<TV>([NotNullWhen(true)]TV? value, out string error, 
        string? valueName = null, string? contextName = null)
    { 
        valueName ??= typeof(TV).Name;
        contextName ??= $"{typeof(TV).Name} context";
        if (value is null)
        {
            error = $"No {valueName} present in '{contextName}'.";
            return false;
        }
        error = null;
        return true;
    }

    public static bool TryHandleRange(string rangeId, Monster? monster, Farmer player, out string error,
        Action<Monster>? monsterHandler = null, Action<Farmer>? playerHandler = null)
    {
        if (!RangeDataAsset.Exists(rangeId))
        {
            error = $"Invalid range id: {rangeId}";
            return false;
        }
        RangeUtil.GetAffectedByRange(rangeId, player, monster, out var affectedPlayers, out var affectedMonsters);
        if (monsterHandler is not null)
        {
            foreach (var affectedMonster in affectedMonsters
                         .Where(affectedMonster => affectedMonster.Health > 0 && affectedMonster.ShouldBeInteractedWith()))
            {
                monsterHandler.Invoke(affectedMonster);
            }
        }
        if (playerHandler is not null)
        {
            affectedPlayers.ForEach(playerHandler.Invoke);
        }
        
        error = null;
        return true;
    }
}
