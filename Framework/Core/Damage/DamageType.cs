using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.UI.Vanilla;
using StardewValley;
using StardewValley.Monsters;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.Core.Damage;

public static class DamageType
{
    public const string Physical = "Physical";
    public const string Ice = "Ice";
    public const string Fire = "Fire";
    public const string Lightning = "Lightning";
    public const string Radiant = "Radiant";
    public const string Poison = "Poison";
    
    /// <summary>
    /// Damage <paramref name="monster"/> directly and display <paramref name="damage"/> with custom <paramref name="damageColor"/>.
    /// </summary>
    /// <param name="location">Location with <paramref name="monster"/> to be damaged.</param>
    /// <param name="monster">The monster to be damaged.</param>
    /// <param name="damage">The damage to <paramref name="monster"/>.</param>
    /// <param name="player">The current player.</param>
    /// <param name="damageType">The type of damage.</param>
    /// <param name="damageColor">The display colour of damage.</param>
    public static void DamageMonsterDirectly(
        this GameLocation location,
        Monster monster,
        int damage,
        Farmer player,
        string damageType,
        Color damageColor
    )
    {
        var actual = monster.TakeDamageWithDamageType(damage, damageType, player);
        DamageNumbersDebris.Display(location, monster, actual, damageColor);
    }
    
    public static int TakeDamageWithDamageType(
        this Monster monster,
        int damage,
        string damageTypeId,
        Farmer? who)
    {
        if (!monster.ShouldBeInteractedWith()) return -1;
        var actualDamage = GetDamageWithResistances(monster, damage, damageTypeId, who);
        monster.Health -= actualDamage;
        if (monster.Health <= 0)
        {
            monster.deathAnimation();
            Utility.RaiseMonsterDeathTrigger(monster, monster.currentLocation);
        }

        return actualDamage;
    }

    public static int GetDamageWithResistances(
        Monster monster,
        int damage,
        string damageTypeId,
        Farmer? who)
    {
        var resistance = monster.GetResistances();
        var mult = resistance.GetMult(damageTypeId);
        if (mult.Equals(0.0)) return 0;
        return (int) Math.Max(1, damage * mult);
    }
}
