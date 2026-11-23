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
    /// The set of names of trigger actions whose delegates have <see cref="SequenceAware"/> attribute.
    /// </summary>
    public static readonly HashSet<string> SequenceAwareTriggerActions = new();

    private int _latestId;
    
    public Dictionary<int, SequenceOrchestrator> SequenceOrchestrators { get; private set; } = new();

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
        SequenceOrchestrators.Add(orchestrator.Id, orchestrator);
        _latestId++;
        orchestrator.Start();
    }
}
