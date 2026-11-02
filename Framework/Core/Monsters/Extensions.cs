using System.Text.Json;
using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Core.Monsters.Buffs;
using CombatExtended.Framework.Core.Monsters.Tags;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.UI.Vanilla;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters;

public static class Extensions
{
    /// <summary>
    /// Get resistance data from <paramref name="monster"/>'s <see cref="Monster.modData"/>.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <returns><see cref="CombinedResistance"/> of <paramref name="monster"/></returns>
    public static CombinedResistance GetResistances(this Monster monster)
    {
        var serialized = monster.modData["Resistance".PrependId()];
        return JsonSerializer.Deserialize<CombinedResistance>(serialized) ?? new CombinedResistance();
    }

    /// <summary>
    /// Update resistance data or add to <paramref name="monster"/>'s <see cref="Monster.modData"/>.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <param name="resistance"><see cref="CombinedResistance"/>.</param>
    public static void UpdateResistance(this Monster monster, CombinedResistance resistance)
    {
        var serialized = JsonSerializer.Serialize(resistance);
        monster.modData[$"Resistance".PrependId()] = serialized;
    }

    public static void AddArmourClass(this Monster monster, int ac)
    {
        monster.missChance.Value = ArmourClass.MonsterMissChanceWithArmourClass(monster, ac);
    }

    /// <summary>
    /// Get approximate position on top of monster sprite.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <returns>Pixel coordinates close to the top of monster sprite.</returns>
    public static Vector2 GetApproximatePositionOnTop(this Monster monster)
    {
        var basePosX = monster.Position.X;
        var basePosY = monster.Position.Y - monster.Sprite.getHeight() - 16;
        if (monster is DinoMonster or BigSlime or Serpent)
            basePosX += monster.Sprite.getWidth() / 4f;
        return new Vector2(basePosX, basePosY);
    }
    
    /// <summary>
    /// Get approximate position on top of monster sprite.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <returns>Pixel coordinates close to the top of monster sprite.</returns>
    public static Vector2 GetApproximatePositionOnBottom(this Monster monster)
    {
        var basePosX = monster.Position.X;
        var basePosY = monster.Position.Y - monster.Sprite.getHeight() - 16;
        if (monster is DinoMonster or BigSlime or Serpent)
            basePosX += monster.Sprite.getWidth() / 4f;
        return new Vector2(basePosX, basePosY);
    }
    
    /// <summary>
    /// Get unsorted monster specific list of <see cref="Tag"/> based on <see cref="Monster.Name"/>.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <returns>List of monster specific tags.</returns>
    public static List<Tag> GetTags(this Monster monster)
    {
        return Tag.GetTags(monster);
    }

    /// <summary>
    /// Get monster's armour class based on monster extension data.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <returns>Monster's armour class.</returns>
    public static int GetArmourClass(this Monster monster)
    {
        return ArmourClass.DefaultMonsterArmourClass + ArmourClass.GetExtraArmourClass(monster);
    }

    /// <summary>
    /// Whether monster should be interacted with (receive damage, buffs, and so on).
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <returns>If monster should be interacted with.</returns>
    public static bool ShouldBeInteractedWith(this Monster monster)
    {
        return monster switch
        {
            Mummy { reviveTimer.Value: > 0 } => false,
            RockCrab { waiter: true } => false,
            Spiker => false,
            Bug { isArmoredBug.Value: true } => false,
            _ => true
        };
    }

    /// <summary>
    /// Apply monster buff <paramref name="attributes"/> to given <paramref name="monster"/>.
    /// </summary>
    /// <param name="monster">The monster instance.</param>
    /// <param name="attributes">The monster buff attributes to apply.</param>
    public static void ApplyMonsterBuffAttributes(this Monster monster, MonsterBuffAttributes attributes)
    {
        monster.resilience.Value += (int) attributes.Resilience;
        monster.MaxHealth += (int) attributes.MaxHealth;
        monster.Health += (int) attributes.Health;
        monster.missChance.Value += (int) attributes.MissChance;
        monster.AddArmourClass((int) attributes.ArmourClass);
        if (attributes.StunTime.Equals(-2f)) monster.stunTime.Value = int.MaxValue;
        else monster.stunTime.Value += (int) attributes.StunTime;
        foreach (var attributesResistance in attributes.Resistances)
        {
            if (!DamageTypeDataAsset.Exists(attributesResistance.DamageType))
            {
                Log.Warn($"Unknown damage type ID {attributesResistance.DamageType}");
                continue;
            }

            var current = monster.GetResistances();
            if (attributesResistance.Delta >= 0) current.IncreaseResistance(attributesResistance.DamageType, 
                (int) attributesResistance.Delta);
            else current.LowerResistance(attributesResistance.DamageType, (int) Math.Abs(attributesResistance.Delta));
        }
        foreach (var attributesDamage in attributes.Damages)
        {
            if (!DamageTypeDataAsset.Exists(attributesDamage.DamageType))
            {
                Log.Warn($"Unknown damage type ID {attributesDamage.DamageType}");
                continue;
            }

            var damageRoll = Game1.random.Next((int)attributesDamage.MinDamage, (int)attributesDamage.MaxDamage + 1);
            var damage =
                DamageType.TakeExtraTypePureDamage(monster, damageRoll, attributesDamage.DamageType, Game1.player);
            DamageNumbersDebris.Display(monster.currentLocation, monster, damage, 
                DamageTypeDataAsset.DamageTypeColors[attributesDamage.DamageType]);
        }
    }
    
    /// <summary>
    /// Revert back monster buff <paramref name="attributes"/> from given <paramref name="monster"/>.
    /// </summary>
    /// <param name="monster">The monster instance.</param>
    /// <param name="attributes">The monster buff attributes to remove.</param>
    public static void RemoveMonsterBuffAttributes(this Monster monster, MonsterBuffAttributes attributes)
    {
        monster.resilience.Value -= (int) attributes.Resilience;
        monster.MaxHealth -= (int) attributes.MaxHealth;
        monster.missChance.Value -= (int) attributes.MissChance;
        monster.AddArmourClass((int) -attributes.ArmourClass);
        if (!attributes.StunTime.Equals(-2f)) monster.stunTime.Value = 0;
        else monster.stunTime.Value -= (int) attributes.StunTime;
        foreach (var attributesResistance in attributes.Resistances)
        {
            if (!DamageTypeDataAsset.Exists(attributesResistance.DamageType))
            {
                Log.Warn($"Unknown damage type ID {attributesResistance.DamageType}");
                continue;
            }

            var current = monster.GetResistances();
            if (attributesResistance.Delta >= 0) current.LowerResistance(attributesResistance.DamageType, 
                (int) attributesResistance.Delta);
            else current.IncreaseResistance(attributesResistance.DamageType, (int) Math.Abs(attributesResistance.Delta));
        }
    }
}
