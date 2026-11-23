using System.Diagnostics.CodeAnalysis;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;
using SequenceOrchestratorManager = CombatExtended.Framework.Managers.SequenceOrchestratorManager;

namespace CombatExtended.Framework.TriggersActionsGSQ.Sequence;

/// <summary>
/// The handler for sequences that orchestrates correct execution of given trigger actions on their completion.
/// </summary>
public class SequenceOrchestrator
{
    
    /// <summary>
    /// ID of this sequence orchestrator.
    /// </summary>
    public int Id { get; }
    
    /// <summary>
    /// The list of trigger actions that should be executed.
    /// </summary>
    private readonly List<string> _actions;

    /// <summary>
    /// The cursor pointing at the current trigger action.
    /// </summary>
    private int _cursor;
    
    /// <summary>
    /// <see cref="TriggerActionContext"/> that should be given to any underlying trigger action.
    /// </summary>
    private readonly TriggerActionContext _context;

    public SequenceOrchestrator(int id, List<string> actions, TriggerActionContext context)
    {
        _actions = actions;
        Id = id;
        var customFields = (context.CustomFields ?? new Dictionary<string, object>()).Append(
            new KeyValuePair<string, object>("orchestratorId".PrependId(), Id)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _context = new TriggerActionContext(context.Trigger, context.TriggerArgs, context.Data, customFields);
    }

    /// <summary>
    /// Start executing underlying trigger actions.
    /// </summary>
    public void Start()
    {
        ExecuteCurrent();
    }
    
    /// <summary>
    /// Execute an action that the cursor is pointing to.
    /// </summary>
    /// <param name="error">The error if ran unsuccessfully.</param>
    /// <param name="exception">The exception if ran unsuccessfully.</param>
    /// <returns><c>true</c> if action is performed successfully or there are no more actions, <c>false</c> otherwise.</returns>
    private bool ExecuteCurrent([NotNullWhen(false)]out string? error, [NotNullWhen(false)]out Exception? exception)
    {
        if (_cursor >= _actions.Count)
        {
            error = null;
            exception = null;
            return true;
        }
        if (!TriggerActionManager.TryRunAction(TriggerActionManager.ParseAction(_actions[_cursor]), _context,
                out error, out exception))
        {
            return false;
        }
        error = null;
        exception = null;
        return true;
    }

    /// <summary>
    /// Used by trigger actions to notify orchestrator when they are ended. To do so, the trigger action delegate should
    /// have <see cref="SequenceAware"/> attribute and MUST call this method to proceed to the next actions.
    /// </summary>
    public void NotifyEnded()
    {
        _cursor++;
        ExecuteCurrent();
    }

    /// <summary>
    /// Execute an action that the cursor is pointing to. Then, if run successfully, check that sequence is not ended and
    /// then run next action if it has not <see cref="SequenceAware"/>, otherwise not do anything and wait for its completion.
    /// </summary>
    private void ExecuteCurrent()
    {
        while (true)
        {
            if (!ExecuteCurrent(out var error, out var ex))
            {
                Log.Error($"Failed to run action {_actions[_cursor]}, error: {error}", ex);
                return;
            }
            if (_cursor >= _actions.Count)
                return;

            var action = ArgUtility.SplitBySpaceQuoteAware(_actions[_cursor])[0];
            if (SequenceOrchestratorManager.SequenceAwareTriggerActions.Contains(action))
                return;
            _cursor++;
        }
    }
}
