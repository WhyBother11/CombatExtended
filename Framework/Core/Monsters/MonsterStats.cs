using System.Diagnostics.CodeAnalysis;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters;

public static class MonsterStats
{
    
    public const double ArmourClassMissChancePerOne = 0.05;
    
    public const int DefaultArmourClass = 3;

    public const float DefaultWeight = 2f;
    
    public static readonly string WeightModDataKey = "Weight".PrependId();
    public static readonly string ArmourClassModDataKey = "ArmourClass".PrependId();

    #region ArmourClass
    public static int GetMonsterExtraArmourClass(Monster monster)
    {
        return GetMonsterExtraArmourClass(monster.Name);
    }
    
    private static int GetMonsterExtraArmourClass(string monsterName)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterName) || 
            MonsterExtensionDataAsset.Data[monsterName].ExtraArmourClass == null)
            return 0;
        return MonsterExtensionDataAsset.ExtraArmourClasses[monsterName];
    }
    
    public static bool TryGetMonsterExtraArmourClass(string monsterId, [NotNullWhen(true)] out int? ac)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterId) || 
            MonsterExtensionDataAsset.Data[monsterId].ExtraArmourClass == null)
        {
            ac = null;
            return false;
        }

        ac = GetMonsterExtraArmourClass(monsterId);
        return true;
    }
    
    public static float MonsterMissChanceWithArmourClass(Monster monster, int ac)
    {
        var missChance = monster.missChance.Value;
        var result = Math.Min(missChance + ArmourClassMissChancePerOne * ac, 
            19 * ArmourClassMissChancePerOne);
        return (float) result;
    }
    #endregion

    #region Weight
    public static float GetMonsterExtraWeight(Monster monster)
    {
        return GetMonsterExtraArmourClass(monster.Name);
    }
    
    private static float GetMonsterExtraWeight(string monsterName)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterName) || 
            MonsterExtensionDataAsset.Data[monsterName].ExtraWeight == null)
            return 0;
        return MonsterExtensionDataAsset.ExtraWeights[monsterName];
    }
    
    public static bool TryGetMonsterExtraWeight(string monsterId, [NotNullWhen(true)] out float? weight)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterId) || 
            MonsterExtensionDataAsset.Data[monsterId].ExtraWeight == null)
        {
            weight = null;
            return false;
        }

        weight = GetMonsterExtraWeight(monsterId);
        return true;
    }
    #endregion
}
