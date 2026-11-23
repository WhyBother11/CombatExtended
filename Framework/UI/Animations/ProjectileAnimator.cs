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

    public ProjectileAnimator(Texture2D texture, int spriteIndex, int[]? animation = null)
    {
        _texture = texture;
        _spriteIndex = spriteIndex;
        _animation = animation;
    }

    public void DispatchDraw(
        SpriteBatch batch, Vector2 position, Projectile projectile, float rotation, float scale)
    {
        Rectangle sourceRect;
        if (_animation is null)
        {
            sourceRect = Game1.getSourceRectForStandardTileSheet(_texture, _spriteIndex, 16, 16);
            batch.Draw(_texture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0.0f, -projectile.height.Value) + new Vector2(32f, 32f)), 
                 sourceRect, projectile.color.Value * projectile.alpha.Value, rotation, new Vector2(8f, 8f), 
                 scale, SpriteEffects.None, (float) ((position.Y + 96.0) / 10000.0));
        }
        else
        {
            if (_cursor >= _animation.Length) _cursor = 0;
            if (_frame >= _animation[_cursor + 1])
            {
                //_cursor += 2;
                _cursor = (_cursor + 2) % _animation.Length;
                _frame = 0;
            }
            var spriteIndex = _animation[_cursor];
            sourceRect = Game1.getSourceRectForStandardTileSheet(_texture, spriteIndex, 16, 16);
            batch.Draw(_texture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0.0f, -projectile.height.Value) + new Vector2(32f, 32f)), 
                sourceRect, projectile.color.Value * projectile.alpha.Value, rotation, new Vector2(8f, 8f), 
                scale, SpriteEffects.None, (float) ((position.Y + 96.0) / 10000.0));
            _frame++;
        }
        
        var num = projectile.alpha.Value;
        var tail = projectile.tail;
        for (var num2 = tail.Count - 1; num2 >= 0; num2--)
        {
            batch.Draw(_texture, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp(
                    (num2 == tail.Count - 1) ? position : tail.ElementAt(num2 + 1), tail.ElementAt(num2), projectile.tailCounter / 50f
                    ) + new Vector2(0f, 0f - projectile.height.Value) + new Vector2(32f, 32f)), sourceRect, 
                projectile.color.Value * num, rotation, new Vector2(8f, 8f), scale, SpriteEffects.None, 
                (position.Y - (tail.Count - num2) + 96f) / 10000f);
            num -= 1f / tail.Count;
            scale = 0.8f * (4 - 4f / (num2 + 4));
        }
    }
}
