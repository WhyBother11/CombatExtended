using StardewValley;
using StardewValley.Projectiles;

namespace CombatExtended.Framework.Core.Misc.Projectiles;

public static class Extensions
{
    /// <summary>
    /// Check that fired projectile belongs to local player and get their instance.
    /// </summary>
    /// <param name="projectile">Projectile instance.</param>
    /// <param name="location">Projectile location.</param>
    /// <returns>Local farmer instance if farmer is local player, <c>null</c> otherwise.</returns>
    public static Farmer? GetLocalFarmer(this Projectile projectile, GameLocation location)
    {
        var character = projectile.theOneWhoFiredMe.Get(location);
        if (character is not Farmer farmer) return null;
        return farmer.IsLocalPlayer ? farmer : null;
    }
}
