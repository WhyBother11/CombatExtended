using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley;

namespace CombatExtended.Framework.Core.Abilities;

public class ContinuousAbility : Ability
{
    
    public const string ContinuousAbilityType = "Continuous";
    
    public int DurationTicks { get; private set; }

    public int DoContinuousEffectEveryTick { get; set; }
    
    public int DurationTicksElapsed { get; set; }
    
    public bool EffectAttachedToWeapon { get; set; }
    
    public string? EndActionCondition { get; set; }
    
    public List<string>? ActionsOnEnd { get; set; }
    
    public string? EffectActionCondition { get; set; }
    
    public List<string>? ActionsOnEffect { get; set; }

    public bool IsEndHandled { get; set; } = true;

    public bool IsActive { get; set; }

    protected ContinuousAbility(
        string id, AbilityData data) : base(id, data)
    {
        DurationTicks = data.Duration!.Value.ToTicks();
        EffectAttachedToWeapon = data.AttachedToWeapon;
        DoContinuousEffectEveryTick = data.DoEffectEverySec.ToTicks();
        DurationTicksElapsed = DurationTicks;
        EndActionCondition = data.EndActionCondition;
        ActionsOnEnd = data.ActionsOnEnd;
        EffectActionCondition = data.EffectActionCondition;
        ActionsOnEffect = data.ActionsOnEffect;
    }
    
    protected override void OnTrigger(Farmer player, GameLocation location, Vector2 cursorTile, Vector2 playerTile)
    {
        DurationTicksElapsed = 0;
        IsActive = true;
        IsEndHandled = false;
    }

    public virtual void OnContinuousTrigger(Farmer player, GameLocation location) {}

    public void End(Farmer player, GameLocation location)
    {
        IsActive = false;
        IsEndHandled = true;
        OnEnd(player, location);
    }

    public virtual void OnEnd(Farmer player, GameLocation location) {}

    public static ContinuousAbility CreateCustom(string abilityId, AbilityData abilityData)
    {
        return new ContinuousAbility(abilityId, abilityData);
    }
}
