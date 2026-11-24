using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Core.Monsters.Buffs;
using CombatExtended.Framework.Core.Monsters.Statuses;
using CombatExtended.Framework.Core.Monsters.Tags;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.UI.Vanilla;
using CombatExtended.Framework.Util;
using CombatExtended.Integrations.Compatibility;
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
        monster.modData["Resistance".PrependId()] = serialized;
    }

    public static void ApplyArmourClass(this Monster monster, int ac)
    {
        monster.modData[MonsterStats.ArmourClassModDataKey] = ac.ToString();
        monster.missChance.Value = MonsterStats.MissChanceWithArmourClass(monster, ac);
    }

    public static void AddArmourClass(this Monster monster, int delta)
    {
        var resultAc = monster.GetArmourClass() + delta;
        monster.modData[MonsterStats.ArmourClassModDataKey] = resultAc.ToString();
        monster.missChance.Value = MonsterStats.MissChanceWithArmourClassDelta(monster, delta);
    }

    [SuppressMessage("ReSharper", "SpecifyACultureInStringConversionExplicitly",
        Justification = "Not displayed, for internal use.")]
    public static void ApplyWeight(this Monster monster, float weight)
    {
        monster.modData[MonsterStats.WeightModDataKey] = weight.ToString();
    }

    public static void AddWeight(this Monster monster, float delta)
    {
        var resultWeight = monster.GetWeight() + delta;
        monster.ApplyWeight(resultWeight);
    }
    
    [SuppressMessage("ReSharper", "SpecifyACultureInStringConversionExplicitly",
        Justification = "Not displayed, for internal use.")]
    public static void ApplyHitMissRate(this Monster monster, float rate)
    {
        monster.modData[MonsterStats.HitMissRateModDataKey] = rate.ToString();
    }

    public static void AddHitMissRate(this Monster monster, float delta)
    {
        var resultRate = monster.GetHitMissRate() + delta;
        monster.ApplyHitMissRate(resultRate);
    }

    public static int GetArmourClass(this Monster monster)
    {
        if (monster.modData.TryGetValue(MonsterStats.ArmourClassModDataKey, out var ac))
            return int.Parse(ac);
        return MonsterStats.DefaultArmourClass;
    }

    public static float GetWeight(this Monster monster)
    {
        if (monster.modData.TryGetValue(MonsterStats.WeightModDataKey, out var weight))
            return MathF.Max(-6, float.Parse(weight));
        return MonsterStats.DefaultWeight;
    }

    [SuppressMessage("ReSharper", "SpecifyACultureInStringConversionExplicitly",
        Justification = "Not displayed, for internal use.")]
    public static float GetHitMissRate(this Monster monster)
    {
        return float.Parse(
            monster.modData.GetValueOrDefault(MonsterStats.HitMissRateModDataKey,
                MonsterStats.DefaultHitMissRate.ToString())
        );
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
        return Tag.GetTags(monster).WithCompatibility(monster,
            m => (Tag.TryGetTags(m.Name, out var tags), tags ?? new List<Tag>()),
            t => (Tag.TryGetTags(t, out var tags), tags ?? new List<Tag>()),
            tags => tags
        );
    }
    
    public static Dictionary<string, MonsterStatusData> GetDataMonsterStatuses(this Monster monster)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monster.Name) || 
            MonsterExtensionDataAsset.Data[monster.Name].Statuses == null)
            return new Dictionary<string, MonsterStatusData>();
        var monsterStatuses = ArgUtility.SplitBySpace(MonsterExtensionDataAsset.Statuses[monster.Name]);
        return monsterStatuses.Select(id =>
                (id, MonsterStatusDataAsset.Data.GetValueOrDefault(id))
            ).Where(tuple => tuple.Item2 != null)
            .Select(tuple => (tuple.id, tuple.Item2!))
            .ToDictionary(tuple => tuple.id, tuple => tuple.Item2);
    }

    /// <summary>
    /// Get monster's armour class based on monster extension data.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <returns>Monster's armour class.</returns>
    public static int GetDataArmourClass(this Monster monster)
    {
        return (MonsterStats.DefaultArmourClass + MonsterStats.GetMonsterExtraArmourClass(monster))
            .WithCompatibility(monster,
                m => (MonsterStats.TryGetMonsterExtraArmourClass(m.Name, out var armourClass), armourClass ?? -1),
                t => (MonsterStats.TryGetMonsterExtraArmourClass(t, out var armourClass), armourClass ?? -1),
                extraAc => MonsterStats.DefaultArmourClass + extraAc
            );
    }

    public static float GetDataWeight(this Monster monster)
    {
        return (MonsterStats.DefaultWeight + MonsterStats.GetMonsterExtraWeight(monster))
            .WithCompatibility(monster,
                m => (MonsterStats.TryGetMonsterExtraWeight(m.Name, out var weight), weight ?? -1),
                t => (MonsterStats.TryGetMonsterExtraWeight(t, out var weight), weight ?? -1),
                extraWeight => MonsterStats.DefaultWeight + extraWeight
            );
    }

    public static float GetDataHitMissRate(this Monster monster)
    {
        return (MonsterStats.DefaultHitMissRate + MonsterStats.GetMonsterExtraHitMissRate(monster))
            .WithCompatibility(monster,
                m => (MonsterStats.TryGetMonsterExtraHitMissRate(m.Name, out var rate), rate ?? -1),
                t => (MonsterStats.TryGetMonsterExtraHitMissRate(t, out var rate), rate ?? -1),
                extraRate => MonsterStats.DefaultHitMissRate + extraRate
            );
    }

    /// <summary>
    /// Whether monster should be interacted with (receive damage, buffs, and so on).
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <returns>If monster should be interacted with.</returns>
    public static bool ShouldBeInteractedWith(this Monster monster)
    {
        if (monster.IsInvisible) return false;
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
        monster.AddWeight(attributes.Weight);
        monster.AddHitMissRate(attributes.HitMissRate);
        if (attributes.StunTime.Equals(-2f)) monster.stunTime.Value = int.MaxValue;
        else monster.stunTime.Value += attributes.StunTime.ToMilliseconds();
        //monster.Speed += (int)attributes.Speed;
        monster.addedSpeed += attributes.Speed;
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
        monster.AddWeight(-attributes.Weight);
        monster.AddHitMissRate(-attributes.HitMissRate);
        if (attributes.StunTime.Equals(-2f)) monster.stunTime.Value = 0;
        else monster.stunTime.Value -= attributes.StunTime.ToMilliseconds();
        monster.addedSpeed -= attributes.Speed;
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
    
    /// <summary>
    /// Apply monster status <paramref name="attributes"/> to given <paramref name="monster"/>.
    /// </summary>
    /// <param name="monster">The monster instance.</param>
    /// <param name="attributes">The monster status attributes to apply.</param>
    public static void ApplyMonsterStatusAttributes(this Monster monster, MonsterStatusAttributes attributes)
    {
        monster.resilience.Value += (int) attributes.Resilience;
        monster.MaxHealth += (int) attributes.MaxHealth;
        monster.Health += (int) attributes.Health;
        monster.missChance.Value += (int) attributes.MissChance;
        monster.AddArmourClass((int) attributes.ArmourClass);
        monster.AddWeight(attributes.Weight);
        monster.AddHitMissRate(attributes.HitMissRate);
        if (attributes.StunTime.Equals(-2f)) monster.stunTime.Value = int.MaxValue;
        else monster.stunTime.Value += (int) attributes.StunTime;
        //monster.Speed += (int)attributes.Speed;
        monster.addedSpeed += attributes.Speed;
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
    /// Revert back monster status <paramref name="attributes"/> from given <paramref name="monster"/>.
    /// </summary>
    /// <param name="monster">The monster instance.</param>
    /// <param name="attributes">The monster status attributes to remove.</param>
    public static void RemoveMonsterStatusAttributes(this Monster monster, MonsterStatusAttributes attributes)
    {
        monster.resilience.Value -= (int) attributes.Resilience;
        monster.MaxHealth -= (int) attributes.MaxHealth;
        monster.missChance.Value -= (int) attributes.MissChance;
        monster.AddArmourClass((int) -attributes.ArmourClass);
        monster.AddWeight(-attributes.Weight);
        monster.AddHitMissRate(-attributes.HitMissRate);
        if (attributes.StunTime.Equals(-2f)) monster.stunTime.Value = 0;
        else monster.stunTime.Value -= (int) attributes.StunTime;
        //monster.Speed -= (int)attributes.Speed;
        monster.addedSpeed -= attributes.Speed;
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
