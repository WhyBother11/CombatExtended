using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Sequence;

[TriggerActionGSQ_Holder]
public class SequenceTriggerActions
{
    [TriggerAction]
    private static bool Sequence(string[] args, TriggerActionContext ctx, out string error)
    {
        var indexes = new LinkedList<int>();
        indexes.AddLast(1);
        for (var i = 1; i < args.Length; i++)
        {
            if (args[i] == "##")
            {
                if (i == indexes.Last())
                {
                    error = "Invalid format: two '##' in a row.";
                    return false;
                }

                indexes.AddLast(i + 1);
            }
        }

        var actions = new List<string>();
        for (var node = indexes.First; node is not null; node = node.Next)
        {
            // get rightmost arg, but +1 to it because exclusive edge, also accounting for ## symbol (or its absence if the rightmost action)
            var rightIdxExcl = (node.Next?.Value ?? args.Length + 1) - 1;
            actions.Add(ArgUtility.UnsplitQuoteAware(args[node.Value..rightIdxExcl], ' '));
        }
        SequenceOrchestratorManager.New(actions, ctx);
        error = null;
        return true;
    }
}
