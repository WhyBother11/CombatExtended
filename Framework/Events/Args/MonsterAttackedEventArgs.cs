using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace CombatExtended.Framework.Events.Args;

public class MonsterAttackedEventArgs
{
    public readonly Farmer Player;

    public readonly MeleeWeapon? Weapon;
    
    public readonly Monster Monster;

    public readonly int Damage;

    public readonly bool IsCrit;
    
    public MonsterAttackedEventArgs(Farmer player, MeleeWeapon? weapon, Monster monster, int damage, bool isCrit)
    {
        Player = player;
        Weapon = weapon;
        Monster = monster;
        Damage = damage;
        IsCrit = isCrit;
    }
}
