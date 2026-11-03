using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.Core.Weapons;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Events.Args;
using CombatExtended.Framework.TriggersActionsGSQ.Abilities;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.UI.Interface;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Tools;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Managers;

public class AbilityManager
{
    public static AbilityManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<AbilityManager> PerScreenInstance = new(()  => new AbilityManager());
    
    /// <summary>
    /// Used to determine how often should ability list be updated.
    /// </summary>
    private const int BaseTicks = 10;

    /// <summary>
    /// Used to store abilities tied to current weapon.
    /// </summary>
    public Dictionary<string, Ability> WeaponAbilities = new();
    
    /// <summary>
    /// Used to store previous abilities data for proper cooldown and modified ability list tracking.
    /// </summary>
    public readonly Dictionary<string, Dictionary<string, Ability>> PreviousWeaponAbilities = new();
    
    public Dictionary<string, Ability> FarmerAbilities = new();
    
    /// <summary>
    /// List of tiles that should currently be highlighted.
    /// </summary>
    private readonly List<Vector2> _tilesToHighlight = new();
    
    /// <summary>
    /// Current weapon, if any.
    /// </summary>
    private MeleeWeapon? _weapon;
    
    public readonly Dictionary<string, int> CooldownTicksOverrides = new();
    public readonly Dictionary<string, int> DurationTicksOverrides = new();
    
    /// <summary>
    /// Cache for <see cref="ContinuousAbility.DoContinuousEffectEveryTick"/>, adjusted for base ticks amount and converted to uint.
    /// </summary>
    private readonly Dictionary<string, uint> _doContinuousEffectUintTicks = new();
    
    public static void Register(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += PerScreenInstance.Value.OnUpdateTicked_UpdateAbilities;
        helper.Events.Display.RenderedWorld += PerScreenInstance.Value.OnRenderedWorld_DrawHighlightedTiles;
        helper.Events.Player.Warped += PerScreenInstance.Value.OnWarped_ClearHighlightedTiles;
        Event.OnWeaponSwitched += PerScreenInstance.Value.OnWeaponSwitched;
        Event.OnWeaponEquipped += PerScreenInstance.Value.OnWeaponEquipped;
        Event.OnWeaponUnequipped += PerScreenInstance.Value.OnWeaponUnequipped;
    }

