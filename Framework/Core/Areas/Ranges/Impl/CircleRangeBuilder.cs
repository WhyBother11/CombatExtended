using CombatExtended.Framework.Data;

namespace CombatExtended.Framework.Core.Areas.Ranges.Impl;

public class CircleRangeBuilder : RangeBuilderImpl<RangeTileData.Circle>
{
    protected override IEnumerable<ParsedVector> Parse(RelativeToType relativeTo, RangeTileData.Circle rangeData)
    {
        var radiusPlusOneSquared = (rangeData.Radius + 1) * (rangeData.Radius + 1);
        for (var x = 0; x <= rangeData.Radius; x++)
        {
            for (var y = 0; y <= rangeData.Radius; y++)
            {
                if (x * x + y * y >= radiusPlusOneSquared) continue;
                yield return new ParsedVector(relativeTo, new Vector2(x + rangeData.Delta.X, y + rangeData.Delta.Y));
                yield return new ParsedVector(relativeTo, new Vector2(-x + rangeData.Delta.X, y + rangeData.Delta.Y));
                yield return new ParsedVector(relativeTo, new Vector2(x + rangeData.Delta.X, -y + rangeData.Delta.Y));
                yield return new ParsedVector(relativeTo, new Vector2(-x + rangeData.Delta.X, -y + rangeData.Delta.Y));
            }
        }
    }
}
