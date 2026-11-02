using CombatExtended.Framework.Data;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;

namespace CombatExtended.Framework.Core.Util;

public class ParsedVector
{
    public Vector2 Tile { get; set; }
    
    public string RelativeTo { get; set; }
    
    public ParsedVector(string relativeTo, Vector2 tile)
    {
        RelativeTo = relativeTo;
        Tile = tile;
    }
}

public static class TileParser
{
    
    private static readonly Dictionary<string, HashSet<ParsedVector>> CachedParsedTiles = new ();

    public static HashSet<ParsedVector> ParseTiles(List<RangeTileData> tiles, string rangeId = "")
    {
        if (rangeId.Any() && CachedParsedTiles.TryGetValue(rangeId, out var parsedTiles))
        {
            return parsedTiles;
        }
        var result = new HashSet<ParsedVector>();
        foreach (var tileData in tiles)
        {
            ParseAbilityTileData(tileData, out var added, out var removed);
            result.AddRange(added);
            if (removed.Any())
                result.RemoveWhere(tile => removed.Any(r => 
                    r.Tile.X.Equals(tile.Tile.X) && r.Tile.Y.Equals(tile.Tile.Y)));
        }
        if (rangeId.Any() && !RangeDataAsset.NonStaticRange(rangeId))
            CachedParsedTiles[rangeId] = result;
        return result;
    }

    private static void ParseAbilityTileData(RangeTileData tileData, out HashSet<ParsedVector> added,
        out HashSet<ParsedVector> removed)
    {
        removed = new HashSet<ParsedVector>();
        added = new HashSet<ParsedVector>();
        var tiles = new HashSet<ParsedVector>();
        tileData.Rectangles?.ForEach(rect =>
        {
            foreach (var vector2 in GetRectangle(tileData.RelativeTo, rect))
            {
                tiles.Add(vector2);
            }
        });

        if (tileData.Radius is not null)
        {
            foreach (var vector2 in GetCircle(tileData.RelativeTo, tileData.Radius.Value))
            {
                tiles.Add(vector2);
            }
        }

        tileData.Lines?.ForEach(line =>
        {
            foreach (var vector2 in GetLine(tileData.RelativeTo, line))
            {
                tiles.Add(vector2);
            }
        });

        tileData.CursorLines?.ForEach(cursorLine =>
        {
            foreach (var vector2 in GetCursorLine(cursorLine))
            {
                tiles.Add(vector2);
            }
        });
        
        if (tileData.Type == "Add")
            added.AddRange(tiles);

        if (tileData.Type == "Remove")
            removed.AddRange(tiles);
    }

    #region RangeTypeParsers
    private static IEnumerable<ParsedVector> GetRectangle(string relativeTo, RangeTileData.Rectangle rectangle)
    {
        for (var x = rectangle.From.X; x <= rectangle.To.X; x++)
        {
            for (var y = rectangle.From.Y; y <= rectangle.To.Y; y++)
            {
                yield return new ParsedVector(relativeTo, new Vector2(x, y));
            }
        }
    }

    private static IEnumerable<ParsedVector> GetCircle(string relativeTo, float radius)
    {
        var radiusSquared = radius * radius;
        for (var x = 0; x <= radius; x++)
        {
            for (var y = 0; y <= radius; y++)
            {
                if (x * x + y * y > radiusSquared) continue;
                yield return new ParsedVector(relativeTo, new Vector2(x, y));
                yield return new ParsedVector(relativeTo, new Vector2(-x, y));
                yield return new ParsedVector(relativeTo, new Vector2(x, -y));
                yield return new ParsedVector(relativeTo, new Vector2(-x, -y));
            }
        }
    }

    private static IEnumerable<ParsedVector> GetLine(string relativeTo, RangeTileData.Line line)
    {
        var radians = MathF.PI * (line.Angle / 180.0f);
        var deltaX = MathF.Cos(radians);
        var deltaY = MathF.Sin(radians);
        return GetLine(relativeTo, deltaX, deltaY, line.MaxLength, line.Delta ?? new RangeTileData.Coordinate(), line.Width);
    }

    private static IEnumerable<ParsedVector> GetCursorLine(RangeTileData.CursorLine cursorLine)
    {
        var player = Game1.player;
        var playerTile = player.Tile;
        var cursorTile = Helper.Input.GetCursorPosition().Tile;
        var deltaX = cursorTile.X - playerTile.X;
        var deltaY = cursorTile.Y - playerTile.Y;
        return GetLine("player", deltaX, deltaY, cursorLine.MaxLength, 
            cursorLine.Delta ?? new RangeTileData.Coordinate(), cursorLine.Width);
    }

    private static IEnumerable<ParsedVector> GetLine(string relativeTo, float deltaX, float deltaY, 
        int maxLen, RangeTileData.Coordinate coordinateDelta, int width)
    {
        // may god forgive me for what i'm doing here
        // when calculating slope by dividing one delta for another, when axis delta is close to zero it approaches infinity
        // and causes gaps so for this case coordinates are swapped
        var shouldUseXySystem = Math.Abs(deltaX) > Math.Abs(deltaY);
        if (shouldUseXySystem)
        {
            // without it, it works only in one half of coordinate plane, where planes are separated
            // by y=x or y=-x depending on if, for example, x is used from 0 to maxLen or from -maxLen to 0
            // this way it swaps when needed, based on what part of coordinate plane cursor is in
            var start = deltaX >= 0 ? 0 : -maxLen;
            var end = deltaX >= 0 ? maxLen : 0;
            for (var x = start; x <= end; x++)
            {
                var y = deltaX == 0 ? 0 : (int) Math.Round(deltaY * x / deltaX);
                yield return new ParsedVector(relativeTo, new Vector2(x + coordinateDelta.X, y + coordinateDelta.Y));
                var extraLines = width - 1;
                // basically <- | ->-> | <-<-<- | ->->->-> to make line have more width
                var top = extraLines % 2 == 0 ? extraLines / 2 : extraLines / 2 + 1;
                var down = extraLines - top;
                for (var i = 1; i <= top; i++)
                {
                    yield return new ParsedVector(relativeTo, new Vector2(x + coordinateDelta.X, y + coordinateDelta.Y - i));
                }
                for (var i = 1; i <= down; i++)
                {
                    yield return new ParsedVector(relativeTo, new Vector2(x + coordinateDelta.X, y + coordinateDelta.Y + i));
                }
            }
        }
        else
        {
            var start = deltaY >= 0 ? 0 : -maxLen;
            var end = deltaY >= 0 ? maxLen : 0;
            for (var y = start; y <= end; y++)
            {
                var x = deltaY == 0 ? 0 : (int) Math.Round(deltaX * y / deltaY);
                yield return new ParsedVector(relativeTo, new Vector2(x + coordinateDelta.X, y + coordinateDelta.Y));
                var extraLines = width - 1;
                var left = extraLines % 2 == 0 ? extraLines / 2 : extraLines / 2 + 1;
                var right = extraLines - left;
                for (var i = 1; i <= left; i++)
                {
                    yield return new ParsedVector(relativeTo, new Vector2(x + coordinateDelta.X - i, y + coordinateDelta.Y));
                }
                for (var i = 1; i <= right; i++)
                {
                    yield return new ParsedVector(relativeTo, new Vector2(x + coordinateDelta.X + i, y + coordinateDelta.Y));
                }
            }
        }
    }
    #endregion
}
