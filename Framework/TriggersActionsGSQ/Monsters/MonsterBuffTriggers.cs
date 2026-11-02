using CombatExtended.Framework.Util;

namespace CombatExtended.Framework.TriggersActionsGSQ.Monsters;

[TriggerActionGSQ_Holder]
public static class MonsterBuffTriggers
{
    [Trigger]
    private static void MonsterBuffApplied() {}
    
    [Trigger]
    private static void MonsterBuffRemoved() {}
    
    [Trigger]
    private static void MonsterBuffExpired() {}
    
    [Trigger]
    private static void MonsterBuffContinuousEffect() {}
}
