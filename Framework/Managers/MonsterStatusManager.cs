using System.Runtime.CompilerServices;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Monsters.Statuses;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Managers;

public class MonsterStatusManager
{
    /// <summary>
    /// How often should monsters status dictionary be updated.
    /// </summary>
    private const int BaseTicks = 10;
    
    /// <summary>
    /// How often should status conditions be checked.
    /// </summary>
    private const int ConditionCheckTicks = 30;
    
    /// <summary>
    /// Single source of truth for all monster buffs.
    /// </summary>
    private readonly ConditionalWeakTable<Monster, Dictionary<string, MonsterStatus>> _statuses = new();
    
    /// <summary>
    /// Cache for <see cref="MonsterStatus.DoEffectEveryTick"/>, adjusted for base ticks amount and converted to uint.
    /// </summary>
    private readonly Dictionary<string, int> _doEffectAdjustedTicks = new();
    
    public void Register(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked_CheckStatuses;
    }
    
    public void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked_CheckStatuses;
    }

    private void OnUpdateTicked_CheckStatuses(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree || !Context.IsMainPlayer) return;
        // handle status effects
        foreach (var (monster, statuses) in _statuses)
        {
            foreach (var (id, status) in statuses)
            {
                status.TicksElapsed += BaseTicks;
                if (status.TicksElapsed % _doEffectAdjustedTicks[id] == 0)
                    status.DoEffect(monster, monster.currentLocation);
            }
        }
        
        if (!e.IsMultipleOf(ConditionCheckTicks)) return;
        
        // check status apply and removal conditions
        foreach (var loc in Helper.Multiplayer.GetActiveLocations())
        {
            foreach (var monster in loc.characters.OfType<Monster>())
            {
                var monsterStatuses = _statuses.GetValue(monster, _ => new Dictionary<string, MonsterStatus>());
                var allStatuses = monster.GetDataMonsterStatuses();
                var item = ItemRegistry.Create("(O)168");
                item.PackMonsterInfo(monster);
                foreach (var (id, statusData) in allStatuses)
                {
                    var conditionCheck = GameStateQuery.CheckConditions(statusData.Condition,
                        location: monster.currentLocation, player: Game1.player, targetItem: item);
                    var hasStatus = monsterStatuses.ContainsKey(id);
                    if (conditionCheck && !hasStatus)
                    {
                        var status = MonsterStatus.Create(id);
                        monsterStatuses.Add(id, status);
                        status.Apply(monster, monster.currentLocation);
                        _doEffectAdjustedTicks[id] = Math.Max(BaseTicks, status.DoEffectEveryTick / BaseTicks * BaseTicks);
                    }
                    else if (!conditionCheck && hasStatus)
                    {
                        if (monsterStatuses.Remove(id, out var status)) 
                            status.Remove(monster, monster.currentLocation);
                    }
                }
            }
        }
    }
}
