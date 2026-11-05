using HarmonyLib;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.UI.Vanilla;

/// <summary>
/// Utility class to enable number debris with static text colour.
/// </summary>
[HarmonyPatch]
public class DamageNumbersDebris : Debris
{

    private Color _overrideColor;
    
    public DamageNumbersDebris(int number, Vector2 debrisOrigin, Color messageColor, float scale, Character toHover)
    {
        chunkType.Value = number;
        debrisType.Value = DebrisType.NUMBERS;
        nonSpriteChunkColor.Value = messageColor;
        InitializeChunks(1, debrisOrigin, Game1.player.Position);
        Chunks[0].scale = scale;
        this.toHover = toHover;
        Chunks[0].xVelocity.Value = Game1.random.Next(-1, 2);
        updateHoverPosition(Chunks[0]);
        _overrideColor = messageColor;
    }
    
    /// <summary>
    /// Postfix to override debris colour with a static one.
    /// </summary>
    /// <param name="__instance">DamageNumbersDebris instance.</param>
    [HarmonyPatch(typeof(Debris), nameof(updateHoverPosition))]
    [HarmonyPostfix]
    private static void Debris_updateHoverPosition_Postfix(Debris __instance)
    {
        if (__instance is not DamageNumbersDebris damageDebris) return;
        damageDebris.nonSpriteChunkColor.R = damageDebris._overrideColor.R;
        damageDebris.nonSpriteChunkColor.G = damageDebris._overrideColor.G;
        damageDebris.nonSpriteChunkColor.B = damageDebris._overrideColor.B;
    }
    
    /// <summary>
    /// Display damage numbers with given <paramref name="damageColor"/>
    /// </summary>
    /// <param name="location">Location instance.</param>
    /// <param name="monster">Monster instance.</param>
    /// <param name="deltaX">X offset for damage amount relative to monster's BoundingBox center.</param>
    /// <param name="deltaY">Y offset for damage amount relative to monster's BoundingBox center.</param>
    /// <param name="damage">Amount of damage that should be displayed.</param>
    /// <param name="damageColor">Colour of damage amount.</param>
    public static void Display(
        GameLocation location,
        Monster monster,
        int damage,
        Color damageColor,
        int deltaX = 24,
        int deltaY = 16)
    {
        var debris = new DamageNumbersDebris(
            damage,
            new Vector2(monster.GetBoundingBox().Center.X + deltaX, monster.GetBoundingBox().Center.Y + deltaY),
            damageColor,
            1f,
            monster
        );
        location.debris.Add(debris);
    }
}
