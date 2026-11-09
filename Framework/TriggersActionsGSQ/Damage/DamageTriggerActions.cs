using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Util;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
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
            !ArgUtility.TryGetInt(args, 3, out var arg3, out error))
        {
            return false;
        }

        var minDamage = 0;
        int maxDamage;
        if (args.Length >= 5)
        {
            minDamage = arg3;
            if (!ArgUtility.TryGetInt(args, 4, out var arg4, out error))
                return false;
            maxDamage = arg4;
        }
        else maxDamage = arg3;
        var damageAmount = Game1.random.Next(minDamage, maxDamage + 1);
        
        var triggerArgs = ctx.TriggerArgs;
        var player = Game1.player;
        
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
                argMonster.takeDamage(damageAmount, 0, 0, false, 0.0, player);
            else DamageType.TakeExtraTypePureDamage(argMonster, damageAmount, damageTypeId, player);
        }

        return true;
    }

    /// <summary>
    /// Trigger action to lower or increase monster's resistance to provided damage type.
    /// </summary>
    /// <param name="args">Trigger actions args.</param>
    /// <param name="ctx"><see cref="TriggerActionContext"/>.</param>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <returns>Whether trigger action was ran successfully.</returns>
    [TriggerAction]
    private static bool AlterResistance(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var damageType, out error) ||
            !ArgUtility.TryGetInt(args, 2, out var diff, out error))
        {
            return false;
        }

        if (!DamageTypeDataAsset.Exists(damageType))
        {
            error = $"Unknown damage type {damageType}";
            return false;
        }

        diff = Math.Clamp(diff, -5, 5);
        var triggerArgs = ctx.TriggerArgs;
        if (triggerArgs.Length == 1 && triggerArgs[0] is Monster monster)
        {
            var combined = monster.GetResistances();
            var absDiff = Math.Abs(diff);
            if (diff < 0)
                combined.LowerResistance(damageType, absDiff);
            else combined.IncreaseResistance(damageType, absDiff);
            monster.UpdateResistance(combined);
        }

        return true;
    }
    
    /// <summary>
    /// Trigger action to add provided armour class amount to current player until the end of the day.
    /// </summary>
    /// <param name="args">Trigger actions args.</param>
    /// <param name="ctx"><see cref="TriggerActionContext"/>.</param>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <returns>Whether trigger action was ran successfully.</returns>
    [TriggerAction]
    private static bool AddArmourClass(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGetInt(args, 1, out var ac, out error))
        {
            return false;
        }

        ArmourClass.AddFarmerArmourClass(ac);

        return true;
    }

    [TriggerAction]
    private static bool AddPlayerHealth(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGetInt(args, 1, out var hp, out error))
        {
            return false;
        }
        var player = Game1.player;
        if (hp >= 0) 
            player.health = Math.Min(player.maxHealth, player.health + hp);
        else player.takeDamage(Math.Abs(hp), false, null);
        if (ArgUtility.TryGetBool(args, 2, out var show, out _) && show) 
            player.currentLocation.debris.Add(new Debris(hp, player.getStandingPosition(), 
                hp >= 0 ? Color.Lime : Color.Red, 1f, player));
        return true;
    }
}
