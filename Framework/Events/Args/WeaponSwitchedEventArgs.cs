using StardewValley.Tools;

namespace CombatExtended.Framework.Events.Args;

public class WeaponSwitchedEventArgs
{
    public readonly MeleeWeapon Equipped;
    
    public readonly MeleeWeapon Unequipped;

    public WeaponSwitchedEventArgs(MeleeWeapon equipped, MeleeWeapon unequipped)
    {
        Equipped = equipped;
        Unequipped = unequipped;
    }
}