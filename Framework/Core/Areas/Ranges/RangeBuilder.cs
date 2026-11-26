using CombatExtended.Framework.Core.Areas.Ranges.Impl;
using CombatExtended.Framework.Data;
using StardewValley.Extensions;

namespace CombatExtended.Framework.Core.Areas.Ranges;

public class ParsedVector
{
    public Vector2 Tile { get; set; }
    
    public RelativeToType RelativeTo { get; set; }
    
    public ParsedVector(RelativeToType relativeTo, Vector2 tile)
    {
        RelativeTo = relativeTo;
        Tile = tile;
    }
}

public abstract class RangeBuilderImpl<TR>
{
    public IEnumerable<ParsedVector> Build(RelativeToType relativeTo, List<TR>? rangeData)
    {
        if (rangeData is null) yield break;
        foreach (var data in rangeData)
        {
            foreach (var parsedVector in Parse(relativeTo, data))
            {
                yield return parsedVector;
            }
        }
    }
    
    protected abstract IEnumerable<ParsedVector> Parse(RelativeToType relativeTo, TR rangeData);
}

public static class RangeBuilder
{
    private static readonly CircleRangeBuilder CircleRangeBuilder = new ();
    
    private static readonly RectangleRangeBuilder RectangleRangeBuilder = new ();
    
    private static readonly LineRangeBuilder LineRangeBuilder = new ();
    
    private static readonly CursorLineRangeBuilder CursorLineRangeBuilder = new ();
    
    private static readonly Dictionary<string, HashSet<ParsedVector>> CachedParsedTiles = new ();

    public static HashSet<ParsedVector> BuildRange(string rangeId, RangeData rangeData)
    {
        if (CachedParsedTiles.TryGetValue(rangeId, out var parsedTiles)) 
            return parsedTiles;
        var result = new HashSet<ParsedVector>();
        foreach (var rangeTileData in rangeData.Tiles)
        {
            if (!Enum.TryParse<RelativeToType>(rangeTileData.RelativeTo, out var relativeTo))
            {
                Log.Error($"Ignored unknown relative to value: {rangeTileData.RelativeTo}.");
                continue;
            }
            HandleRangeType(rangeTileData, CircleRangeBuilder.Build(relativeTo, rangeTileData.Circles));
            HandleRangeType(rangeTileData, RectangleRangeBuilder.Build(relativeTo, rangeTileData.Rectangles));
            HandleRangeType(rangeTileData, LineRangeBuilder.Build(relativeTo, rangeTileData.Lines));
            HandleRangeType(rangeTileData, CursorLineRangeBuilder.Build(relativeTo, rangeTileData.CursorLines));
        }
        if (!RangeDataAsset.NonStaticRange(rangeId))
            CachedParsedTiles[rangeId] = result;
        return result;

        void HandleRangeType(RangeTileData data, IEnumerable<ParsedVector> parsedVectors)
        {
            if (data.Type is "Add")
                result.AddRange(parsedVectors);
            else if (data.Type is "Remove") 
                result.RemoveWhere(tile => parsedVectors.Any(r => 
                    r.Tile.X.Equals(tile.Tile.X) && r.Tile.Y.Equals(tile.Tile.Y)));
            else 
                Log.Error($"Ignored unknown range action type: {data.Type}.");
        }
    }
}
