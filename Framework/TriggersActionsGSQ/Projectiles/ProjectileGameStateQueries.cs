using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
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
        if (!TryGetMultipleTillEnd(query, 1, out var projectiles, out var error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        if (!RequireContextModData(query, context.TargetItem?.modData, ProjectileIdItemKey, out var id,
                "Projectile")) return false;
        return projectiles.Contains(id);
    }
}
