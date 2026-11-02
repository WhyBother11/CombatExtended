using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Util;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;

namespace CombatExtended.Framework.TriggersActionsGSQ.Monsters;

[TriggerActionGSQ_Holder]
public static class MonsterBuffTriggerActions
{
    
    /// <summary>
    /// Trigger action to apply provided monster buff to monster if present in <paramref name="ctx"/>'s trigger args, or to monsters in ability's range if it is provided.
    /// </summary>
    /// <param name="args">Trigger actions args.</param>
    /// <param name="ctx"><see cref="TriggerActionContext"/>.</param>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <returns>Whether trigger action was ran successfully.</returns>
    [TriggerAction]
    private static bool ApplyMonsterBuff(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var monsterBuffId, out error)) 
            return false;
        if (!MonsterBuffDataAsset.Exists(monsterBuffId))
        {
            error = $"Unknown monster buff id: {monsterBuffId}.";
            return false;
        }

        if (!ArgUtility.TryGetFloat(args, 2, out var chance, out error)) 
            return false;
        
        var triggerArgs = ctx.TriggerArgs;
        var player = Game1.player;
        var location = player.currentLocation;

        if (ArgUtility.TryGet(args, 3, out var rangeId, out _))
        {
            if (!RangeDataAsset.Exists(rangeId))
            {
                error = $"Invalid range id: {rangeId}";
                return false;
            }
            RangeUtil.GetAffectedByRange(rangeId, player, triggerArgs.Length == 1 && triggerArgs[0] is Monster monster ? monster : null, 
                out _, out var affectedMonsters);
            affectedMonsters
                .Where(m => m.Health > 0 && m.ShouldBeInteractedWith())
                .ToList()
                .ForEach(m =>
                {
                    if (Game1.random.NextDouble() < chance) 
                        MonsterBuffManager.ApplyBuff(m, location, monsterBuffId, 
                            player.GetCharacterType(), player.GetCharacterId());
                });
            return true;
        }

        if (triggerArgs.Length == 1 && triggerArgs[0] is Monster argMonster)
        {
            if (argMonster.Health > 0 && argMonster.ShouldBeInteractedWith() && Game1.random.NextDouble() < chance) 
                MonsterBuffManager.ApplyBuff(argMonster, location, monsterBuffId,
                    player.GetCharacterType(), player.GetCharacterId());
            return true;
        }

        error = "No range was used in a context that does not have a single monster to apply buff to.";
        return false;
    }

    [TriggerAction]
    private static bool RemoveMonsterBuff(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var monsterBuffId, out error)) 
            return false;
        if (!MonsterBuffDataAsset.Exists(monsterBuffId))
        {
            error = $"Unknown monster buff id: {monsterBuffId}.";
            return false;
        }
        var triggerArgs = ctx.TriggerArgs;
        var player = Game1.player;
        var location = player.currentLocation;

        if (ArgUtility.TryGet(args, 2, out var rangeId, out _))
        {
            if (!RangeDataAsset.Exists(rangeId))
            {
                error = $"Invalid range id: {rangeId}";
                return false;
            }
            RangeUtil.GetAffectedByRange(rangeId, player, triggerArgs.Length == 1 && triggerArgs[0] is Monster monster ? monster : null, 
                out _, out var affectedMonsters);
            affectedMonsters
                .ToList()
                .ForEach(m => MonsterBuffManager.RemoveBuff(m, location, monsterBuffId));
            return true;
        }

        if (triggerArgs.Length == 1 && triggerArgs[0] is Monster argMonster)
        {
            MonsterBuffManager.RemoveBuff(argMonster, location, monsterBuffId);
        }

        error = "No range was used in a context that does not have a single monster to apply buff to.";
        return false;
    }
}
