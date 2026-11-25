using static CombatExtended.Framework.TriggersActionsGSQ.Util.TriggerActionGSQUtils;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;

namespace CombatExtended.Framework.TriggersActionsGSQ.LinkEffects;

[TriggerActionGSQ_Holder]
public class LinkEffectTriggerActions
{
    [TriggerAction]
    [MonsterContextAware]
    private static bool LinkMonster(string[] args, TriggerActionContext ctx, Monster? monster, out string error)
    {
        if (!ArgUtility.TryGet(args, 1, out var linkId, out error))
            return false;
        if (!LinkEffectDataAsset.Data.TryGetValue(linkId, out var linkData) && linkId is not "_none")
        {
            error = $"Unknown link effect ID: {linkId}.";
            return false;
        }
        if (ArgUtility.TryGet(args, 2, out var rangeId, out _))
        {
            return TryHandleRange(rangeId, null, Game1.player, out error,
                monsterHandler: HandleMonster);
        }
        if (!RequireContext(monster, out error)) return false;
        HandleMonster(monster);
        return true;

        void HandleMonster(Monster m)
        {
            if (linkId is "_none") 
                LinkEffectManager.Remove(m);
            else 
                LinkEffectManager.BeginMonsterLink(m, linkId, linkData!);
        }
    }
}