using CombatExtended.Framework.Util;

namespace CombatExtended.Framework.TriggersActionsGSQ.Util;

public static class Triggers
{
    public static readonly string AbilityTriggered = "AbilityTriggered".PrependId();
    public static readonly string AbilityEnded = "AbilityEnded".PrependId();
    public static readonly string CooldownEnded = "AbilityCooldownEnded".PrependId();
    
    public static readonly string BuffApplied = "BuffApplied".PrependId();
    
    public static readonly string AttackHitMonster = "AttackHitMonster".PrependId();
    public static readonly string AttackMissedMonster = "AttackMissedMonster".PrependId();
    public static readonly string AttackHitAnyMonster = "AttackHitAnyMonster".PrependId();
    public static readonly string PlayerAttacked = "PlayerAttacked".PrependId();
    
    public static readonly string MonsterBuffApplied = "MonsterBuffApplied".PrependId();
    public static readonly string MonsterBuffRemoved = "MonsterBuffRemoved".PrependId();
    public static readonly string MonsterBuffExpired = "MonsterBuffExpired".PrependId();
    public static readonly string MonsterBuffContinuousEffect = "MonsterBuffContinuousEffect".PrependId();
    
    public static readonly string ProjectileMonsterHit = "ProjectileMonsterHit".PrependId();
}