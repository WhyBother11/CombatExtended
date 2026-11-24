using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using CombatExtended.Framework.Core.Buffs;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using HarmonyLib;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters;

public static class MonsterStats
{
    
    public const float ArmourClassMissChancePerOne = 0.05f;
    
    public const int DefaultArmourClass = 3;

    public const float DefaultWeight = 2f;
    
    public const float DefaultHitMissRate = 0f;
    
    public static readonly string WeightModDataKey = "Weight".PrependId();
    public static readonly string ArmourClassModDataKey = "ArmourClass".PrependId();
    public static readonly string HitMissRateModDataKey = "HitMissRate".PrependId();

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
    
    public static float MissChanceWithArmourClass(Monster monster, int ac)
    {
        
        var missChance = monster.missChance.Value;
        return (float)missChance + ArmourClassMissChancePerOne * ac;
    }
    
    public static float MissChanceWithArmourClassDelta(Monster monster, int acDelta)
    {
        
        var missChance = monster.missChance.Value;
        return (float)missChance + ArmourClassMissChancePerOne * acDelta;
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
    
    #region HitMissRate
    public static float GetMonsterExtraHitMissRate(Monster monster)
    {
        return GetMonsterExtraHitMissRate(monster.Name);
    }
    
    private static float GetMonsterExtraHitMissRate(string monsterName)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterName) || 
            MonsterExtensionDataAsset.Data[monsterName].ExtraHitMissRate == null)
            return 0;
        return MonsterExtensionDataAsset.ExtraWeights[monsterName];
    }
    
    public static bool TryGetMonsterExtraHitMissRate(string monsterId, [NotNullWhen(true)] out float? hitMissRate)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterId) || 
            MonsterExtensionDataAsset.Data[monsterId].ExtraHitMissRate == null)
        {
            hitMissRate = null;
            return false;
        }

        hitMissRate = GetMonsterExtraWeight(monsterId);
        return true;
    }
    
    [HarmonyPatch(typeof(Farmer), nameof(Farmer.takeDamage))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Farmer_takeDamage_Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        try
        {
            var matcher = new CodeMatcher(instructions);
            // match damager?.onDealContactDamage(this);
            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Brfalse_S),
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Callvirt,
                    AccessTools.Method(typeof(Monster), nameof(Monster.onDealContactDamage)))
            );
            var label = generator.DefineLabel();
            var labels = matcher.Instruction.ExtractLabels();
            matcher.Instruction.WithLabels(label);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labels),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ArmourClass), nameof(HandleDamageMiss))),
                new CodeInstruction(OpCodes.Brfalse_S, label),
                new CodeInstruction(OpCodes.Ret)
            );
            return matcher.Instructions();
        }
        catch (Exception e)
        {
            Log.Error("Failed injecting patch for armour miss chance", e);
            return instructions;
        }
    }

    /// <summary>
    /// Check if damage dealt to player should be missed.
    /// </summary>
    /// <param name="player">Damaged player.</param>
    /// <param name="damager">Monster that dealt damage, if any.</param>
    /// <returns><c>true</c> if damage should be missed, <c>false</c> otherwise.</returns>
    private static bool HandleDamageMiss(Farmer player, Monster? damager)
    {
        if (damager is null) return false;
        var hitMissRate = damager.GetHitMissRate();
        if (hitMissRate <= 0) return false;
        var result = Game1.random.NextDouble() < hitMissRate;
        if (result)
        {
            var debrisOrigin = new Vector2(
                player.Position.X - Game1.viewport.X, player.Position.Y - Game1.viewport.Y
            );
            // TODO: replace MISS with actual i18n string (and test if it even works)
            Game1.player.currentLocation.debris.Add(new Debris("MISS!!!", 1, debrisOrigin, Color.LightGray, 1f, 0.0f));
        }
        return result;
    }
    #endregion
}
