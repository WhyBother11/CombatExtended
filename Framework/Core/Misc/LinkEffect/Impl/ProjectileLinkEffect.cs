using CombatExtended.Framework.Core.Projectiles;
using CombatExtended.Framework.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace CombatExtended.Framework.Core.Misc.LinkEffect.Impl;

public class ProjectileLinkEffect : LinkEffect
{
    
    private CustomProjectile Projectile { get; set; }

    public ProjectileLinkEffect(CustomProjectile projectile, string id, LinkEffectData data) : base(id, data)
    {
        Projectile = projectile;
    }
    
    public override void Draw(SpriteBatch spriteBatch)
    {
        var linkPosition = GetLinkPosition();
        Draw(spriteBatch, linkPosition);
    }

    public override IEnumerable<Rectangle> GetHitBox()
    {
        return GetHitBox(GetLinkPosition());
    }

    private Vector2 GetLinkPosition()
    {
        var projectilePosition = Game1.GlobalToLocal(
            Game1.viewport, Projectile.position.Value + new Vector2(0.0f, -Projectile.height.Value) + new Vector2(32f, 32f));
        var rotatedOffset = Vector2.Transform(
            new Vector2(-Size.Width / 2f, Size.Height / 2f) * Scale,
            Matrix.CreateRotationZ(Projectile.Angle)
        );
        return projectilePosition + rotatedOffset;
    }
}
