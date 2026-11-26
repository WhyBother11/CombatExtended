using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;

namespace CombatExtended.Framework.TriggersActionsGSQ.Monsters;

[TriggerActionGSQ_Holder]
public static class MonsterBuffTriggerActions
{
    
    /// <summary>
    /// Trigger action to apply provided monster buff to monster if present in <paramref name="ctx"/>'s trigger args, or
    /// to monsters in ability's range if it is provided.
    /// </summary>
    /// <param name="args">The trigger action args.</param>
    /// <param name="ctx"><see cref="TriggerActionContext"/>.</param>
    /// <param name="monster">The context monster, if any.</param>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <returns>Whether trigger action was ran successfully.</returns>
    [TriggerAction]
    [MonsterContextAware]
    private static bool ApplyMonsterBuff(string[] args, TriggerActionContext ctx, Monster? monster, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var monsterBuffId, out error)) 
            return false;
        if (!MonsterBuffDataAsset.Exists(monsterBuffId))
        {
            error = $"Unknown monster buff ID: {monsterBuffId}.";
            return false;
        }
        ArgUtility.TryGetOptionalFloat(args, 2, out var chance, out _, 1f);
        
        var player = Game1.player;
        var location = player.currentLocation;

        if (ArgUtility.TryGet(args, 3, out var rangeId, out _))
        {
            return TryHandleRange(rangeId, monster, player, out error,
                monsterHandler: m =>
                {
                    if (Game1.random.NextDouble() < chance) 
                        MonsterBuffManager.ApplyBuff(m, location, monsterBuffId, 
                            player.GetCharacterType(), player.GetCharacterId());
                });
        }

        if (!RequireContext(monster, out error)) return false;
        if (monster.Health > 0 && monster.ShouldBeInteractedWith() && Game1.random.NextDouble() < chance)
        {
            MonsterBuffManager.ApplyBuff(monster, location, monsterBuffId,
                player.GetCharacterType(), player.GetCharacterId());
        }
        return true;
    }

    [TriggerAction]
    [MonsterContextAware]
    private static bool RemoveMonsterBuff(string[] args, TriggerActionContext ctx, Monster? monster, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var monsterBuffId, out error)) 
            return false;
        if (!MonsterBuffDataAsset.Exists(monsterBuffId))
        {
            error = $"Unknown monster buff ID: {monsterBuffId}.";
            return false;
        }
        ArgUtility.TryGetOptionalFloat(args, 2, out var chance, out _, 1f);
        var player = Game1.player;
        var location = player.currentLocation;

        if (ArgUtility.TryGet(args, 3, out var rangeId, out _))
        {
            return TryHandleRange(rangeId, monster, player, out error,
                monsterHandler: m =>
                {
                    if (Game1.random.NextDouble() < chance)
                        MonsterBuffManager.RemoveBuff(m, location, monsterBuffId);
                });
        }

        if (!RequireContext(monster, out error)) return false;
        if (Game1.random.NextDouble() < chance) 
            MonsterBuffManager.RemoveBuff(monster, location, monsterBuffId);
        return true;
    }
}
