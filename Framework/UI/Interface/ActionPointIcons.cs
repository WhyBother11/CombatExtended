using CombatExtended.Framework.Core.Misc.ActionPoints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CombatExtended.Framework.UI.Interface;

public class ActionPointIcons : IClickableMenu
{

    private static ActionPointIcons? _icons;
    public static ActionPointIcons Icons () => _icons ??= new ActionPointIcons();

    private List<ActionPoint> _actionPoints = new();

    private const int IconSize = 16;

    private const int ScaledIconSize = IconSize * 4;

    private const int ScaledFullIconSizeWidth = ScaledIconSize + 8;
    private const int ScaledFullIconSizeHeight = ScaledIconSize + DrawUtil.DigitHeight * 3 + 4;
    
    private int Hovered { get; set; } = -1;

    private int _colAmount;
    private int _rowAmount;

    private ActionPointIcons()
    {
        UpdatePosition();
    }
    
    public static void Register(IModHelper helper)
    {
        helper.Events.Display.RenderedHud += EventHandlers.OnRenderedHud_Draw;
        helper.Events.Display.WindowResized += EventHandlers.OnWindowResized_UpdatePosition;
        helper.Events.Input.CursorMoved += EventHandlers.OnCursorMoved_HandleCursorHover;
        helper.Events.GameLoop.ReturnedToTitle += (_, _) => { _icons = null; };
        helper.Events.GameLoop.DayEnding += (_, _) => { _icons = null; };
    }

    private void UpdatePosition()
    {
        xPositionOnScreen = 16;
        yPositionOnScreen = 16;
        _colAmount = (int) Math.Min(_actionPoints.Count, Game1.uiViewport.Width * 0.1 / ScaledFullIconSizeWidth);
        _rowAmount = _colAmount == 0 ? 0 : (int)MathF.Ceiling((float)_actionPoints.Count / _colAmount);
        height = ScaledFullIconSizeHeight * _rowAmount;
        width = ScaledFullIconSizeWidth * _colAmount;
    }
    
    public void NotifyActionPointsUpdated()
    {
        Hovered = -1;
        _actionPoints = ActionPointManager.ActionPoints.Values.ToList();
        // some stability ig
        // todo : remove
        //Log.Error(string.Join(' ', _actionPoints.Select(ap => ap.Id)));
        _actionPoints.Sort((ap1, ap2) => ap1.Id.CompareTo(ap2.Id));
        UpdatePosition();
    }

    public override void draw(SpriteBatch b)
    {
        if (Game1.activeClickableMenu != null || _actionPoints.Count <= 0) return;
        foreach (var (ap, index) in _actionPoints.Select((ap, index) => (ap, index)))
        {
            var yOffset = index / _colAmount * ScaledFullIconSizeHeight;
            DrawActionPoint(b, ap, index, yOffset);
        }
    }

    public override void performHoverAction(int x, int y)
    {
        if (!isWithinBounds(x, y))
        {
            Hovered = -1;
            return;
        }

        var yIndex = (y - yPositionOnScreen) / ScaledFullIconSizeHeight;
        var xIndex = (x - xPositionOnScreen) / ScaledFullIconSizeWidth;
        var index = xIndex + _colAmount * yIndex;
        if (index < 0 || index >= _actionPoints.Count)
        {
            Hovered = -1;
            return;
        }

        Hovered = index;
    }

    private void DrawActionPoint(SpriteBatch b, ActionPoint ap, int index, int yOffset)
    {
        var xIndex = index % _colAmount;
        var xPosition = xPositionOnScreen + xIndex * ScaledFullIconSizeWidth;
        var yPosition = yPositionOnScreen + yOffset;
        var position = new Vector2(xPosition, yPosition);
        var actionPointSourceRect = 
            Game1.getSourceRectForStandardTileSheet(ap.Texture, ap.SpriteIndex, IconSize, IconSize);
        b.Draw(ap.Texture, position, actionPointSourceRect, scale: 4);
        DrawActionPointAmount(b, ap.CurrentAmount, xPosition, yPosition + ScaledIconSize);
        
        if (Hovered != -1)
        {
            var description = Game1.parseText(_actionPoints[Hovered].Description, Game1.smallFont, 260);
            var displayName = Game1.parseText(_actionPoints[Hovered].DisplayName, Game1.smallFont, 260);
            drawToolTip(b, description, displayName, null);
        }
    }

    private const int AdjustedDigitWidth = DrawUtil.DigitWidth * 3 - 1;
    
    private static void DrawActionPointAmount(SpriteBatch b, int amount, int x, int y)
    {
        var amountStr = amount.ToString().ToList();
        var middleX = x + IconSize * 2 - amountStr.Count * AdjustedDigitWidth / 2;
        x = middleX;
        foreach (var charNum in amountStr)
        {
            b.DrawNumber(charNum, new Vector2(x, y), 3f);
            x += AdjustedDigitWidth;
        }
        
    }

    private static class EventHandlers
    {
        public static void OnRenderedHud_Draw(object? sender, RenderedHudEventArgs e)
        {
            if (!Context.IsPlayerFree) return;
            Icons().draw(e.SpriteBatch);
        }

        public static void OnWindowResized_UpdatePosition(object? sender, WindowResizedEventArgs e)
        {
            if (!Context.IsPlayerFree) return;
            Icons().UpdatePosition();
        }

        public static void OnCursorMoved_HandleCursorHover(object? sender, CursorMovedEventArgs e)
        {
            var scaled = SUtility.ModifyCoordinatesForUIScale(e.NewPosition.ScreenPixels);
            var x = (int)scaled.X;
            var y = (int)scaled.Y;
            Icons().performHoverAction(x, y);
        }
    }
}
