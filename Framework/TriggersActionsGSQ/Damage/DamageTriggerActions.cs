using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Util;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;

namespace CombatExtended.Framework.TriggersActionsGSQ.Damage;

[TriggerActionGSQ_Holder]
public static class DamageTriggerActions
{

    /// <summary>
    /// Trigger action to damage monster with provided damage type and minimum and maximum damage.
    /// </summary>
    /// <param name="args">Trigger actions args.</param>
    /// <param name="ctx"><see cref="TriggerActionContext"/>.</param>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <returns>Whether trigger action was ran successfully.</returns>
    [TriggerAction]
    private static bool DamageMonster(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var damageTypeId, out error) ||
            !ArgUtility.TryGetInt(args, 2, out var min, out error))
        {
            return false;
        }

        ArgUtility.TryGetOptionalInt(args, 3, out var max, out _, min);
        var damageAmount = Game1.random.Next(min, max + 1);
        
        var triggerArgs = ctx.TriggerArgs;
        var player = Game1.player;
        
        if (ArgUtility.TryGet(args, 4, out var rangeId, out _))
        {
            if (!RangeDataAsset.Exists(rangeId))
            {
                error = $"Invalid range id: {rangeId}";
                return false;
            }
            RangeUtil.GetAffectedByRange(rangeId, player, triggerArgs.Length == 1 && triggerArgs[0] is Monster monster ? monster : null, 
                out _, out var affectedMonsters);
            affectedMonsters
                .Where(affectedMonster => affectedMonster.Health > 0 && affectedMonster.ShouldBeInteractedWith())
                .ToList()
                .ForEach(affectedMonster => affectedMonster.currentLocation.DamageMonsterDirectly(
                    affectedMonster, damageAmount, player, DamageTypeDataAsset.DamageTypeColors[damageTypeId]));
            return true;
        }

        if (triggerArgs.Length == 1 && triggerArgs[0] is Monster argMonster)
        {
            if (argMonster.Health <= 0 || !argMonster.ShouldBeInteractedWith())
                return true;
            if (damageTypeId == DamageType.Physical || !DamageTypeDataAsset.Exists(damageTypeId))
            {
                argMonster.takeDamage(damageAmount, 0, 0, false, 0.0, player);
                argMonster.currentLocation.debris.Add(new Debris(damageAmount, new Vector2(argMonster.GetBoundingBox().Center.X + 16, 
                    argMonster.GetBoundingBox().Center.Y), new Color(255, 130, 0), 1f, argMonster));
            }
            else DamageType.TakeExtraTypePureDamage(argMonster, damageAmount, damageTypeId, player);
        }

        return true;
    }

    [TriggerAction]
    private static bool AddPlayerHealth(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGetInt(args, 1, out var minHp, out error)) 
            return false;
        ArgUtility.TryGetOptionalInt(args, 2, out var maxHp, out _, minHp);
        var hp = Game1.random.Next(minHp, maxHp + 1);
        var player = Game1.player;
        if (hp >= 0) 
            player.health = Math.Min(player.maxHealth, player.health + hp);
        else player.takeDamage(Math.Abs(hp), false, null);
        if (ArgUtility.TryGetBool(args, 3, out var show, out _) && show) 
            player.currentLocation.debris.Add(new Debris(hp, player.getStandingPosition(), 
                hp >= 0 ? Color.Lime : Color.Red, 1f, player));
        return true;
    }
}
