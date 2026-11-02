using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Projectiles;

namespace CombatExtended.Framework.UI.Animations;

public class ProjectileAnimator
{
    private readonly Texture2D _texture;

    private readonly int _spriteIndex;

    private readonly int[]? _animation;

    private int _frame;

    private int _cursor;
    
    private int _tailLength;

    public ProjectileAnimator(Texture2D texture, int spriteIndex, int[]? animation = null, int tailLength = 1)
    {
        _texture = texture;
        _spriteIndex = spriteIndex;
        _animation = animation;
        _tailLength = tailLength;
    }

    public void DispatchDraw(
        SpriteBatch batch, Vector2 position, Projectile projectile, float rotation, float scale
    )
    {
        if (_animation is null)
        {
            var sourceRect = Game1.getSourceRectForStandardTileSheet(_texture, _spriteIndex, 16, 16);
            batch.Draw(_texture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0.0f, -projectile.height.Value) + new Vector2(32f, 32f)), 
                 sourceRect, projectile.color.Value * projectile.alpha.Value, rotation, new Vector2(8f, 8f), 
                 scale, SpriteEffects.None, (float) ((position.Y + 96.0) / 10000.0));
        }
        else
        {
            if (_cursor >= _animation.Length) _cursor = 0;
            if (_frame >= _animation[_cursor + 1])
            {
                _cursor++;
                _frame = 0;
            }
            var spriteIndex = _animation[_cursor];
            var sourceRect = Game1.getSourceRectForStandardTileSheet(_texture, spriteIndex, 16, 16);
            batch.Draw(_texture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0.0f, -projectile.height.Value) + new Vector2(32f, 32f)), 
                sourceRect, projectile.color.Value * projectile.alpha.Value, rotation, new Vector2(8f, 8f), 
                scale, SpriteEffects.None, (float) ((position.Y + 96.0) / 10000.0));
            _frame++;
        }
    }
}
