using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using HarmonyLib;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Core.Buffs;

[HarmonyPatch]
public static class BuffPatch
{
    
    [HarmonyPatch(typeof(BuffManager), nameof(BuffManager.Apply))]
    [HarmonyPostfix]
    private static void BuffManager_Apply_Postfix(Buff buff)
    {
        if (BuffExtensionDataAsset.Data.TryGetValue(buff.id, out var data) && data.ExtraAttributes is not null)
        {
            buff.AddExtraBuffEffects(data.ExtraAttributes);
        }
        var item = ItemRegistry.Create(buff.id);
        TriggerActionManager.Raise(Triggers.BuffApplied, null, Game1.player.currentLocation, Game1.player, item);
    }
}
