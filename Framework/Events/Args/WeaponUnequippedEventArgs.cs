using StardewValley.Tools;

namespace CombatExtended.Framework.Events.Args;

public class WeaponUnequippedEventArgs
{
    public readonly MeleeWeapon Unequipped;

    public WeaponUnequippedEventArgs(MeleeWeapon unequipped)
    {
        Unequipped = unequipped;
    }
}
