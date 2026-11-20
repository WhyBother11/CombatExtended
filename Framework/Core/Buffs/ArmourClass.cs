using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Buffs;

[HarmonyPatch]
public static class ArmourClass
{

    private static readonly PerScreen<int> FarmerArmourClass = new (() => 0);

    #region FarmerArmourClass
    /// <summary>
    /// Get armour class for the current player, clamped to [0, 19].
    /// </summary>
    /// <returns>Current player's armour class.</returns>
    public static int GetFarmerArmourClass()
    {
        var extraArmourClass = BuffExtensionEnabler.CalculateTotalArmourClass();

        return Math.Clamp(FarmerArmourClass.Value + extraArmourClass, 0, 19);
    }

    /// <summary>
    /// Add armour class directly to the current player.
    /// </summary>
    /// <param name="armourClass">Armour class to be added.</param>
    public static void AddFarmerArmourClass(int armourClass)
    {
        FarmerArmourClass.Value += armourClass;
    }

    public static void ClearFarmerArmourClass()
    {
        FarmerArmourClass.Value = 0;
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
}
