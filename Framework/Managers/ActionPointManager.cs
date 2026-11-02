using CombatExtended.Framework.Core.Misc.ActionPoints;
using CombatExtended.Framework.Core.Weapons;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Events.Args;
using CombatExtended.Framework.UI.Interface;
using CombatExtended.Framework.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace CombatExtended.Framework.Managers;

public class ActionPointManager
{
    public static ActionPointManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<ActionPointManager> PerScreenInstance = new(()  => new ActionPointManager());
    
    /// <summary>
    /// Used to determine how often should action point replenish time be handled.
    /// </summary>
    private const int BaseTicks = 1;
    
    /// <summary>
    /// Used to store all active action points.
    /// </summary>
    public Dictionary<string, ActionPoint> ActionPoints { get; } = new ();
    
    private readonly Dictionary<string, int> _savedActionPointAmounts = new();
    
    private readonly Dictionary<string, int> _replenishTimeTicksOverrides = new ();
    private readonly Dictionary<string, int> _maxAmountOverrides = new ();

    public static void Register(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += PerScreenInstance.Value.OnUpdateTicked_HandleActionPointReplenish;
        Event.OnWeaponSwitched += PerScreenInstance.Value.OnWeaponSwitched_EnableActionPoints;
        Event.OnWeaponEquipped += PerScreenInstance.Value.OnWeaponEquipped_EnableActionPoints;
        Event.OnWeaponUnequipped += PerScreenInstance.Value.OnWeaponUnequipped_EnableActionPoints;
    }

    public static void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= PerScreenInstance.Value.OnUpdateTicked_HandleActionPointReplenish;
        Event.OnWeaponSwitched -= PerScreenInstance.Value.OnWeaponSwitched_EnableActionPoints;
        Event.OnWeaponEquipped -= PerScreenInstance.Value.OnWeaponEquipped_EnableActionPoints;
        Event.OnWeaponUnequipped -= PerScreenInstance.Value.OnWeaponUnequipped_EnableActionPoints;
    }

    /// <summary>
    /// Add directly given <paramref name="amount"/> to action points with given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The action point ID.</param>
    /// <param name="amount">The amount to add.</param>
    public void AddActionPoint(string id, int amount)
    {
        if (!ActionPoints.TryGetValue(id, out var actionPoint)) return;
        if (amount >= 0) actionPoint.Add(amount);
        else actionPoint.Spend(Math.Abs(amount));
    }

    /// <summary>
    /// Get current action points for given <paramref name="id"/>, or <c>0</c> if it is not being handled.
    /// </summary>
    /// <param name="id">The action point ID</param>
    /// <returns>The amount of action points.</returns>
    public int GetActionPointAmount(string id)
    {
        if (ActionPoints.TryGetValue(id, out var actionPoint))
            return actionPoint.CurrentAmount;
        return 0;
    }

    /// <summary>
    /// Enable action point to start showing and handling it.
    /// </summary>
    /// <param name="id">The action point ID</param>
    /// <param name="overrideAmount">The amount of points that should override amount from data.</param>
    public void EnableActionPoint(string id, int overrideAmount = -1)
    {
        var actionPoint = ActionPoint.Create(id);
        // if max amount overriden while action point was not enabled
        if (_maxAmountOverrides.TryGetValue(id, out var maxAmount))
        {
            actionPoint.MaxAmount = maxAmount;
            _maxAmountOverrides.Remove(id);
        }
        // restore saved previous amount if present
        if (_savedActionPointAmounts.TryGetValue(id, out var amount))
        {
            actionPoint.CurrentAmount = amount;
            _savedActionPointAmounts.Remove(Id);
        }
        if (overrideAmount >= 0)
            actionPoint.CurrentAmount = overrideAmount;
        ActionPoints[id] = actionPoint;
    }
    
    /// <summary>
    /// Disable action point to hide it and stop handling.
    /// </summary>
    /// <param name="id">The action point ID.</param>
    public void DisableActionPoint(string id)
    {
        if (ActionPoints.Remove(id, out var removed)) 
            _savedActionPointAmounts[id] = removed.CurrentAmount;
    }
    
    /// <summary>
    /// Set replenish time of action point with given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The action point ID.</param>
    /// <param name="replenishTime">The replenish time override.</param>
    public void SetReplenishTime(string id, float replenishTime)
    {
        _replenishTimeTicksOverrides[id] = replenishTime.ToTicks();
    }

    public void SetMaxAmount(string id, int maxAmount)
    {
        if (ActionPoints.TryGetValue(id, out var actionPoint))
            actionPoint.MaxAmount = maxAmount;
        else _maxAmountOverrides[id] = maxAmount;
    }

    public void RestoreMaxAmount(string id)
    {
        _maxAmountOverrides.Remove(id);
        if (ActionPoints.TryGetValue(id, out var actionPoint))
            actionPoint.MaxAmount = ActionPointDataAsset.Data[id].MaxAmount;
    }

    /// <summary>
    /// Restore replenish time of action point with given <paramref name="id"/> to replenish time from data.
    /// </summary>
    /// <param name="id">The action point ID.</param>
    public void RestoreReplenishTime(string id)
    {
        _replenishTimeTicksOverrides.Remove(id);
    }

    private void OnWeaponEquipped_EnableActionPoints(object? sender, WeaponEquippedEventArgs e)
    {
        e.Equipped.GetActionPoints().ForEach(ap => EnableActionPoint(ap.Id));
        ActionPointIcons.Icons().NotifyActionPointsUpdated();
    }
    
    private void OnWeaponUnequipped_EnableActionPoints(object? sender, WeaponUnequippedEventArgs e)
    {
        e.Unequipped.GetActionPoints().ForEach(ap => DisableActionPoint(ap.Id));
        ActionPointIcons.Icons().NotifyActionPointsUpdated();
    }
    
    private void OnWeaponSwitched_EnableActionPoints(object? sender, WeaponSwitchedEventArgs e)
    {
        e.Equipped.GetActionPoints().ForEach(ap => EnableActionPoint(ap.Id));
        e.Unequipped.GetActionPoints().ForEach(ap => DisableActionPoint(ap.Id));
        ActionPointIcons.Icons().NotifyActionPointsUpdated();
    }

    /// <summary>
    /// Handle replenishing of all action points that have their replenish time defined or modified.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e"><see cref="UpdateTickedEventArgs"/>.</param>
    private void OnUpdateTicked_HandleActionPointReplenish(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree) return;
        
        foreach (var (_, ap) in ActionPoints)
        {
            // should not be replenished
            if (ap.ReplenishTimeTicks <= 0) continue;
            if (_replenishTimeTicksOverrides.TryGetValue(ap.Id, out var replenishTimeTicks))
            {
                // should not be replenished
                if (replenishTimeTicks <= 0) continue;
                if (ap.ReplenishTicksElapsed >= replenishTimeTicks)
                    ap.Replenish();
            } else if (ap.ReplenishTicksElapsed >= ap.ReplenishTimeTicks)
                ap.Replenish();
            else ap.ReplenishTicksElapsed += BaseTicks;
        }
    }
}
