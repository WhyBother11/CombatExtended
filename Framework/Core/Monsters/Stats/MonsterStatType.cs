using CombatExtended.Framework.Data;

namespace CombatExtended.Framework.Core.Monsters.Stats;

/// <summary>
/// The enum storing values for different monster stats added by mod.
/// </summary>
public enum MonsterStatType
{
    ArmourClass,
    Weight,
    HitMissRate
}

public static class MonsterStatTypeExtensions
{
    /// <summary>
    /// Check if <see cref="MonsterExtensionDataAsset"/> contains given <paramref name="statType"/> value for <paramref name="monsterName"/>
    /// </summary>
    /// <param name="statType">The <see cref="MonsterStatType"/>.</param>
    /// <param name="monsterName">The name of monster that should be checked for defined monster stat.</param>
    /// <returns>Whether given <paramref name="statType"/> is defined for <paramref name="monsterName"/>.</returns>
    /// <exception cref="ArgumentException">Unknown <paramref name="statType"/>.</exception>
    public static bool Defined(this MonsterStatType statType, string monsterName)
    {
        return statType switch
        {
            MonsterStatType.ArmourClass => MonsterExtensionDataAsset.Data[monsterName].ExtraArmourClass is not null,
            MonsterStatType.Weight => MonsterExtensionDataAsset.Data[monsterName].ExtraWeight is not null,
            MonsterStatType.HitMissRate => MonsterExtensionDataAsset.Data[monsterName].ExtraHitMissRate is not null,
            _ => throw new ArgumentException($"Unsupported stat type: {statType.ToString()}.")
        };
    }

    /// <summary>
    /// Get stat value for <paramref name="statType"/> and <paramref name="monsterName"/>, assuming it is defined for
    /// given monster name.
    /// </summary>
    /// <param name="statType">The <see cref="MonsterStatType"/>.</param>
    /// <param name="monsterName">The name of monster whose monster stat to get.</param>
    /// <returns>The stat value.</returns>
    /// <exception cref="ArgumentException">Unknown <paramref name="statType"/>.</exception>
    public static object GetValue(this MonsterStatType statType, string monsterName)
    {
        switch (statType)
        {
            case MonsterStatType.ArmourClass:
                return MonsterExtensionDataAsset.ExtraArmourClasses[monsterName];
            case MonsterStatType.Weight:
                return MonsterExtensionDataAsset.ExtraWeights[monsterName];
            case MonsterStatType.HitMissRate:
                return MonsterExtensionDataAsset.ExtraHitMissRates[monsterName];
        }
        throw new ArgumentException($"Unsupported stat type: {statType.ToString()}.");
    }
}
