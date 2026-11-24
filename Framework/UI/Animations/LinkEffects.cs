using CombatExtended.Framework.Core.Misc.Projectiles;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using LinkEffectManager = CombatExtended.Framework.Managers.LinkEffectManager;

namespace CombatExtended.Framework.UI.Animations;

#pragma warning disable CS8618
public abstract class LinkEffect
{
    public const string StaticType = "Static";
    
    public const string SequenceType = "Sequence";
    
    public const string RandomType = "Random";
    
    public string Id { get; set; }
    
    protected string Type { get; init; }
    
    protected Texture2D Texture { get; init; }
    
    protected List<int> Frames { get; init; }
    
    protected float Scale { get; init; }
    
    protected Rectangle Size { get; init; }

    protected int Cursor = -1;
    
    public int DurationTicks { get; set; }

    public int TicksElapsed { get; set; }
    
    public int DoMonsterTouchEffectEverySec { get; set; }

    public abstract void Draw(SpriteBatch spriteBatch);

    protected LinkEffect(string id, LinkEffectData data)
    {
        Id = id;
        Type = data.Type;
        Texture = Helper.GameContent.Load<Texture2D>(data.Texture);
        Frames = data.Frames;
        Size = data.Size;
        Scale = data.Scale;
        DurationTicks = data.Duration.Equals(-2) ? int.MaxValue : data.Duration.ToTicks();
        if (data.TriggerEffectData is not null)
        {
            DoMonsterTouchEffectEverySec = data.TriggerEffectData.DoMonsterTouchEffectEverySec.ToTicks();
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
            StaticType => Frames.First(),
            SequenceType => Frames[(Cursor + 1) % Frames.Count],
            RandomType => Game1.random.ChooseFrom(Frames),
            _ => Frames.First()
        };
    }
}

public class ProjectileLinkEffect : LinkEffect
{
    
    private CustomProjectile Projectile { get; set; }

    public ProjectileLinkEffect(CustomProjectile projectile, string id, LinkEffectData data) : base(id, data)
    {
        Projectile = projectile;
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        var projectilePosition = Game1.GlobalToLocal(
            Game1.viewport, Projectile.position.Value + new Vector2(0.0f, -Projectile.height.Value) + new Vector2(32f, 32f));
        var rotatedOffset = Vector2.Transform(
            new Vector2(-Size.Width / 2f, Size.Height / 2f) * Scale,
            Matrix.CreateRotationZ(Projectile.Angle)
        );
        var linkPosition = projectilePosition + rotatedOffset;
        Draw(spriteBatch, linkPosition);
    }
}

public class MonsterLinkEffect : LinkEffect
{
    
    private Monster Monster { get; set; }
    
    public int DoMonsterEffectEverySec { get; set; }
    
    public Vector2? Offset { get; set; }
    
    public MonsterLinkEffect(Monster monster, string id, LinkEffectData data) : base(id, data)
    {
        Monster = monster;
        if (data.TriggerEffectData is not null)
        {
            DoMonsterEffectEverySec = data.TriggerEffectData.DoMonsterEffectEverySec.ToTicks();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var monsterPosition = Game1.GlobalToLocal(Game1.viewport, Monster.Position);
        var linkPosition = monsterPosition + new Vector2(Monster.GetBoundingBox().Width / 2f, Monster.GetBoundingBox().Height / 2f);
        if (Offset is not null)
            linkPosition += Offset.Value;
        Draw(spriteBatch, linkPosition);
    }
}

[Registrable]
public static class LinkEffects
{

    public static void Register(IModHelper helper)
    {
        Helper.Events.Display.RenderedWorld += OnRenderedWorld_DrawLink;
    }

    private static void OnRenderedWorld_DrawLink(object? sender, RenderedWorldEventArgs e)
    {
        foreach (var (_, manager) in LinkEffectManager.GetAll())
        {
            foreach (var (_, links) in manager.Links)
            {
                foreach (var (_, link) in links)
                {
                    link.Draw(e.SpriteBatch);
                }
            }
        }
    }
}
