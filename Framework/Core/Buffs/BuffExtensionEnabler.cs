using System.Runtime.CompilerServices;
using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Core.Buffs;

[Registrable]
[HarmonyPatch]
public static class BuffExtensionEnabler
{

    private static readonly string BuffCustomFieldsArmourClassKey = "ArmourClass".PrependId();
    private static readonly string BuffCustomFieldsFlatCritChanceKey = "FlatCriticalChance".PrependId();
    private static readonly string BuffCustomFieldsFlatCritMultKey = "FlatCriticalMultiplier".PrependId();
    
    private static readonly PerScreen<ConditionalWeakTable<Buff, BuffExtension>> Extensions = new (
        () => new ConditionalWeakTable<Buff, BuffExtension>());

    public static void Register(IModHelper helper)
    {
        BuffsDisplay.displayAttributes.Add(new BuffAttributeDisplay(
            texture: () => helper.ModContent.Load<Texture2D>("assets/armour_class_buff_icon.png"),
            spriteIndex: 0,
            value: buff => GetArmourClass(buff),
            description: value => 
                string.Format(helper.Translation.Get("string.buff.armour-class.description"), 
                (int) value)
            ));
        helper.Events.GameLoop.DayEnding += (_, _) =>
        {
            Extensions.Value.Clear();
            ArmourClass.ClearFarmerArmourClass();
        };
    }

    /// <summary>
    /// Add armour class attached to applied <paramref name="buff"/>.
    /// </summary>
    /// <param name="buff">Applied buff.</param>
    /// <param name="armourClass">Armour class to be added.</param>
    public static void AddArmourClass(Buff buff, int armourClass)
    {
        Extensions.Value.GetValue(buff, _ => new BuffExtension()).ArmourClass += armourClass;
    }

    public static void AddFlatCritChance(Buff buff, float flatCritChance)
    {
        Extensions.Value.GetValue(buff, _ => new BuffExtension()).FlatCritChance += flatCritChance;
    }
    
    public static void AddFlatCritMult(Buff buff, float flatCritMult)
    {
        Extensions.Value.GetValue(buff, _ => new BuffExtension()).FlatCritMultiplier += flatCritMult;
    }

    public static void RemoveBuffExtension(Buff buff)
    {
        Extensions.Value.Remove(buff);
    }

    public static int CalculateTotalArmourClass(Buff? removed = null)
    {
        var buffArmourClass = 0;
        foreach (var (_, playerBuff) in Game1.player.buffs.AppliedBuffs)
        {
            if (playerBuff.id == removed?.id) continue;
            buffArmourClass += GetArmourClass(playerBuff);
        }

        return buffArmourClass;
    }

    public static float CalculateTotalFlatCritChance()
    {
        var buffFlatCritChance = 0f;
        foreach (var (_, playerBuff) in Game1.player.buffs.AppliedBuffs)
        {
            buffFlatCritChance += GetFlatCritChance(playerBuff);
        }
        return buffFlatCritChance;
    }

    public static float CalculateTotalFlatCritMultiplier()
    {
        var buffFlatCritMult = 0f;
        foreach (var (_, playerBuff) in Game1.player.buffs.AppliedBuffs)
        {
            buffFlatCritMult += GetFlatCritMult(playerBuff);
        }
        return buffFlatCritMult;
    }
    
    /// <summary>
    /// Get armour class attached to given <paramref name="buff"/>.
    /// </summary>
    /// <param name="buff">Buff instance.</param>
    /// <returns>Buff's armour class.</returns>
    public static int GetArmourClass(Buff buff) => Extensions.Value.TryGetValue(buff, out var extension) ? extension.ArmourClass : 0;
    
    /// <summary>
    /// Get flat critical strike chance attached to given <paramref name="buff"/>.
    /// </summary>
    /// <param name="buff">Buff instance.</param>
    /// <returns>Buff's flat crit chance.</returns>
    public static float GetFlatCritChance(Buff buff) => Extensions.Value.TryGetValue(buff, out var extension) ? extension.FlatCritChance : 0f;
    
    /// <summary>
    /// Get flat critical strike multiplier attached to given <paramref name="buff"/>.
    /// </summary>
    /// <param name="buff">Buff instance.</param>
    /// <returns>Buff's flat crit multiplier.</returns>
    public static float GetFlatCritMult(Buff buff) => Extensions.Value.TryGetValue(buff, out var extension) ? extension.FlatCritMultiplier : 0f;
    
