using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CombatExtended.Framework.UI;

public static class DrawUtil
{
    
    private static Texture2D? _cursorsTexture;
    private static Texture2D CursorsTexture => _cursorsTexture ??= 
        Helper.GameContent.Load<Texture2D>(Path.Combine("LooseSprites", "Cursors"));
    
    public const int DigitWidth = 5;
    public const int DigitHeight = 7;

    public static void DrawNumber(this SpriteBatch batch, char number, Vector2 position, float scale)//int x, int y, float scale)
    {
        var num = int.Parse(number.ToString());
        batch.Draw(
            texture: CursorsTexture,
            position: position,
            sourceRectangle: new Rectangle(368 + num * DigitWidth, 56, DigitWidth, DigitHeight),
            scale: scale
        );
    }
    
    /// <summary>
    /// Utility call to <see cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, float, SpriteEffects, float"/> with most arguments with default values.
    /// </summary>
    /// <param name="batch">Sprite batch that should be drawn upon.</param>
    /// <param name="texture">See aforementioned method.</param>
    /// <param name="position">See aforementioned method.</param>
    /// <param name="sourceRectangle">See aforementioned method.</param>
    /// <param name="color">See aforementioned method.</param>
    /// <param name="rotation">See aforementioned method.</param>
    /// <param name="origin">See aforementioned method.</param>
    /// <param name="scale">See aforementioned method.</param>
    /// <param name="effects">See aforementioned method.</param>
    /// <param name="layerDepth">See aforementioned method.</param>
    public static void Draw(
        this SpriteBatch batch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color? color = null,
        float rotation = 0f,
        Vector2? origin = null,
        float scale = 1f,
        SpriteEffects? effects = null,
        float layerDepth = 1f)
    {
        batch.Draw(
            texture,
            position,
            sourceRectangle,
            color ?? Color.White,
            rotation,
            origin ?? Vector2.Zero,
            scale,
            effects ?? SpriteEffects.None,
            layerDepth);
    }
}
