using CombatExtended.Framework.Data;

namespace CombatExtended.Framework.Core.Areas.Ranges.Impl;

public class RectangleRangeBuilder : RangeBuilderImpl<RangeTileData.Rectangle>
{
    protected override IEnumerable<ParsedVector> Parse(RelativeToType relativeTo, RangeTileData.Rectangle rangeData)
    {
        for (var x = rangeData.From.X; x <= rangeData.To.X; x++)
        {
            for (var y = rangeData.From.Y; y <= rangeData.To.Y; y++)
            {
                yield return new ParsedVector(relativeTo, new Vector2(x + rangeData.Delta.X, y + rangeData.Delta.Y));
            }
        }
    }
}