    [HarmonyPatch(typeof(BuffManager), nameof(BuffManager.Apply))]
    [HarmonyPostfix]
    [HarmonyPriority(Priority.HigherThanNormal)]
    private static void BuffManager_Apply_Postfix(Buff buff)
    {
        RemoveBuffExtension(buff);
        if (BuffExtensionDataAsset.Data.TryGetValue(buff.id, out var data) && data.ExtraAttributes is not null)
        {
            buff.AddExtraBuffEffects(data.ExtraAttributes);
        }
        if (buff.customFields.TryGetValue(BuffCustomFieldsArmourClassKey, out var value))
        {
            buff.AddExtraBuffEffects(new ExtraBuffAttributesData { ArmourClass = float.Parse(value) });
        }
        if (buff.customFields.TryGetValue(BuffCustomFieldsFlatCritChanceKey, out value))
        {
            buff.AddExtraBuffEffects(new ExtraBuffAttributesData { FlatCriticalChance = float.Parse(value) });
        }
        if (buff.customFields.TryGetValue(BuffCustomFieldsFlatCritMultKey, out value))
        {
            buff.AddExtraBuffEffects(new ExtraBuffAttributesData { FlatCriticalMultiplier = float.Parse(value) });
        }
        var item = ItemRegistry.Create(buff.id);
        TriggerActionManager.Raise(Triggers.BuffApplied, null, Game1.player.currentLocation, Game1.player, item);
    }
    
    /// <summary>
    /// Postfix to include any buffs where extensions stats are present in custom fields, but were not handled by <see cref="SObject.TryCreateBuffsFromData"/>
    /// due to not having buff effects, so that they will be applied with <see cref="Farmer.applyBuff(Buff)"/>.
    /// </summary>
    /// <param name="values">The result buff iterator.</param>
    /// <param name="__instance">The object instance.</param>
    /// <returns>The modified buff iterator.</returns>
    [HarmonyPatch(typeof(SObject), nameof(SObject.GetFoodOrDrinkBuffs))]
    [HarmonyPostfix]
    private static IEnumerable<Buff> Object_GetFoodOrDrinkBuffs_Postfix(IEnumerable<Buff> values,
        SObject __instance)
    {
        foreach (var buff in values)
        {
            yield return buff;
        }

        if (__instance.Edibility <= -300 || !Game1.objectData.TryGetValue(__instance.ItemId, out var value))
            yield break;
        List<ObjectBuffData?>? buffs = value.Buffs;
        if (buffs is null || buffs.Count <= 0)
            yield break;
        var durationMultiplier = __instance.Quality != 0 ? 1.5f : 1f;
        
        foreach (var buffData in buffs)
        {
            if (buffData is null) continue;
            // already was handled, no need to handle again
            if (buffData.CustomAttributes is not null) continue;
            // not containing buff extensions
            if (!buffData.CustomFields.ContainsKey(BuffCustomFieldsArmourClassKey) &&
                !buffData.CustomFields.ContainsKey(BuffCustomFieldsFlatCritChanceKey) &&
                !buffData.CustomFields.ContainsKey(BuffCustomFieldsFlatCritMultKey))
            {
                continue;
            }
            // a food/drink buff with custom fields set with extensions, but no additional buff effects and thus was not handled in TryCreateBuffsFromData iterator
            var text = buffData.BuffId;
            var num = !string.IsNullOrWhiteSpace(text);
            if (!num)
            {
                text = value.IsDrink ? "drink" : "food";
            }
            Texture2D? iconTexture = null;
            var iconSheetIndex = -1;
            if (buffData.IconTexture != null)
            {
                iconTexture = Game1.content.Load<Texture2D>(buffData.IconTexture);
                iconSheetIndex = buffData.IconSpriteIndex;
            }
            var duration = -1;
            if (buffData.Duration == -2)
            {
                duration = -2;
            }
            else if (buffData.Duration != 0)
            {
                duration = (int)(buffData.Duration * durationMultiplier) * Game1.realMilliSecondsPerGameMinute;
            }
            var isDebuff = buffData.IsDebuff;
            var color = SUtility.StringToColor(buffData.GlowColor);
            var buff = new Buff(text, __instance.name, __instance.displayName, duration, iconTexture, iconSheetIndex, isDebuff: isDebuff);
            buff.customFields.TryAddMany(buffData.CustomFields);
            if (color.HasValue)
            {
                buff.glow = color.Value;
            }

            yield return buff;
        }
    }
    
    [HarmonyPatch(typeof(Buff), nameof(Buff.update))]
    [HarmonyPostfix]
    private static void Buff_update_Postfix(Buff __instance, bool __result)
    {
        if (!__result) return; 
        RemoveBuffExtension(__instance);
    }
    
    private class BuffExtension
    {
        public int ArmourClass;
        
        public float FlatCritChance;
        
        public float FlatCritMultiplier;
    }
}
