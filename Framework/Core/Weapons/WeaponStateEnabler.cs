using CombatExtended.Framework.Data;
using CombatExtended.Framework.Events.Args;
using CombatExtended.Framework.Util;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Tools;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Core.Weapons;

[Registrable]
[HarmonyPatch]
public static class WeaponStateEnabler
{

    private static readonly PerScreen<Dictionary<string, string>> PerScreenCurrentStates = 
        new (() => new Dictionary<string, string>());
    private static Dictionary<string, string> CurrentStates => PerScreenCurrentStates.Value;

    private static readonly PerScreen<Dictionary<string, Texture2D>> PerScreenCachedOverrideTextures = 
        new (() => new Dictionary<string, Texture2D>());
    private static Dictionary<string, Texture2D> CachedOverrideTextures => PerScreenCachedOverrideTextures.Value;
    
    public static void Register(IModHelper helper)
    {
        Event.OnWeaponEquipped += OnWeaponEquipped_RaiseActions;
        Event.OnWeaponUnequipped += OnWeaponUnequipped_RaiseActions;
        Event.OnWeaponSwitched += OnWeaponSwitched_RaiseActions;
        helper.Events.Player.InventoryChanged += OnInventoryChanged_ApplyOrRestoreWeaponStates;
        helper.Events.GameLoop.DayEnding += (_, _) => { CleanUp(); };
        helper.Events.GameLoop.ReturnedToTitle += (_, _) => { CleanUp(); };
    }

    public static string GetWeaponState(string weaponId) => CurrentStates.GetValueOrDefault(weaponId, "_default");
    
    /// <summary>
    /// Applies present weapon stats and texture from <paramref name="stateData"/> to given <paramref name="weapon"/>.
    /// </summary>
    /// <param name="weapon">Weapon whose state should change.</param>
    /// <param name="stateData">Weapon state data.</param>
    public static void ApplyState(MeleeWeapon weapon, WeaponStateData stateData)
    {
        Update(weapon.ItemId, stateData.Id);
        if (stateData.Texture is not null)
            CachedOverrideTextures[weapon.ItemId] = Helper.GameContent.Load<Texture2D>(stateData.Texture);
        if (stateData.MinDamage is not null)
            weapon.minDamage.Value = stateData.MinDamage.Value;
        if (stateData.MaxDamage is not null)
            weapon.maxDamage.Value = stateData.MaxDamage.Value;
        if (stateData.Knockback is not null)
            weapon.knockback.Value = stateData.Knockback.Value;
        if (stateData.Speed is not null)
            weapon.speed.Value = stateData.Speed.Value;
        if (stateData.Precision is not null)
            weapon.addedPrecision.Value = stateData.Precision.Value;
        if (stateData.Defense is not null)
            weapon.addedDefense.Value = stateData.Defense.Value;
        if (stateData.AreaOfEffect is not null)
            weapon.addedAreaOfEffect.Value = stateData.AreaOfEffect.Value;
        if (stateData.CritChance is not null)
            weapon.critChance.Value = stateData.CritChance.Value;
        if (stateData.CritMultiplier is not null)
            weapon.critMultiplier.Value = stateData.CritMultiplier.Value;
    }

    /// <summary>
    /// Restore <paramref name="weapon"/> default state by applying corresponding data from Data/Weapons.
    /// </summary>
    /// <param name="weapon">Weapon whose state should be restored.</param>
    public static void RestoreToDefault(MeleeWeapon weapon)
    {
        Remove(weapon.ItemId);
        var data = Game1.weaponData[weapon.ItemId];
        weapon.minDamage.Value = data.MinDamage;
        weapon.maxDamage.Value = data.MaxDamage;
        weapon.knockback.Value = data.Knockback;
        weapon.speed.Value = data.Speed;
        weapon.addedPrecision.Value = data.Precision;
        weapon.addedDefense.Value = data.Defense;
        weapon.addedAreaOfEffect.Value = data.AreaOfEffect;
        weapon.critChance.Value = data.CritChance;
        weapon.critMultiplier.Value = data.CritMultiplier;
    }
    
    public static List<DamageTypeWeaponExtensionData> GetExtraDamageTypesWithState(string weaponId)
    {
        if (!CurrentStates.ContainsKey(weaponId)) return WeaponExtensionDataAsset.GetDefaultExtraDamageTypes(weaponId);
        var result = new List<DamageTypeWeaponExtensionData>();
        var stateId = CurrentStates[weaponId];
        var stateData = GetWeaponStateData(weaponId, stateId);
        stateData.DamageTypes?.ForEach(damageType =>
        {
            if (DamageTypeDataAsset.Exists(damageType.Type)) result.Add(damageType);
            else Log.Warn($"Unknown damage type {damageType.Type} for weapon extension on {weaponId} and state {stateId}");
        });
        return result;
    }

    /// <summary>
    /// Get state data for given <paramref name="stateId"/> and weapon with given <paramref name="weaponId"/>.
    /// </summary>
    /// <param name="weaponId">The weapon ID.</param>
    /// <param name="stateId">The weapon state ID.</param>
    /// <returns><see cref="WeaponStateData"/> instance.</returns>
    /// <exception cref="ArgumentException">Unknown weapon or weapon state ID.</exception>
    public static WeaponStateData GetWeaponStateData(string weaponId, string stateId)
    {
        if (!WeaponExtensionDataAsset.WeaponStates.TryGetValue(weaponId, out var states))
            throw new ArgumentException($"Weapon {weaponId} has no registered states.");
        if (!states.TryGetValue(stateId, out var stateData))
            throw new ArgumentException($"State {stateId} is not registered on weapon {weaponId}.");
        return stateData;
    }

