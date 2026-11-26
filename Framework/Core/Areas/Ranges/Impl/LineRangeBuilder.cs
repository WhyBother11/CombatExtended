using CombatExtended.Framework.Data;

namespace CombatExtended.Framework.Core.Areas.Ranges.Impl;

public class LineRangeBuilder : RangeBuilderImpl<RangeTileData.Line>
{
    protected override IEnumerable<ParsedVector> Parse(RelativeToType relativeTo, RangeTileData.Line rangeData)
    {
        var radians = MathF.PI * (rangeData.Angle / 180.0f);
        var deltaX = MathF.Cos(radians);
        var deltaY = MathF.Sin(radians);
        return Util.GetLine(relativeTo, deltaX, deltaY, rangeData.MaxLength, rangeData.Delta, rangeData.Width);
    }
}
