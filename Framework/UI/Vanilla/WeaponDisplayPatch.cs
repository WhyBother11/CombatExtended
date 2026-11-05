using System.Reflection.Emit;
using System.Text;
using CombatExtended.Framework.Core.Weapons;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;

namespace CombatExtended.Framework.UI.Vanilla;

[HarmonyPatch]
public static class WeaponDisplayPatch
{

    /// <summary>
    /// Transpiler to add extra damage instances display when drawing tooltip in MeleeWeapon.drawTooltip
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.drawTooltip))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? MeleeWeapon_drawTooltip_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            var matcher = new CodeMatcher(instructions);
            // match Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Damage", ...)
            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldarg_1),
                new CodeMatch(OpCodes.Ldsfld),
                new CodeMatch(OpCodes.Ldstr, "Strings\\UI:ItemHover_Damage")
            ).ThrowIfNotMatch("Failed to match part of weapon damage drawing code.");
            // match y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Ldind_I4),
                new CodeMatch(OpCodes.Ldarg_S, (byte) 4),
                new CodeMatch(OpCodes.Ldstr, "TT"),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ldfld),
                new CodeMatch(OpCodes.Ldc_R4, 48f),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Conv_I4),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Stind_I4)
            ).ThrowIfNotMatch("Failed to match y adjustment for drawn weapon damage numbers.");
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldarg_3),
                new CodeInstruction(OpCodes.Ldarg_S, 4),
                new CodeInstruction(OpCodes.Ldarg_S, 5),
                new CodeInstruction(OpCodes.Call, 
                    AccessTools.Method(typeof(WeaponDisplayPatch), nameof(DrawExtraDamageTypeDamageNumbers)))
            );
            return matcher.Instructions();
        }
        catch (Exception e)
        {
            Log.Error("Failed injecting patch for extra damage type tooltip displaying.", e);
            return instructions;
        }
    }

    private static void DrawExtraDamageTypeDamageNumbers(
        MeleeWeapon weapon, SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont spriteFont, float alpha)
    {
        var extraDamageTypeData = WeaponStateEnabler.GetExtraDamageTypesWithState(weapon.ItemId);
        if (!extraDamageTypeData.Any()) return;
        var extraDamageTypeColors = DamageTypeDataAsset.DamageTypeColors;
        foreach (var data in extraDamageTypeData)
        {
            var text = $"+ {data.MinDamage}-{data.MaxDamage} {data.Type}";
            var co = extraDamageTypeColors[data.Type];
            SUtility.drawTextWithShadow(spriteBatch, text, spriteFont, new Vector2(x + 16 + 22, y + 16 + 12), co * 0.9f * alpha, 0.9f);
            y += (int)Math.Max(spriteFont.MeasureString("TT").Y, 32f);
        }
    }

    [HarmonyPatch(typeof(IClickableMenu), nameof(IClickableMenu.drawHoverText),
        typeof(SpriteBatch), typeof(StringBuilder), typeof(SpriteFont), typeof(int), typeof(int), typeof(int), typeof(string),
        typeof(int), typeof(string[]), typeof(Item), typeof(int), typeof(string), typeof(int), typeof(int), typeof(int),
        typeof(float), typeof(CraftingRecipe), typeof(IList<Item>), typeof(Texture2D), typeof(Rectangle?), typeof(Color?),
        typeof(Color?), typeof(float), typeof(int), typeof(int))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? IClickableMenu_drawHoverText_Transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        try
        {
            var matcher = new CodeMatcher(instructions);
            // match int boxHeight = ((boxHeightOverride != -1) ? boxHeightOverride : height);
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldarg_S),
                new CodeMatch(OpCodes.Ldc_I4_M1),
                new CodeMatch(OpCodes.Bne_Un_S),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Br_S),
                new CodeMatch(OpCodes.Ldarg_S),
                new CodeMatch(instr => instr.opcode == OpCodes.Stloc_S && instr.MatchByIndex(25))
            ).ThrowIfNotMatch("Failed to match boxHeight variable assignment.");
            // match end of width += 4;
            matcher.MatchEndBackwards(
                new CodeMatch(OpCodes.Stloc_1)
            ).ThrowIfNotMatch("Failed to match width adjustment right before boxHeight variable assignment.");
            matcher.Advance(1);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_S, 9),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WeaponDisplayPatch), nameof(AdjustTooltipHeight))),
                new CodeInstruction(OpCodes.Stloc_2),
                new CodeInstruction(OpCodes.Ldarg_S, 9),
                new CodeInstruction(OpCodes.Ldarg_2),
                new CodeInstruction(OpCodes.Ldloc_S, 6),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WeaponDisplayPatch), nameof(AdjustTooltipY))),
                new CodeInstruction(OpCodes.Stloc_S, 6)
            );
            return matcher.Instructions();
        }
        catch (Exception e)
        {
            Log.Error("Failed injecting patch for weapon tooltip height adjustment.", e);
            return instructions;
        }
    }

    /// <summary>
    /// Adjust <paramref name="height"/> to account for weapon damage for extra damage types.
    /// </summary>
    /// <param name="hoverItem">The hovered item.</param>
    /// <param name="spriteFont">The font used when drawing hovered text.</param>
    /// <param name="height">The height of tooltip box.</param>
    private static int AdjustTooltipHeight(Item? hoverItem, SpriteFont spriteFont, int height)
    {
        if (hoverItem is not MeleeWeapon weapon) return height;
        var extraDamageTypeData = WeaponStateEnabler.GetExtraDamageTypesWithState(weapon.ItemId);
        if (!extraDamageTypeData.Any()) return height;
        var extraHeightPerDamageType = (int)Math.Max(spriteFont.MeasureString("TT").Y, 32f);
        return height + extraHeightPerDamageType * extraDamageTypeData.Count;
    }
    
    /// <summary>
    /// Adjust <paramref name="y"/> to make extra height be considered when adjusting tooltip box position so it does not go out of screen.
    /// </summary>
    /// <param name="hoverItem">The hovered item.</param>
    /// <param name="spriteFont">The font used when drawing hovered text.</param>
    /// <param name="y">The y offest of tooltip box.</param>
    private static int AdjustTooltipY(Item? hoverItem, SpriteFont spriteFont, int y)
    {
        if (hoverItem is not MeleeWeapon weapon) return y;
        var extraDamageTypeData = WeaponStateEnabler.GetExtraDamageTypesWithState(weapon.ItemId);
        if (!extraDamageTypeData.Any()) return y;
        var extraHeightPerDamageType = (int)Math.Max(spriteFont.MeasureString("TT").Y, 32f);
        return y - extraHeightPerDamageType * extraDamageTypeData.Count;
    }
}
