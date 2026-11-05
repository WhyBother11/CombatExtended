using CombatExtended.Framework.Core.Monsters;
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
    
    public static int TakeExtraTypePureDamage(
        Monster monster,
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
