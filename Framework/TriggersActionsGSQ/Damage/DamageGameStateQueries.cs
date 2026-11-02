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
    /// Check weapon attack resulting damage from <paramref name="context"/>'s <see cref="GameStateQueryContext.CustomFields"/>.
    /// </summary>
    /// <param name="query">GSQ</param>
    /// <param name="context"><see cref="GameStateQuery"/></param>
    /// <returns>GSQ result</returns>
    [GSQ]
    private static bool DAMAGE_RESULT(string[] query, GameStateQueryContext context)
    {
        if (!ArgUtility.TryGet(query, 1, out var arg1, out var error) ||
            !ArgUtility.TryGet(query, 2, out var arg2, out error))
        {
            return GameStateQuery.Helpers.ErrorResult(query, error);
        }
        var minDamage = arg1 == "_" ? int.MinValue : int.Parse(arg1);
        var maxDamage = arg2 == "_" ? int.MaxValue : int.Parse(arg2);
        if (context.TargetItem?.modData.TryGetValue(DamageItemKey, out var damageObj) ?? false)
        {
            var damage = int.Parse(damageObj);
            return damage >= minDamage && damage <= maxDamage;
        }
        return false;
    }

    /// <summary>
    /// Check if damage was or was not crit from <paramref name="context"/>'s <see cref="GameStateQueryContext.CustomFields"/>.
    /// </summary>
    /// <param name="query">GSQ</param>
    /// <param name="context"><see cref="GameStateQuery"/></param>
    /// <returns>GSQ result</returns>
    [GSQ]
    private static bool DAMAGE_IS_CRIT(string[] query, GameStateQueryContext context)
    {
        var critConditionStr = ArgUtility.Get(query, 1, "true");
        var critCondition = critConditionStr == "true";
        if (context.TargetItem?.modData.TryGetValue(IsCritItemKey, out var isCritStr) ?? false)
        {
            var isCrit = bool.Parse(isCritStr);
            return critCondition == isCrit;
        }
        return false;
    }
}
