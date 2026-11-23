using CombatExtended.Framework.Core.Misc.MonsterDisplacement;
using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.TriggersActionsGSQ.Sequence;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;
using StardewValley.Triggers;

namespace CombatExtended.Framework.TriggersActionsGSQ.Misc;

[TriggerActionGSQ_Holder]
[HarmonyPatch]
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
    private static bool DelayedAction(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGetFloat(args, 1, out var delay, out error) ||
            !ArgUtility.TryGet(args, 2, out var action, out error))
            return false;
        StardewValley.DelayedAction.functionAfterDelay(() =>
            {
                if (!TriggerActionManager.TryRunAction(TriggerActionManager.ParseAction(action), ctx,
                        out var delayedError, out var exception))
                {
                    Log.Error($"Failed to run action: {action}, error: {delayedError}", exception);
                }
            }, delay.ToMilliseconds()
        );
        return true;
    }

    [TriggerAction]
    [SequenceAware]
    [MonsterContextAware]
    private static bool DragMonster(string[] args, TriggerActionContext ctx, SequenceOrchestrator? orchestrator,
        Monster? monster, out string error)
    {
        if (!RequireContext(monster, out error)) return false;
        if (monster is LavaLurk or Spiker or { IsInvisible: true }) 
            return true;
        if (!ArgUtility.TryGet(args, 1, out var direction, out error) ||
            !ArgUtility.TryGetFloat(args, 2, out var power, out error))
            return false;
        var playerPos = Game1.player.Position;
        
        ArgUtility.TryGetOptional(args, 3, out var argDistance, out _, "5");
        float distance;
        if (direction is "_toPlayer" && argDistance is "_total")
        {
            distance = Vector2.Distance(playerPos, monster.Position);
            distance *= GetPlayerDistanceFactor(distance / 64);
        }
        else distance = float.Parse(argDistance) * Game1.tileSize;
        
        ArgUtility.TryGetOptionalBool(args, 4, out var ignoreWeight, out _);
        if (!ignoreWeight)
        {
            var weight = monster.GetWeight();
            power *= 1 / (1 + MathF.Log(0.1f * weight + 1));
        }

        var velocity = direction switch
        {
            "_toPlayer" => SUtility.getVelocityTowardPlayer(monster.Position.ToPoint(), power, Game1.player),
            "_fromPlayer" => -1 * SUtility.getVelocityTowardPlayer(monster.Position.ToPoint(), power, Game1.player),
            _ => GetAngleVelocity(monster.Position, MathHelper.ToRadians(float.Parse(direction)), power)
        };
        MonsterDisplacementEnabler.StartDisplacement(monster, velocity, distance, () => orchestrator?.NotifyEnded());
        return true;
        Vector2 GetAngleVelocity(Vector2 monsterPos, float angle, float power)
        {
            var targetPos = new Vector2(monsterPos.X + MathF.Cos(angle), monsterPos.Y + MathF.Sin(angle));
            return SUtility.getVelocityTowardPoint(monsterPos, targetPos, power);
        }
        float GetPlayerDistanceFactor(float distance)
        {
            return 2 * MathF.Atan(distance / 2) / MathF.PI;
        }
    }

    private static readonly PerScreen<bool> CanMove = new(() => true);

    [TriggerAction]
    private static bool SetMoving(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGetBool(args, 1, out var canMove, out error))
            return false;
        CanMove.Value = canMove;
        return true;
    }

    [HarmonyPatch(typeof(Farmer), nameof(Farmer.MovePosition))]
    [HarmonyPrefix]
    private static bool Farmer_MovePosition_Prefix()
    {
        return CanMove.Value;
    }
}
