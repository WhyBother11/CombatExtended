using CombatExtended.Framework.Util;

namespace CombatExtended.Framework.TriggersActionsGSQ.Abilities;

[TriggerActionGSQ_Holder]
public class AbilityTriggers
{
    [Trigger]
    private static void AbilityTriggered() {}
    
    [Trigger]
    private static void AbilityEnded() {}
    
    [Trigger]
    private static void AbilityCooldownEnded() {}
}
