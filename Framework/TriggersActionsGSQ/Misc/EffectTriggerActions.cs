using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
using CombatExtended.Framework.Core.Util;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.UI.Animations;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;

namespace CombatExtended.Framework.TriggersActionsGSQ.Misc;

[TriggerActionGSQ_Holder]
public static class EffectTriggerActions
{

    [TriggerAction]
    private static bool LightningStrike(string[] args, TriggerActionContext ctx, out string error)
    {
        if (ArgUtility.TryGet(args, 1, out var rangeId, out error))
        {
            if (!RangeDataAsset.Exists(rangeId))
            {
                error = $"Invalid range id: {rangeId}";
                return false;
            }
            var entity = "all";
            if (ArgUtility.TryGet(args, 2, out var target, out error))
                entity = target;
            var player = Game1.player;
            RangeUtil.GetAffectedByRange(rangeId, player, null, out var affectedFarmers, out var affectedMonsters);
            var characters = new List<Character>();
            switch (entity)
            {
                case "all":
                    characters.AddRange(affectedMonsters);
                    characters.AddRange(affectedMonsters);
                    break;
                case "player":
                    characters.AddRange(affectedFarmers); 
                    break;
                case "monster":
                    characters.AddRange(affectedMonsters); 
                    break;
            }
            characters.ForEach(c => SUtility.drawLightningBolt(new Vector2(
                c.Position.X, c.Position.Y - c.Sprite.SpriteHeight / 2f), c.currentLocation));
            return true;
        }
        if (ctx.TriggerArgs.Length == 1 && ctx.TriggerArgs[0] is Monster monster)
        {
            SUtility.drawLightningBolt(new Vector2(monster.Position.X, monster.Position.Y - monster.Sprite.SpriteHeight / 2f), 
                monster.currentLocation);
        }

        error = "No range was used in a context that does not have a single monster for lightning strike.";
        return false;
    }

    [TriggerAction]
    private static bool AnimatedSprite(string[] args, TriggerActionContext ctx, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var spriteId, out error))
            return false;
        var positions = new List<Vector2>();
        if (ArgUtility.TryGet(args, 2, out var rangeId, out error))
        {
            if (!RangeDataAsset.Data.TryGetValue(rangeId, out var rangeData))
            {
                error = $"Invalid range id: {rangeId}";
                return false;
            }
            if (!ArgUtility.TryGet(args, 3, out var spawnArea, out error))
                return false;
            var playerTile = Game1.player.Tile;
            var cursorTile = Helper.Input.GetCursorPosition().Tile;
            var tiles = 
                RangeUtil.GetTilesFromRelative(TileParser.ParseTiles(rangeData.Tiles, rangeId), cursorTile, playerTile);
            if (spawnArea is "tiles")
                positions.AddRange(tiles.Select(t => t * 64f));
            else
            {
                RangeUtil.GetAffectedByRange(Game1.player, tiles, out var affectedFarmers, out var affectedMonsters);
                switch (spawnArea)
                {
                    case "monsters": positions.AddRange(affectedMonsters.Select(m => m.Position)); break;
                    case "players": positions.AddRange(affectedFarmers.Select(f => f.Position)); break;
                    case "allEntities":
                        positions.AddRange(affectedMonsters.Select(m => m.Position));
                        positions.AddRange(affectedFarmers.Select(f => f.Position));
                        break;
                    case "monsterTiles": positions.AddRange(affectedMonsters.Select(m => m.Tile * 64f)); break;
                    case "playerTiles": positions.AddRange(affectedFarmers.Select(f => f.Tile * 64f)); break;
                    case "allEntityTiles":
                        positions.AddRange(affectedMonsters.Select(m => m.Tile * 64f));
                        positions.AddRange(affectedFarmers.Select(f => f.Tile * 64f));
                        break;
                }
            }
        }
        else if (ctx.TriggerArgs.Length == 1 && ctx.TriggerArgs[0] is Monster monster)
        {
            positions.Add(monster.Position);
        }
        
        if (!positions.Any()) error = "No range was used in a context that does not have a single monster for spawning TAS.";
        return positions.Any();
    }

    [TriggerAction]
    [MonsterContextAware]
    private static bool LinkMonster(string[] args, TriggerActionContext ctx, Monster? monster, out string error)
    {
        if (!RequireContext(monster, out error)) return false;
        if (!ArgUtility.TryGet(args, 1, out var linkId, out error))
            return false;
        if (linkId is "_none") 
            LinkEffects.Remove(monster);
        else
        {
            if (!LinkEffectDataAsset.Data.TryGetValue(linkId, out var linkData))
            {
                error = $"Unknown link effect ID: {linkId}.";
                return false;
            }
            LinkEffects.BeginMonsterLink(monster, linkData);
        }
        return true;
    }
}
