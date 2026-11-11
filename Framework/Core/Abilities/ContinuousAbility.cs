using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;

namespace CombatExtended.Framework.Core.Abilities;

public class ContinuousAbility : Ability
{
    
    public const string ContinuousAbilityType = "Continuous";
    
    public int DurationTicks { get; private set; }

    public int DoContinuousEffectEveryTick { get; set; }
    
    public int DurationTicksElapsed { get; set; }
    
    public bool EffectAttachedToWeapon { get; set; }
    
    public HashSet<Vector2> AffectedTiles = new ();

    public bool IsEndHandled { get; set; } = true;

    public bool IsActive { get; set; }

    protected ContinuousAbility(
        string id,
        string displayName,
        string description,
        float cooldown,
        string texture,
        int spriteIndex,
        float durationSeconds,
        int doEffectEveryTick = 0,
        bool effectAttachedToWeapon = false
    ) : base(id, displayName, description, cooldown, texture, spriteIndex)
    {
        DurationTicks = durationSeconds.ToTicks();
        EffectAttachedToWeapon = effectAttachedToWeapon;
        DoContinuousEffectEveryTick = doEffectEveryTick;
        DurationTicksElapsed = DurationTicks;
    }

    protected ContinuousAbility(
        string id, AbilityData data) : base(id, data)
    {
        DurationTicks = data.Duration!.Value.ToTicks();
        EffectAttachedToWeapon = data.AttachedToWeapon;
        DoContinuousEffectEveryTick = data.DoEffectEverySec?.ToTicks() ?? 0;
        DurationTicksElapsed = DurationTicks;
    }
    
    protected override void OnTrigger(Farmer player, GameLocation location, Vector2 cursorTile, Vector2 playerTile)
    {
        AffectedTiles = GetHighlightedTiles(player, location, cursorTile, playerTile);
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
