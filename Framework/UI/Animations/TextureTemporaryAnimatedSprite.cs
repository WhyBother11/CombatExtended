using Microsoft.Xna.Framework;
using StardewValley;

namespace CombatExtended.Framework.UI.Animations;

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
}