    private static void Update(string weaponId, string stateId)
    {
        CurrentStates[weaponId] = stateId;
    }

    private static void Remove(string weaponId)
    {
        CurrentStates.Remove(weaponId);
        CachedOverrideTextures.Remove(weaponId);
    }

    private static void OnWeaponEquipped_RaiseActions(object? sender, WeaponEquippedEventArgs e)
    {
        if (WeaponExtensionDataAsset.Data.TryGetValue(e.Equipped.ItemId, out var data))
            data.ActionsOnEquip.ForEach(action => TriggerActionManager.Raise(action));
    }
    
    private static void OnWeaponUnequipped_RaiseActions(object? sender, WeaponUnequippedEventArgs e)
    {
        if (WeaponExtensionDataAsset.Data.TryGetValue(e.Unequipped.ItemId, out var data))
            data.ActionsOnUnequip.ForEach(action => TriggerActionManager.Raise(action));
    }

    private static void OnWeaponSwitched_RaiseActions(object? sender, WeaponSwitchedEventArgs e)
    {
        if (WeaponExtensionDataAsset.Data.TryGetValue(e.Equipped.ItemId, out var data))
            data.ActionsOnEquip.ForEach(action => TriggerActionManager.Raise(action));
        if (WeaponExtensionDataAsset.Data.TryGetValue(e.Unequipped.ItemId, out data))
            data.ActionsOnUnequip.ForEach(action => TriggerActionManager.Raise(action));
    }

    /// <summary>
    /// Apply current state to items added to inventory and restore back default state to removed items.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e"><see cref="InventoryChangedEventArgs"/>.</param>
    private static void OnInventoryChanged_ApplyOrRestoreWeaponStates(object? sender, InventoryChangedEventArgs e)
    {
        foreach (var weapon in e.Added.OfType<MeleeWeapon>()
                     .Where(weapon => CurrentStates.ContainsKey(weapon.ItemId)))
        { 
            var stateId = CurrentStates[weapon.ItemId];
            var stateData = GetWeaponStateData(weapon.ItemId, stateId);
            ApplyState(weapon, stateData);
        }

        foreach (var weapon in e.Removed.OfType<MeleeWeapon>()
                     .Where(weapon => CurrentStates.ContainsKey(weapon.ItemId)))
        {
            RestoreToDefault(weapon);
        }
    }
    
    /// <summary>
    /// Clear current state data and cached state textures.
    /// </summary>
    private static void CleanUp()
    {
        CurrentStates.Clear();
        CachedOverrideTextures.Clear();
    }

    #region DisplayPatches
    /// <summary>
    /// If <paramref name="__instance"/> is weapon and its current state overrides sprite index, replace argument of sprite index in original method.
    /// </summary>
    /// <param name="__instance"><see cref="ParsedItemData"/> instance.</param>
    /// <param name="spriteIndex">Sprite index to be used in original method.</param>
    [HarmonyPatch(typeof(ParsedItemData), nameof(ParsedItemData.GetSourceRect))]
    [HarmonyPrefix]
    private static void ParsedItemData_GetSourceRect_Prefix(ParsedItemData __instance, ref int? spriteIndex)
    {
        if (__instance.ItemType.Identifier != ItemRegistry.type_weapon) return;
        if (!CurrentStates.TryGetValue(__instance.ItemId, out var stateId)) return;
        var stateData = GetWeaponStateData(__instance.ItemId, stateId);
        spriteIndex = stateData.SpriteIndex;
    }

    /// <summary>
    /// If <paramref name="__instance"/> is weapon and its current state overrides texture, replace its texture.
    /// </summary>
    /// <param name="__instance"><see cref="ParsedItemData"/> instance.</param>
    /// <param name="__result">The result item texture.</param>
    /// <returns>If original method should be executed.</returns>
    [HarmonyPatch(typeof(ParsedItemData), nameof(ParsedItemData.GetTexture))]
    [HarmonyPrefix]
    private static bool ParsedItemData_GetTexture_Prefix(ParsedItemData __instance, ref Texture2D __result)
    {
        if (__instance.ItemType.Identifier != ItemRegistry.type_weapon) return true;
        if (CachedOverrideTextures.TryGetValue(__instance.ItemId, out var cachedTexture))
        {
            __result = cachedTexture;
            return false;
        }

        return true;
    }
    
    /// <summary>
    /// If <paramref name="__instance"/> is weapon and its current state overrides texture, replace name of it.
    /// </summary>
    /// <param name="__instance"><see cref="ParsedItemData"/> instance.</param>
    /// <param name="__result">The result item texture name.</param>
    /// <returns>If original method should be executed.</returns>
    [HarmonyPatch(typeof(ParsedItemData), nameof(ParsedItemData.GetTextureName))]
    [HarmonyPrefix]
    private static bool ParsedItemData_GetTextureName_Prefix(ParsedItemData __instance, ref string __result)
    {
        if (__instance.ItemType.Identifier != ItemRegistry.type_weapon) return true;
        if (CachedOverrideTextures.TryGetValue(__instance.ItemId, out var cachedTexture))
        {
            __result = cachedTexture.Name;
            return false;
        }

        return true;
    }
    #endregion
}
