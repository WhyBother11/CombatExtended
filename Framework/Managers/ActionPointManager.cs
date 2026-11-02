using CombatExtended.Framework.Core.Misc.ActionPoints;
using CombatExtended.Framework.Core.Weapons;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.UI.Interface;
using CombatExtended.Framework.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Tools;
using StardewValley.Triggers;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.Managers;

public class ActionPointManager
{
    public static ActionPointManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<ActionPointManager> PerScreenInstance = new(()  => new ActionPointManager());
    
    /// <summary>
    /// Used to determine how often should action point replenish time be handled.
    /// </summary>
    private const int BaseTicks = 10;
    
    /// <summary>
    /// Used to store all active action points.
    /// </summary>
    public Dictionary<string, ActionPoint> ActionPoints { get; } = new ();
    
    private readonly List<ActionPoint> _actionPointsToEnable = new ();
    private readonly List<string> _actionPointsToDisable = new ();
    
    private readonly Dictionary<string, int> _savedActionPointAmounts = new();
    
    private readonly Dictionary<string, int> _replenishTimeTicksOverrides = new ();
    private readonly Dictionary<string, int> _maxAmountOverrides = new ();

    private MeleeWeapon? _weapon;

    public static void Register(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += PerScreenInstance.Value.OnUpdateTicked_HandleActionPointReplenish;
    }

    public static void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= PerScreenInstance.Value.OnUpdateTicked_HandleActionPointReplenish;
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
        _actionPointsToEnable.Add(actionPoint);
    }
    
    /// <summary>
    /// Disable action point to hide it and stop handling.
    /// </summary>
    /// <param name="id">The action point ID.</param>
    public void DisableActionPoint(string id)
    {
        _actionPointsToDisable.Add(id);
    }

    /// <summary>
    /// Modify replenish time of action point with given <paramref name="id"/> by <paramref name="modifier"/>.
    /// </summary>
    /// <param name="id">The action point ID.</param>
    /// <param name="modifier">The replenish time modifier.</param>
    public void ModifyReplenishTime(string id, float modifier)
    {
        if (_replenishTimeTicksOverrides.TryGetValue(id, out var replenishTimeTicks))
        {
            _replenishTimeTicksOverrides[id] = (int)(replenishTimeTicks * modifier);
        }
        else _replenishTimeTicksOverrides[id] =
            (int) (ActionPointDataAsset.Data[id].ReplenishTime.ToTicks() * modifier);
    }
    
    /// <summary>
    /// Add to or subtract given <paramref name="delta"/> from replenish time of action point with given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The action point ID.</param>
    /// <param name="delta">The replenish time delta.</param>
    public void AddReplenishTime(string id, float delta)
    {
        if (_replenishTimeTicksOverrides.TryGetValue(id, out var replenishTimeTicks))
        {
            // not to set non-replenishable if delta is negative and too big
            _replenishTimeTicksOverrides[id] = Math.Max(1, replenishTimeTicks + delta.ToTicks());
        }
        else _replenishTimeTicksOverrides[id] =
             ActionPointDataAsset.Data[id].ReplenishTime.ToTicks() + delta.ToTicks();
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

    /// <summary>
    /// Handle replenishing of all action points that have their replenish time defined or modified.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e"><see cref="UpdateTickedEventArgs"/>.</param>
    private void OnUpdateTicked_HandleActionPointReplenish(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree) return;
        
        var weapon = Utility.CurrentWeapon();
        var changedWeapon = weapon is not null && _weapon is not null && weapon.ItemId != _weapon.ItemId;
        var shouldNotify = false;
        if (weapon is null && _weapon is not null || changedWeapon)
        {
            // if weapon is unequipped or changed, remove previous weapon's action points
            _weapon!.GetActionPoints().ForEach(ap => DisableActionPoint(ap.Id));
            // todo: ok enough but should really move to separate manager and think how ability manager should be handled in that case
            if (WeaponExtensionDataAsset.Data.TryGetValue(_weapon!.ItemId, out var data))
                data.ActionsOnUnequip.ForEach(action => TriggerActionManager.Raise(action));
            _weapon = null;
            shouldNotify = true;
            //ActionPointIcons.Icons().NotifyActionPointsUpdated();
        } else if (weapon is not null && _weapon is null || changedWeapon)
        {
            // if weapon is equipped or changed, add current weapon's action points
            weapon!.GetActionPoints().ForEach(ap => EnableActionPoint(ap.Id));
            if (WeaponExtensionDataAsset.Data.TryGetValue(_weapon!.ItemId, out var data))
                data.ActionsOnEquip.ForEach(action => TriggerActionManager.Raise(action));
            _weapon = weapon;
            shouldNotify = true;
            //ActionPointIcons.Icons().NotifyActionPointsUpdated();
        }
        
        if (_actionPointsToEnable.Count > 0)
        {
            _actionPointsToEnable.ForEach(ap => ActionPoints[ap.Id] = ap);
            _actionPointsToEnable.Clear();
        }
        if (_actionPointsToDisable.Count > 0)
        {
            _actionPointsToDisable.ForEach(id =>
            {
                // save current amount to restore when added back
                if (ActionPoints.Remove(id, out var removed)) 
                    _savedActionPointAmounts[id] = removed.CurrentAmount;
            });
            _actionPointsToDisable.Clear();
        }
        if (shouldNotify) 
            ActionPointIcons.Icons().NotifyActionPointsUpdated();
        
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