    public static void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= PerScreenInstance.Value.OnUpdateTicked_UpdateAbilities;
        helper.Events.Display.RenderedWorld -= PerScreenInstance.Value.OnRenderedWorld_DrawHighlightedTiles;
        helper.Events.Player.Warped -= PerScreenInstance.Value.OnWarped_ClearHighlightedTiles;
        Event.OnWeaponSwitched -= PerScreenInstance.Value.OnWeaponSwitched;
        Event.OnWeaponEquipped -= PerScreenInstance.Value.OnWeaponEquipped;
        Event.OnWeaponUnequipped -= PerScreenInstance.Value.OnWeaponUnequipped;
    }

    /// <summary>
    /// Add <paramref name="ability"/> to <see cref="WeaponAbilities"/> if given <paramref name="weaponId"/> matches current weapon,
    /// or to <see cref="PreviousWeaponAbilities"/> otherwise.
    /// </summary>
    /// <param name="weaponId">ID of weapon that should receive ability.</param>
    /// <param name="ability">The ability to add.</param>
    public void AddWeaponAbility(string weaponId, Ability ability)
    {
        if (weaponId == _weapon?.ItemId)
            WeaponAbilities[ability.Id] = ability;
        else if (PreviousWeaponAbilities.TryGetValue(weaponId, out var prevAbilities))
            prevAbilities[ability.Id] = ability;
        AbilityHotbar.Hotbar().NotifyAbilitiesUpdated();
        if (ability is ContinuousAbility continuousAbility)
            _doContinuousEffectUintTicks[continuousAbility.Id] =
                Math.Max(BaseTicks, continuousAbility.DoContinuousEffectEveryTick / BaseTicks * BaseTicks);
    }

    /// <summary>
    /// Remove ability with given <paramref name="abilityId"/> from ability dictionary of weapon with <paramref name="weaponId"/>.
    /// </summary>
    /// <param name="weaponId">ID of weapon that should receive ability.</param>
    /// <param name="abilityId">ID of ability to remove.</param>
    public void RemoveWeaponAbility(string weaponId, string abilityId)
    {
        if (weaponId == _weapon?.ItemId)
            WeaponAbilities.Remove(abilityId);
        else if (PreviousWeaponAbilities.TryGetValue(weaponId, out var prevAbilities))
            prevAbilities.Remove(abilityId);
        AbilityHotbar.Hotbar().NotifyAbilitiesUpdated();
    }
    
    /// <summary>
    /// Set cooldown of ability with given <paramref name="abilityId"/>.
    /// </summary>
    /// <param name="abilityId">The ability ID.</param>
    /// <param name="cooldown">The cooldown override.</param>
    public void SetCooldown(string abilityId, float cooldown)
    {
        CooldownTicksOverrides[abilityId] = cooldown.ToTicks();
    }

    /// <summary>
    /// Restore cooldown of ability with given <paramref name="abilityId"/> to cooldown from data.
    /// </summary>
    /// <param name="abilityId">The ability point ID.</param>
    public void RestoreCooldown(string abilityId)
    {
        CooldownTicksOverrides.Remove(abilityId);
    }
    
    /// <summary>
    /// Set duration of continuous ability with given <paramref name="abilityId"/>.
    /// </summary>
    /// <param name="abilityId">The action point ID.</param>
    /// <param name="duration">The duration override.</param>
    public void SetDuration(string abilityId, float duration)
    {
        var data = AbilityDataAsset.Data[abilityId];
        if (data.Type != ContinuousAbility.ContinuousAbilityType)
        {
            Log.Error("Adding to ability duration when it is not continuous");
            return;
        }
        DurationTicksOverrides[abilityId] = duration.ToTicks();
    }
    
    /// <summary>
    /// Restore cooldown of ability with given <paramref name="abilityId"/> to cooldown from data.
    /// </summary>
    /// <param name="abilityId">The ability point ID.</param>
    public void RestoreDuration(string abilityId)
    {
        var data = AbilityDataAsset.Data[abilityId];
        if (data.Type != ContinuousAbility.ContinuousAbilityType)
        {
            Log.Error("Adding to ability duration when it is not continuous");
            return;
        }
        DurationTicksOverrides.Remove(abilityId);
    }
    
    /// <summary>
    /// Trigger given <paramref name="ability"/> by executing <see cref="Ability.Trigger"/> and raising trigger <see cref="AbilityTriggers.AbilityTriggered"/>.
    /// </summary>
    /// <param name="ability">The ability to trigger.</param>
    public void TriggerAbility(Ability ability)
    {
        if (ability.CooldownTicksElapsed < ability.CooldownTicks) return;
        var player = Game1.player;
        var playerTile = player.Tile;
        var cursorTile = Helper.Input.GetCursorPosition().Tile;
        ability.Trigger(
            player: player,
            location: player.currentLocation,
            cursorTile: cursorTile,
            playerTile: playerTile);
        RaiseAbilityTriggerAction(Triggers.AbilityTriggered, ability, player);
    }

    /// <summary>
    /// Initiate highlighting <paramref name="ability"/>'s tiles from its <see cref="Ability.GetHighlightedTiles"/>
    /// </summary>
    /// <param name="ability">The ability to highlight.</param>
    public void HighlightAbility(Ability ability)
    {
        var player = Game1.player;
        var cursorTile = Helper.Input.GetCursorPosition().Tile;
        var tiles = ability.GetHighlightedTiles(
            player,
            player.currentLocation,
            cursorTile,
            player.Tile);
        ClearHighlightedTiles();
        _tilesToHighlight.AddRange(tiles);
    }

    /// <summary>
    /// Clear list of tiles that should be highlighted.
    /// </summary>
    public void ClearHighlightedTiles()
    {
        _tilesToHighlight.Clear();
    }

    private void OnWeaponEquipped()
    {
        if (PreviousWeaponAbilities.TryGetValue(_weapon!.ItemId, out var abilities))
        {
            // weapon still has tracked data, restore it
            WeaponAbilities = abilities;
            PreviousWeaponAbilities.Remove(_weapon.ItemId);
        }
        else
        {
            var weaponAbilities = _weapon.GetAbilityDictionary();
            // weapon was not equipped yet or its cooldowns were reset
            WeaponAbilities = weaponAbilities;
            foreach (var ability in weaponAbilities.Values)
            {
                if (ability is ContinuousAbility continuousAbility)
                    _doContinuousEffectUintTicks[continuousAbility.Id] =
                        Math.Max(BaseTicks, continuousAbility.DoContinuousEffectEveryTick / BaseTicks * BaseTicks);
            }
        }
        AbilityHotbar.Hotbar().NotifyAbilitiesUpdated();
    }

    private void OnWeaponEquipped(object? sender, WeaponEquippedEventArgs e)
    {
        _weapon = e.Equipped;
        OnWeaponEquipped();
    }
    
    private void OnWeaponUnequipped(object? sender, WeaponUnequippedEventArgs e)
    {
        PreviousWeaponAbilities[e.Unequipped.ItemId] = WeaponAbilities;
        WeaponAbilities = new Dictionary<string, Ability>();
        _weapon = null;
        AbilityHotbar.Hotbar().NotifyAbilitiesUpdated();
    }
    
    private void OnWeaponSwitched(object? sender, WeaponSwitchedEventArgs e)
    {
        foreach (var continuousAbility in WeaponAbilities.Values.OfType<ContinuousAbility>())
        {
            // stop effect if ability is attached to the weapon
            if (continuousAbility.EffectAttachedToWeapon)
            {
                if (DurationTicksOverrides.TryGetValue(continuousAbility.Id, out var durationTicks))
                    continuousAbility.DurationTicksElapsed = durationTicks;
                else continuousAbility.DurationTicksElapsed = continuousAbility.DurationTicks;
            }
        }
        // save to track cooldowns
        PreviousWeaponAbilities[e.Unequipped.ItemId] = WeaponAbilities;
        _weapon = e.Equipped;
        OnWeaponEquipped();
    }

    private void OnUpdateTicked_UpdateAbilities(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree) return;
        var player = Game1.player;
        // not to cause illegal state exception
        List<(string, Ability, Farmer)> triggers = new ();
        
        foreach (var (_, ability) in WeaponAbilities)
        {
            HandleAbility(ability, player, triggers, e);
        }
        
        // update all previous weapon abilities cooldowns and trigger continuous effects
        foreach (var (_, abilities) in PreviousWeaponAbilities)
        {
            foreach (var (_, ability) in abilities)
            {
                HandleAbility(ability, player, triggers, e, true);
            }
        }
        triggers.ForEach(tr => RaiseAbilityTriggerAction(tr.Item1, tr.Item2, tr.Item3));
    }

    /// <summary>
    /// Check that given <paramref name="continuousAbility"/>'s <see cref="ContinuousAbility.DurationTicksElapsed"/>
    /// are lower than its duration in ticks, taking into account if it was overwritten.
    /// </summary>
    /// <param name="continuousAbility">The ability to</param>
    /// <returns>Whether <paramref name="continuousAbility"/> is considered active.</returns>
    private bool IsContinuousAbilityActive(ContinuousAbility continuousAbility)
    {
        if (DurationTicksOverrides.TryGetValue(continuousAbility.Id, out var durationTicks))
            return continuousAbility.DurationTicksElapsed < durationTicks;
        return continuousAbility.DurationTicksElapsed < continuousAbility.DurationTicks;
    }

    /// <summary>
    /// Check that given <paramref name="ability"/>'s <see cref="Ability.CooldownTicksElapsed"/>
    /// are lower than its cooldown in ticks, taking into account if it was overwritten.
    /// </summary>
    /// <param name="ability">The ability to</param>
    /// <returns>Whether <paramref name="ability"/>'s cooldown is considered active.</returns>
    private bool IsAbilityCooldownActive(Ability ability)
    {
        if (CooldownTicksOverrides.TryGetValue(ability.Id, out var cooldownTicks))
            return ability.CooldownTicksElapsed < cooldownTicks;
        return ability.CooldownTicksElapsed < ability.CooldownTicks;
    }

    private void HandleAbility(Ability ability, Farmer player,
        List<(string, Ability, Farmer)> triggers,
        UpdateTickedEventArgs e, bool activeWeapon = false)
    {
        if (ability is ContinuousAbility continuousAbility)
        {
            var isActive = IsContinuousAbilityActive(continuousAbility);
            if (isActive && continuousAbility.DoContinuousEffectEveryTick > 0 &&
                (!activeWeapon || !continuousAbility.EffectAttachedToWeapon) &&
                e.IsMultipleOf(_doContinuousEffectUintTicks[continuousAbility.Id]))
            {
                continuousAbility.OnContinuousTrigger(player, player.currentLocation);
            }
            // if end was not handled, handle it and start cooldown elapsing
            if (!isActive && !continuousAbility.IsEndHandled)
            {
                continuousAbility.End(player, player.currentLocation);
                triggers.Add((Triggers.AbilityEnded, continuousAbility, player));
                return;
            }
            continuousAbility.DurationTicksElapsed += BaseTicks;
            if (isActive) return;
        }
        // for continuous ability cooldown elapsing started
        if (!IsAbilityCooldownActive(ability) && !ability.IsCooldownEndHandled)
        {
            ability.EndCooldown(player, player.currentLocation);
            triggers.Add((Triggers.CooldownEnded, ability, player));
            return;
        }
        ability.CooldownTicksElapsed += BaseTicks;
    }

    /// <summary>
    /// Highlight tiles in <see cref="_tilesToHighlight"/>.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e"><see cref="RenderedWorldEventArgs"/>.</param>
    private void OnRenderedWorld_DrawHighlightedTiles(object? sender, RenderedWorldEventArgs e)
    {
        if (!_tilesToHighlight.Any() || !Context.IsPlayerFree) return;
        var b = e.SpriteBatch;
        _tilesToHighlight.ForEach(tile =>
        {
            var screenPosition = new Vector2(tile.X * Game1.tileSize - Game1.viewport.X, tile.Y * Game1.tileSize - Game1.viewport.Y);
            var rect = new Rectangle((int)screenPosition.X, (int)screenPosition.Y, Game1.tileSize, Game1.tileSize);
            b.Draw(Game1.staminaRect, rect, Color.Red * 0.25f);
        });
    }

    /// <summary>
    /// Clear all highlighted tiles whenever current player warps.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e"><see cref="WarpedEventArgs"/>.</param>
    private void OnWarped_ClearHighlightedTiles(object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer) return;
        ClearHighlightedTiles();
    }

    /// <summary>
    /// Raise given <paramref name="ability"/> related <paramref name="trigger"/> for <paramref name="player"/>.
    /// </summary>
    /// <param name="trigger">The trigger to raise.</param>
    /// <param name="ability">The ability whose info to use.</param>
    /// <param name="player">The player for whom trigger is raised.</param>
    private static void RaiseAbilityTriggerAction(string trigger, Ability ability, Farmer player)
    {
        var playerTile = player.Tile;
        var cursorTile = Helper.Input.GetCursorPosition().Tile;
        var triggerArgs = new object[] { ability, cursorTile, playerTile };
        var item = ItemRegistry.Create("(O)168");
        item.PackAbilityInfo(ability);
        
        TriggerActionManager.Raise(trigger, triggerArgs, player.currentLocation, player, item);
    }
}
