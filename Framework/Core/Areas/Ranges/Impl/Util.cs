using CombatExtended.Framework.Data;

namespace CombatExtended.Framework.Core.Areas.Ranges.Impl;

public static class Util
{
    public static IEnumerable<ParsedVector> GetLine(RelativeToType relativeTo, float deltaX, float deltaY, 
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
}