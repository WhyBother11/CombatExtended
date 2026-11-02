using CombatExtended.Framework.Core.Misc.Projectiles;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.TriggersActionsGSQ.Projectiles;

[TriggerActionGSQ_Holder]
public static class ProjectileTriggerActions
{

    [TriggerAction]
    private static bool LaunchProjectile(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var projectileId, out error)) 
            return false;
        if (!ProjectileDataAsset.Data.TryGetValue(projectileId, out var projectileData))
        {
            error = $"Unknown projectile id: {projectileId}.";
            return false;
        }
        if (!ArgUtility.TryGet(args, 2, out var direction, out error)) 
            return false;
        switch (direction)
        {
            case "_cursor":
                var targetPoint = Helper.Input.GetCursorPosition().AbsolutePixels;
                var startingPoint = Game1.player.Position;
                var velocity = SUtility.getVelocityTowardPoint(startingPoint, targetPoint, projectileData.Velocity);
                CustomProjectile.Launch(Game1.player.currentLocation, startingPoint, velocity, projectileData);
                break;
            case "_closestMonster":
                var radius = 5f;
                var fromMonster = false;
                GameLocation location;
                if (ArgUtility.TryGetFloat(args, 3, out var radiusArg, out _))
                    radius = radiusArg;
                if (ArgUtility.TryGetBool(args, 4, out var fromMonsterArg, out _))
                    fromMonster = fromMonsterArg;
                Monster? monster = null;
                Vector2 startingTile;
                if (ctx.TriggerArgs.Length > 0 && ctx.TriggerArgs[0] is Monster argMonster && fromMonster)
                {
                    startingPoint = argMonster.Position;
                    startingTile = argMonster.Tile;
                    monster = argMonster;
                    location = argMonster.currentLocation;
                }
                else
                {
                    startingPoint = Game1.player.Position;
                    startingTile = Game1.player.Tile;
                    location = Game1.player.currentLocation;
                }
                if (Utility.TryFindClosestMonster(location, startingTile, radius, out var closest, monster))
                {
                    targetPoint = closest.Position;
                    velocity = SUtility.getVelocityTowardPoint(startingPoint, targetPoint, projectileData.Velocity);
                    CustomProjectile.Launch(location, startingPoint, velocity, projectileData);
                }
                break;
            default:
                if (!float.TryParse(direction, out var angle))
                {
                    error = $"Cannot parse {direction} as a float angle.";
                    return false;
                }
                var angleRadians = MathHelper.ToRadians(angle);
                //var angleRadians = MathHelper.ToRadians(90 - angle);
                startingPoint = Game1.player.Position;
                //targetPoint = startingPoint + new Vector2(MathF.Cos(angleRadians), MathF.Sin(angleRadians));
                targetPoint = startingPoint + new Vector2(MathF.Sin(angleRadians), MathF.Cos(angleRadians));
                velocity = SUtility.getVelocityTowardPoint(startingPoint, targetPoint, projectileData.Velocity);
                CustomProjectile.Launch(Game1.player.currentLocation, startingPoint, velocity, projectileData);
                break;
        }
        return true;
    }
}
