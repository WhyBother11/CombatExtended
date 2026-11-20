using CombatExtended.Framework.Core.Misc.MonsterPull;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;
using StardewValley.Triggers;

namespace CombatExtended.Framework.TriggersActionsGSQ.Misc;

[TriggerActionGSQ_Holder]
public static class MiscTriggerActions
{
    [TriggerAction]
    private static bool CooldownAction(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var id, out error) ||
            !ArgUtility.TryGetFloat(args, 2, out var time, out error) ||
            !ArgUtility.TryGet(args, 3, out var action, out error))
            return false;
        if (!CooldownManager.Timers.TryGetValue(id, out _))
        {
            CooldownManager.Add(id, time);
            if (!TriggerActionManager.TryRunAction(action, out error, out _))
                return false;
        }
        return true;
    }

    [TriggerAction]
    private static bool DisplaceMonster(string[] args, TriggerActionContext ctx, out string error)
    {
        var triggerArgs = ctx.TriggerArgs;
        if (!(triggerArgs.Length == 1 && triggerArgs[0] is Monster argMonster))
        {
            error = "Used in a context that does not have a monster to pull";
            return false;
        }
        if (argMonster is LavaLurk or Spiker or { IsInvisible: true })
        {
            error = null;
            return true;
        }
        if (!ArgUtility.TryGet(args, 1, out var direction, out error) ||
            !ArgUtility.TryGetFloat(args, 2, out var power, out error))
            return false;
        var playerPos = Game1.player.Position;
        
        ArgUtility.TryGetOptional(args, 3, out var argDistance, out _, "5");
        float distance;
        if (direction is "_toPlayer" && argDistance is "_total")
            distance = Vector2.Distance(playerPos, argMonster.Position) * 0.8f;
        else distance = float.Parse(argDistance);
        ArgUtility.TryGetOptionalBool(args, 4, out var ignoreWeight, out _);
        if (!ignoreWeight)
        {
            var weight = argMonster.GetWeight();
            power *= 1 / (1 + MathF.Log(0.1f * weight + 1));
        }

        var velocity = direction switch
        {
            "_toPlayer" => SUtility.getVelocityTowardPlayer(argMonster.Position.ToPoint(), power, Game1.player),
            "_fromPlayer" => -1 * SUtility.getVelocityTowardPlayer(argMonster.Position.ToPoint(), power, Game1.player),
            _ => GetAngleVelocity(argMonster.Position, MathHelper.ToRadians(float.Parse(direction)), power)
        };
        MonsterDisplacementEnabler.StartDisplacement(argMonster, velocity, distance);
        return true;
        Vector2 GetAngleVelocity(Vector2 monsterPos, float angle, float power)
        {
            var targetPos = new Vector2(monsterPos.X + MathF.Cos(angle), monsterPos.Y + MathF.Sin(angle));
            return SUtility.getVelocityTowardPoint(monsterPos, targetPos, power);
        };
    }
}
