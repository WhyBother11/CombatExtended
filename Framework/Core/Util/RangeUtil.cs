using CombatExtended.Framework.Data;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Util;

public static class RangeUtil
{
    public static void GetAffectedByRange(string rangeId, Farmer player, Monster? monster,
        out List<Farmer> affectedFarmers, out List<Monster> affectedMonsters)
    {
        var playerTile = player.Tile;
        var cursorTile = Helper.Input.GetCursorPosition().Tile;
        var specialTile = monster?.Tile;
        var parsedTiles = TileParser.ParseTiles(RangeDataAsset.Data[rangeId].Tiles, rangeId);
        var tiles = GetTilesFromRelative(parsedTiles, cursorTile, playerTile, specialTile);
        GetAffectedByRange(player, tiles, out affectedFarmers, out affectedMonsters);
    }
    
    
    public static HashSet<Vector2> GetTilesFromRelative(HashSet<ParsedVector> relativeTiles, Vector2 cursorTile,
        Vector2 playerTile, Vector2? specialTile = null)
    {
        var tiles = new HashSet<Vector2>();
        specialTile ??= playerTile;
        foreach (var tile in relativeTiles)
        {
            switch (tile.RelativeTo)
            {
                case "cursor":
                    tiles.Add(
                        new Vector2(tile.Tile.X + cursorTile.X, tile.Tile.Y + cursorTile.Y)
                    ); break;
                case "player":
                    tiles.Add(
                        new Vector2(tile.Tile.X + playerTile.X, tile.Tile.Y + playerTile.Y)
                    ); break;
                case "special":
                    tiles.Add(
                        new Vector2(tile.Tile.X + specialTile.Value.X, tile.Tile.Y + specialTile.Value.Y)
                    ); break;
            }
        }
        return tiles;
    }


    /// <summary>
    /// Utility method to get affected tiles and affected entities.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="tiles">List of affected by this skill tiles.</param>
    /// <param name="affectedFarmers">List of affected (including the one who triggered skill) farmers.</param>
    /// <param name="affectedMonsters">List of affected by this skill monsters.</param>
    public static void GetAffectedByRange(Farmer player, HashSet<Vector2> tiles,
        out List<Farmer> affectedFarmers, out List<Monster> affectedMonsters)
    {
        affectedMonsters = new List<Monster>();
        affectedFarmers = new List<Farmer>();
        var monsters = player.currentLocation.characters.OfType<Monster>().ToList();
        var farmers = player.currentLocation.farmers;
        foreach (var vector2 in tiles)
        {
            var tileX = (int)vector2.X;
            var tileY = (int)vector2.Y;
            foreach (var monster in monsters)
            {
                if ((int)monster.Tile.X == tileX && (int)monster.Tile.Y == tileY)
                {
                    affectedMonsters.Add(monster);
                }
            }
            foreach (var farmer in farmers)
            {
                if ((int)farmer.Tile.X == tileX && (int)farmer.Tile.Y == tileY)
                {
                    affectedFarmers.Add(farmer);
                }
            }
        }
    }
}
