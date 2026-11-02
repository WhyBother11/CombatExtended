using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Projectiles;

[TriggerActionGSQ_Holder]
public static class ProjectileGameStateQueries
{
    
    public static readonly string ProjectileIdItemKey = "Projectile_Id".PrependId();
    public static readonly string ProjectilePiercesItemKey = "Projectile_Pierces".PrependId();
    
    [GSQ]
    private static bool PROJECTILE_ID(string[] query, GameStateQueryContext context)
    {
        if (!TriggerActionGSQUtils.TryGetMultipleTillEnd(query, 1, out var projectiles, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (context.TargetItem?.modData.TryGetValue(ProjectileIdItemKey, out var id) ?? false)
        {
            return projectiles.Contains(id);
        }
        return GameStateQuery.Helpers.ErrorResult(query, "GSQ context does not contain monster buff id");
    }
}
