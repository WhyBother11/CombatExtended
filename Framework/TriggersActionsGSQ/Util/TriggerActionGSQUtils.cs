using System.Text;
using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.Core.Misc.Projectiles;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Monsters.Buffs;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Abilities;
using CombatExtended.Framework.TriggersActionsGSQ.Monsters;
using CombatExtended.Framework.TriggersActionsGSQ.Projectiles;
using StardewValley;
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
    }

    /// <summary>
    /// Append <paramref name="monsterBuff"/>'s info into <paramref name="item"/>'s modData for use in related game state queries. 
    /// </summary>
    /// <param name="item">The target or input item.</param>
    /// <param name="monsterBuff">The monster buff whose info should be handled.</param>
    public static void PackMonsterBuffInfo(this Item item, MonsterBuff monsterBuff)
    {
        item.modData[MonsterBuffGameStateQueries.MonsterBuffIdItemKey] = monsterBuff.Id;
        /*item.modData[MonsterBuffGameStateQueries.MonsterBuffContextTagsItemKey] = ArgUtility.UnsplitQuoteAware(
            monsterBuff.
        );*/
        // TODO: do monster buff context tags
        item.modData[MonsterBuffGameStateQueries.MonsterBuffContextTagsItemKey] = "";
    }

    /// <summary>
    /// Append <paramref name="ability"/>'s info into <paramref name="item"/>'s modData for use in related game state queries. 
    /// </summary>
    /// <param name="item">The target or input item.</param>
    /// <param name="ability">The ability whose info should be handled.</param>
    public static void PackAbilityInfo(this Item item, Ability ability)
    {
        item.modData[AbilityGameStateQueries.AbilityIdItemKey] = ability.Id;
        if (AbilityDataAsset.ContextTags.TryGetValue(ability.Id, out var contextTags))
        {
            item.modData[AbilityGameStateQueries.AbilityContextTagsItemKey] = string.Join(' ', contextTags);
        }
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
}
