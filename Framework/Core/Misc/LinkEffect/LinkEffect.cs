using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Extensions;

namespace CombatExtended.Framework.Core.Misc.LinkEffect;

public enum LinkType
{
    Static,
    Sequence,
    Random
}

public abstract class LinkEffect
{
    
    public string Id { get; set; }
    
    protected LinkType Type { get; init; }
    
    protected Texture2D Texture { get; init; }
    
    protected List<int> Frames { get; init; }
    
    protected float Scale { get; init; }
    
    protected Rectangle Size { get; init; }

    protected int Cursor = -1;
    
    public int DurationTicks { get; set; }
    
    public int DoMonsterTouchEffectEveryTick { get; set; }
    
    public int TicksElapsed { get; set; }

    public abstract void Draw(SpriteBatch spriteBatch);
    
    public abstract IEnumerable<Rectangle> GetHitBox();

    protected LinkEffect(string id, LinkEffectData data)
    {
        Id = id;
        if (Enum.TryParse<LinkType>(data.Type, out var type))
            Type = type;
        else
        {
            Log.Error($"Unknown link effect type: {data.Type}, defaulting to 'StaticType'");
            Type = LinkType.Static;
        }
        Texture = Helper.GameContent.Load<Texture2D>(data.Texture);
        Frames = data.Frames;
        Size = data.Size;
        Scale = data.Scale;
        DurationTicks = data.Duration.CheckIfIndefiniteAndModify(v => v.ToTicks());
        if (data.TriggerEffectData is not null)
        {
            DoMonsterTouchEffectEveryTick = data.TriggerEffectData.DoMonsterTouchEffectEverySec.ToTicks();
        }
    }
    
    protected IEnumerable<Rectangle> GetHitBox(Vector2 linkPosition)
    {
        var playerPosition = Game1.GlobalToLocal(Game1.viewport, Game1.player.Position);
        var delta = linkPosition - playerPosition;
        if (delta is { X: 0, Y: 0 }) yield break;
        var angle = MathF.Atan2(delta.Y, delta.X) + MathF.PI / 2;
        var (sin, cos) = MathF.SinCos(angle);
        var length = delta.Length();
        var dir = delta / length;
        
        var segmentLength = Size.Height * Scale;
        var segmentAmount = (int)MathF.Ceiling(length / segmentLength);
        for (var i = 0; i < segmentAmount; i++)
        { 
            var position = linkPosition - dir * segmentLength * i;
            var aligned = new Vector2(
                MathF.Min(position.X, position.X - Size.Height * sin * Scale),
                MathF.Min(position.Y, position.Y + Size.Width * sin * Scale)
            );
            yield return new Rectangle((int)aligned.X, (int)aligned.Y,
                (int)(MathF.Abs(cos) * Size.Width * Scale + MathF.Abs(sin) * Size.Height * Scale ),
                (int)(MathF.Abs(sin) * Size.Width * Scale + MathF.Abs(cos) * Size.Height * Scale )
            );
        }
    }

    protected void Draw(SpriteBatch spriteBatch, Vector2 linkPosition)
    {
        var playerPosition = Game1.GlobalToLocal(Game1.viewport, Game1.player.Position);
        var delta = linkPosition - playerPosition;
        if (delta is { X: 0, Y: 0 }) return;
        var angle = MathF.Atan2(delta.Y, delta.X) + MathF.PI / 2;
        var length = delta.Length();
        var dir = delta / length;
        
        var segmentLength = Size.Height * Scale;
        var segmentAmount = (int)MathF.Floor(length / segmentLength);
        for (var i = 0; i < segmentAmount; i++)
        {
            UpdateCursor();
            var rectangle = Game1.getSourceRectForStandardTileSheet(Texture, Frames[Cursor], Size.Width, Size.Height);
            var position = linkPosition - dir * segmentLength * i;
            spriteBatch.Draw(
                Texture, position, rectangle, Color.White, angle, Vector2.Zero, Scale, SpriteEffects.None, 0f
            );
        }
    }
    
    protected void UpdateCursor()
    {
        Cursor = Type switch
        {
            LinkType.Static => Frames.First(),
            LinkType.Sequence => Frames[(Cursor + 1) % Frames.Count],
            LinkType.Random => Game1.random.ChooseFrom(Frames),
            _ => Frames.First()
        };
    }
}
