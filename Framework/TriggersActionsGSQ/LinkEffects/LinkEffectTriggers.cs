using CombatExtended.Framework.Util;

namespace CombatExtended.Framework.TriggersActionsGSQ.LinkEffects;

[TriggerActionGSQ_Holder]
public class LinkEffectTriggers
{
    [Trigger]
    private static void LinkEffectTriggered() {}
    
    [Trigger]
    private static void MonsterTouchedLink() {}
}