using CombatExtended.Framework.Events.Args;

namespace CombatExtended.Framework.Events;

public static class Events
{
    public static event EventHandler<WeaponEquippedEventArgs>? OnWeaponEquipped;
    
    public static event EventHandler<WeaponUnequippedEventArgs>? OnWeaponUnequipped;
    
    public static event EventHandler<WeaponSwitchedEventArgs>? OnWeaponSwitched;
    
    public static event EventHandler<MonsterTakenDamageEventArgs>? OnMonsterTakenDamage;
    
    public static event EventHandler<MonsterAttackedEventArgs>? OnMonsterAttacked; 

    internal static void RaiseWeaponEquipped(WeaponEquippedEventArgs args)
    {
        OnWeaponEquipped?.Invoke(null, args);
    }
    
    internal static void RaiseWeaponUnequipped(WeaponUnequippedEventArgs args)
    {
        OnWeaponUnequipped?.Invoke(null, args);
    }
    
    internal static void RaiseWeaponSwitched(WeaponSwitchedEventArgs args)
    {
        OnWeaponSwitched?.Invoke(null, args);
    }

    internal static void RaiseMonsterTakenDamage(MonsterTakenDamageEventArgs args)
    {
        OnMonsterTakenDamage?.Invoke(null, args);
    }

    internal static void RaiseMonsterAttacked(MonsterAttackedEventArgs args)
    {
        OnMonsterAttacked?.Invoke(null, args);
    }
}
