using CombatExtended.Framework.Util;

namespace CombatExtended.Framework.TriggersActionsGSQ.Damage;

[TriggerActionGSQ_Holder]
public class DamageTriggers
{
    [Trigger]
    private static void AttackHitMonster() {}

    [Trigger]
    private static void AttackMissedMonster() {}
    
    [Trigger]
    private static void AttackHitAnyMonster() {}

    [Trigger]
    private static void PlayerAttacked() {}
}