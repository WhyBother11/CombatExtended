using CombatExtended.Framework.Data;
using StardewValley;

namespace CombatExtended.Framework.UI.Animations;

/// <summary>
/// Utility class extending <see cref="TemporaryAnimatedSprite"/> with constructor where most arguments have default values.
/// </summary>
public class TextureTemporaryAnimatedSprite : TemporaryAnimatedSprite
{
    public TextureTemporaryAnimatedSprite(
        string texturePath,
        Rectangle sourceRect,
        float animationInterval,
        int animationLength,
        int numberOfLoops,
        Vector2 position,
        float layerDepth,
        Color color,
        bool flicker = false,
        bool flipped = false,
        float alphaFade = 0.0f,
        float scale = 1f,
        float scaleChange = 0f,
        float rotation = 0f,
        float rotationChange = 0f
    ) : base(
        texturePath, sourceRect, animationInterval, animationLength, numberOfLoops, position, flicker,
        flipped, layerDepth, alphaFade, color, scale, scaleChange, rotation, rotationChange) {}

    public static TemporaryAnimatedSprite FromData(CombatTemporaryAnimatedSpriteData data,
        Vector2 position, float layerDepth = 1f, Color? color = null)
    {
        return new TextureTemporaryAnimatedSprite(
            texturePath: data.Texture,
            sourceRect: data.SourceRect,
            animationInterval: data.Interval,
            animationLength: data.Frames,
            numberOfLoops: data.Loops,
            position: position,
            layerDepth: layerDepth,
            color: color ?? Color.White,
            scale: data.Scale,
            alphaFade: data.AlphaFade,
            flicker: data.Flicker,
            flipped: data.Flip,
            scaleChange: data.ScaleChange,
            rotation: data.Rotation,
            rotationChange: data.RotationChange
        );
    }
}
