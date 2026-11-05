using Microsoft.Xna.Framework;
using StardewValley;

namespace CombatExtended.Framework.UI.Animations;

/// <summary>
/// <see cref="TextureTemporaryAnimatedSprite"/> that is attached to some <see cref="Character"/>.
/// </summary>
public class AttachedTextureTemporaryAnimatedSprite : TextureTemporaryAnimatedSprite
{

    private readonly Character _attached;

    private Vector2 _prevAttachedPosition;
    
    public AttachedTextureTemporaryAnimatedSprite(
        string texturePath,
        Rectangle sourceRect,
        float animationInterval,
        int animationLength,
        int numberOfLoops,
        Vector2 position,
        float layerDepth,
        Color color,
        Character attachTo,
        bool flicker = false,
        bool flipped = false,
        float alphaFade = 0.0f,
        float scale = 1f,
        float scaleChange = 0f,
        float rotation = 0f,
        float rotationChange = 0f
    ) : base(
        texturePath, sourceRect, animationInterval, animationLength, numberOfLoops, position, layerDepth, color, flicker,
        flipped, alphaFade, scale, scaleChange, rotation, rotationChange)
    {
        _attached = attachTo;
        _prevAttachedPosition = attachTo.Position;
    }

    public override bool update(GameTime time)
    {
        var attachedDeltaX = _attached.Position.X - _prevAttachedPosition.X;
        var attachedDeltaY = _attached.Position.Y - _prevAttachedPosition.Y;
        Position = new Vector2(
            Position.X + attachedDeltaX,
            Position.Y + attachedDeltaY
        );
        _prevAttachedPosition = _attached.Position;
        return base.update(time);
    }
}
