using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using CombatExtended.Framework.Core.Buffs;
using CombatExtended.Framework.Data;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Damage;

[HarmonyPatch]
public static class ArmourClass
{
    
    public const int DefaultMonsterArmourClass = 3;

    public const double MissChancePerOne = 0.05;

    private static readonly PerScreen<int> FarmerArmourClass = new (() => 0);
    
    public static float MonsterMissChanceWithArmourClass(Monster monster, int ac)
    {
        var missChance = monster.missChance.Value;
        var result = Math.Min(missChance + MissChancePerOne * ac, 19 * MissChancePerOne);
        return (float) result;
    }

    #region FarmerArmourClass
    /// <summary>
    /// Get armour class for the current player, clamped to [0, 19].
    /// </summary>
    /// <returns>Current player's armour class.</returns>
    public static int GetFarmerArmourClass()
    {
        var buffArmourClass = 0;
        foreach (var (_, buff) in Game1.player.buffs.AppliedBuffs)
        {
            buffArmourClass += BuffExtensionEnabler.GetArmourClass(buff);
        }

        return Math.Clamp(FarmerArmourClass.Value + buffArmourClass, 0, 19);
    }

    /// <summary>
    /// Add armour class directly to the current player.
    /// </summary>
    /// <param name="armourClass">Armour class to be added.</param>
    public static void AddFarmerArmourClass(int armourClass)
    {
        FarmerArmourClass.Value += armourClass;
    }
    #endregion

    #region FarmerArmourClassPatches
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
        var armourClass = GetFarmerArmourClass();
        if (armourClass <= 0) return false;
        var random = Game1.random.Next(20);
        var result = random < armourClass;
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

    #region Data
    public static int GetExtraArmourClass(Monster monster)
    {
        return GetExtraArmourClass(monster.Name);
    }
    
    private static int GetExtraArmourClass(string monsterName)
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

        ac = GetExtraArmourClass(monsterId);
        return true;
    }
    #endregion
}
