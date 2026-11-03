using System.Collections.Immutable;
using CombatExtended.Framework.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

namespace CombatExtended.Framework.Managers;

/// <summary>
/// Manager to handle various cooldowns not tied to anything in particular.
/// </summary>
public class CooldownManager
{
    public static CooldownManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<CooldownManager> PerScreenInstance = new(()  => new CooldownManager());
    
    private const int BaseTicks = 15;

    private readonly Dictionary<string, int> _timers = new ();
    public ImmutableDictionary<string, int> Timers => _timers.ToImmutableDictionary();
    
    public static void Register(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += PerScreenInstance.Value.OnUpdateTicked_HandleTimers;
    }
    
    public static void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= PerScreenInstance.Value.OnUpdateTicked_HandleTimers;
    }

    public void Add(string id, float timeSeconds)
    {
        _timers.TryAdd(id, timeSeconds.ToTicks());
    }

    private void OnUpdateTicked_HandleTimers(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree) return;
        var toRemove = new List<string>();
        foreach (var (id, time) in _timers)
        {
            if (time <= 0) toRemove.Add(id);
            else _timers[id] -= BaseTicks;
        }
        toRemove.ForEach(id => _timers.Remove(id));
    }
}
