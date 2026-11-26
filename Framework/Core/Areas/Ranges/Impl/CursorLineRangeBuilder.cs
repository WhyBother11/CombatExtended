using CombatExtended.Framework.Data;
using StardewValley;

namespace CombatExtended.Framework.Core.Areas.Ranges.Impl;

public class CursorLineRangeBuilder : RangeBuilderImpl<RangeTileData.CursorLine>
{
    protected override IEnumerable<ParsedVector> Parse(RelativeToType relativeTo, RangeTileData.CursorLine rangeData)
    {
        var player = Game1.player;
        var playerTile = player.Tile;
        var cursorTile = Helper.Input.GetCursorPosition().Tile;
        var deltaX = cursorTile.X - playerTile.X;
        var deltaY = cursorTile.Y - playerTile.Y;
        return Util.GetLine(RelativeToType.Player, deltaX, deltaY, rangeData.MaxLength, rangeData.Delta, rangeData.Width);
    }
}
