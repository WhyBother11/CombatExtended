using System.Collections.Immutable;
using CombatExtended.Framework.TriggersActionsGSQ.Sequence;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley.Delegates;

namespace CombatExtended.Framework.Managers;

/// <summary>
/// Manager to handle sequence orchestrators.
/// </summary>
public class SequenceOrchestratorManager
{
    public static SequenceOrchestratorManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<SequenceOrchestratorManager> PerScreenInstance = new(()  => 
        new SequenceOrchestratorManager());

    /// <summary>
    /// The set of names of trigger actions whose delegates have <see cref="TriggersActionsGSQ.Util.SequenceAware"/> attribute.
    /// </summary>
    public static readonly HashSet<string> SequenceAwareTriggerActions = new();

    private int _latestId;
    
    private readonly Dictionary<int, SequenceOrchestrator> _sequenceOrchestrators = new();
    public ImmutableDictionary<int, SequenceOrchestrator> SequenceOrchestrators => _sequenceOrchestrators.ToImmutableDictionary();

    public static void Register(IModHelper helper)
    {
        PerScreenInstance.Value = new SequenceOrchestratorManager();
    }

    /// <summary>
    /// Create a new <see cref="SequenceOrchestrator"/>, register and start it.
    /// </summary>
    /// <param name="actions">The list of trigger actions to execute</param>
    /// <param name="context"><see cref="TriggerActionContext"/> to pass to <paramref name="actions"/></param>
    public void New(List<string> actions, TriggerActionContext context)
    {
        var orchestrator = new SequenceOrchestrator(_latestId, actions, context);
        _sequenceOrchestrators.Add(orchestrator.Id, orchestrator);
        _latestId++;
        orchestrator.Start();
    }
}
