using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;

namespace CombatExtended.Framework.TriggersActionsGSQ.Damage;

[TriggerActionGSQ_Holder]
public static class DamageGameStateQueries
{
    public static readonly string IsCritItemKey = "Damage_IsCrit".PrependId();
    public static readonly string DamageItemKey = "Damage_Amount".PrependId();

    /// <summary>
    /// Check if weapon attack resulting damage matches provided in arguments.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQuery"/>.</param>
    /// <returns>The GSQ evaluation result.</returns>
    [GSQ]
    private static bool DAMAGE_RESULT(string[] query, GameStateQueryContext context)
    {
        var maxDamage = int.MaxValue;
        if (!ArgUtility.TryGetInt(query, 1, out var minDamage, out var error))
            return GameStateQuery.Helpers.ErrorResult(query, error);
        if (ArgUtility.TryGetInt(query, 2, out var maxDamageArg, out _))
            maxDamage = maxDamageArg;
        if (!RequireContextModData(query, context.TargetItem?.modData, DamageItemKey, out var damageStr,
                "Damage")) return false;
        var damage = int.Parse(damageStr);
        return damage >= minDamage && damage <= maxDamage;
    }

    /// <summary>
    /// Check if damage was or was not critical.
    /// </summary>
    /// <param name="query">The game state query.</param>
    /// <param name="context">See <see cref="GameStateQuery"/>.</param>
    /// <returns>The GSQ evaluation result.</returns>
    [GSQ]
    private static bool DAMAGE_IS_CRIT(string[] query, GameStateQueryContext context)
    {
        if (!RequireContextModData(query, context.TargetItem?.modData, IsCritItemKey, out var isCritStr,
                "Damage")) return false;
        var isCrit = bool.Parse(isCritStr);
        return isCrit;
    }
}
