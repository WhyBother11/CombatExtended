using StardewValley.Tools;

namespace CombatExtended.Framework.Events.Args;

public class WeaponEquippedEventArgs
{
    public readonly MeleeWeapon Equipped;

    public WeaponEquippedEventArgs(MeleeWeapon equipped)
    {
        Equipped = equipped;
    }
}
