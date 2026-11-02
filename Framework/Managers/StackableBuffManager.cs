using CombatExtended.Framework.Data;
using CombatExtended.Framework.UI.Vanilla;
using CombatExtended.Framework.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace CombatExtended.Framework.Managers;

public class StackableBuffManager
{
    public static StackableBuffManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<StackableBuffManager> PerScreenInstance = new(()  => new StackableBuffManager());

    private const int BaseTicks = 20;
    
    private readonly Dictionary<string, int> _buffStacks = new ();
    
    private readonly Dictionary<string, int> _buffMaxStacks = new ();
    
    private readonly Dictionary<string, LinkedList<int>> _buffTimers = new ();
    
    private readonly Dictionary<string, int> _buffTimerAmountsToBeRemoved = new();
    
    private readonly Dictionary<string, int> _buffTimerAmountsToBeAdded = new();

    public static void Register(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += PerScreenInstance.Value.OnUpdateTicked_HandleIndependentBuffStacks;
    }
    
    public static void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= PerScreenInstance.Value.OnUpdateTicked_HandleIndependentBuffStacks;
    }

    /// <summary>
    /// Try to register added stack to a buff if stack amount is lower than max stacks.
    /// </summary>
    /// <param name="buffId">ID of buff</param>
    public void RegisterAddedStack(string buffId)
    {
        var currentStacks = GetStacks(buffId);
        var isIndependentStacking = BuffExtensionDataAsset.Data[buffId].StackableData?.IndependentStacks ?? false;
        if (currentStacks >= GetMaxStacks(buffId))
        {
            if (!isIndependentStacking) return;
            
            if (!_buffTimerAmountsToBeAdded.TryAdd(buffId, 1))
                _buffTimerAmountsToBeAdded[buffId] += 1;
            
            if (!_buffTimerAmountsToBeRemoved.TryAdd(buffId, 1))
                _buffTimerAmountsToBeRemoved[buffId] += 1;
            return;
        }
        if (currentStacks == 0)
        {
            BuffStackDisplay.StartTracking(buffId, () => GetStacks(buffId));
            if (isIndependentStacking) 
                _buffTimers[buffId] = new LinkedList<int>();
        }
        _buffStacks[buffId] = currentStacks + 1;
        
        if (isIndependentStacking)
        {
            if (!_buffTimerAmountsToBeAdded.TryAdd(buffId, 1))
                _buffTimerAmountsToBeAdded[buffId] += 1;
        }
    }

    /// <summary>
    /// Register that buff with given <paramref name="buffId"/> expired.
    /// </summary>
    /// <param name="buffId">ID of expired buff.</param>
    public void RegisterBuffExpiration(string buffId)
    {
        _buffStacks.Remove(buffId);
        _buffMaxStacks.Remove(buffId);
        _buffTimers.Remove(buffId);
        BuffStackDisplay.StopTracking(buffId);
    }
    
    /// <summary>
    /// Get current stacks of buff with given <paramref name="buffId"/>.
    /// <remarks>If buff is not stackable, <c>1</c> is returned if buff is present, <c>0</c> otherwise</remarks>.
    /// </summary>
    /// <param name="buffId">Buff ID.</param>
    /// <returns>Buff stack amount.</returns>
    public int GetStacks(string buffId) => _buffStacks.GetValueOrDefault(
        buffId, BuffExtensionDataAsset.Data[buffId].StackableData is not null ? 0 :
        (Game1.player.hasBuff(buffId) ? 1 : 0));

    /// <summary>
    /// Get max stacks of buff with given <paramref name="buffId"/>, accounting for any changes to its max stacks.
    /// </summary>
    /// <remarks>For non-stackable buffs <c>1</c> is returned.</remarks>
    /// <param name="buffId">Buff ID.</param>
    /// <returns>Buff max stack amount.</returns>
    public int GetMaxStacks(string buffId) => _buffMaxStacks.GetValueOrDefault(buffId,
        BuffExtensionDataAsset.Data[buffId].StackableData?.MaxStacks ?? 1);

    /// <summary>
    /// Dynamically change max stack amount for buff with given <paramref name="buffId"/>
    /// </summary>
    /// <param name="buffId">Buff ID.</param>
    /// <param name="newLimit">New max stack amount for buff.</param>
    public void ChangeStackLimit(string buffId, int newLimit)
    {
        _buffMaxStacks[buffId] = newLimit;
        if (_buffStacks.TryGetValue(buffId, out var stacks))
            _buffStacks[buffId] = Math.Min(stacks, newLimit);
    }

    /// <summary>
    /// Iterate over buff timers, decrement them and update stack amount for independently stackable buffs.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">See <see cref="UpdateTickedEventArgs"/>.</param>
    private void OnUpdateTicked_HandleIndependentBuffStacks(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree) return;
        if (_buffTimerAmountsToBeAdded.Count > 0)
        {
            var buffData = DataLoader.Buffs(Game1.content);
            foreach (var (buffId, amount) in _buffTimerAmountsToBeRemoved)
            {
                var buff = buffData[buffId];
                if (buff.Duration == -2) continue;
                for (var i = 0; i < amount; i++)
                {
                    _buffTimers[buffId].RemoveFirst();
                }
            }
            foreach (var (buffId, amount) in _buffTimerAmountsToBeAdded)
            {
                var buff = buffData[buffId];
                if (buff.Duration == -2) continue;
                for (var i = 0; i < amount; i++)
                {
                    _buffTimers[buffId].AddLast(buff.Duration.MillisToTicks());
                }
            }
            _buffTimerAmountsToBeAdded.Clear();
            _buffTimerAmountsToBeRemoved.Clear();
        }
        foreach (var (buffId, timers) in _buffTimers)
        {
            var stackAmount = 0;
            // why no Int for reference type smh
            for (var node = timers.First; node is not null; node = node.Next)
            {
                node.Value -= BaseTicks;
                if (node.Value > 0) stackAmount += 1;
            }
            // due to rounding or not quite accurate first 20 tick deduction it may display 0 stacks for a little which is awkward so max is used
            _buffStacks[buffId] = Math.Max(1, stackAmount);
        }
    }
}
