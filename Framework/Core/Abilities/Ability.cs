using CombatExtended.Framework.Core.Abilities.Impl;
using CombatExtended.Framework.Core.Util;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace CombatExtended.Framework.Core.Abilities;

public class Ability
{
    
    /// <summary>
    /// Unique id for this ability.
    /// </summary>
    public string Id { get; protected set; }
    
    /// <summary>
    /// Name that should be displayed for this ability.
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    /// Description for this ability.
    /// </summary>
    public string Description { get; set; }

    public int CooldownTicks { get; private set; }
    
    public Texture2D Texture { get; set; }
    
    public int SpriteIndex { get; set; }
    
    public string? HighlightRangeId { get; set; }
    
    public string? TriggerActionCondition { get; set; }

    public List<string>? ActionsOnTrigger { get; set; }

    public string? CooldownEndActionCondition { get; set; }
    
    public List<string>? ActionsOnCooldownEnd { get; set; }

    public int CooldownTicksElapsed { get; set; }

    public bool IsCooldownEndHandled { get; private set; } = true;
    
    private HashSet<ParsedVector>? _relativeTiles;
    
    protected Ability(string id, AbilityData data)
    {
        Id = id;
        DisplayName = data.DisplayName;
        Description = data.Description;
        CooldownTicks = data.Cooldown.ToTicks();
        Texture = Helper.GameContent.Load<Texture2D>(data.Texture);
        SpriteIndex = data.SpriteIndex;
        CooldownTicksElapsed = CooldownTicks;
        HighlightRangeId = data.HighlightRangeId;
        TriggerActionCondition = data.TriggerActionCondition;
        ActionsOnTrigger = data.ActionsOnTrigger;
        CooldownEndActionCondition = data.CooldownEndActionCondition;
        ActionsOnCooldownEnd = data.ActionsOnCooldownEnd;
    }

    /// <summary>
    /// Get affected by this skill tiles.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="location">Location instance.</param>
    /// <param name="cursorTile">Tile of cursor.</param>
    /// <param name="playerTile">Tile of player.</param>
    /// <returns></returns>
    public virtual HashSet<Vector2> GetHighlightedTiles(Farmer player, GameLocation location, Vector2 cursorTile,
        Vector2 playerTile)
    {
        return RangeUtil.GetTilesFromRelative(GetRelativeTiles(), cursorTile, playerTile);
    }

    /// <summary>
    /// Do effect when this skill is triggered.
    /// </summary>
    /// <param name="player">Player instance.</param>
    /// <param name="location">Location instance.</param>
    /// <param name="cursorTile">Tile of cursor.</param>
    /// <param name="playerTile">Tile of player.</param>
    public void Trigger(Farmer player, GameLocation location, Vector2 cursorTile, Vector2 playerTile)
    {
        IsCooldownEndHandled = false;
        CooldownTicksElapsed = 0;
        OnTrigger(player, location, cursorTile, playerTile);
    }
    
    protected virtual void OnTrigger(Farmer player, GameLocation location, Vector2 cursorTile, Vector2 playerTile) {}

    public void EndCooldown(Farmer player, GameLocation location)
    {
        IsCooldownEndHandled = true;
        OnCooldownEnded(player, location);
    }
    
    protected virtual void OnCooldownEnded(Farmer player, GameLocation location) {}

    public static Ability Create(string abilityId)
    {
        if (!AbilityDataAsset.Data.TryGetValue(abilityId, out var abilityData))
            throw new ArgumentException($"Unknown ability ID {abilityId}");
        if (CustomDataAbilityFactories.TryGetValue(abilityId, out var dataFactory))
            return dataFactory(abilityData);
        
        return abilityData.Type switch
        {
            ContinuousAbility.ContinuousAbilityType => ContinuousAbility.CreateCustom(abilityId, abilityData),
            ThrowAbilityImpl.ThrowAbilityType => ThrowAbilityImpl.CreateCustom(abilityId, abilityData),
            _ => new Ability(abilityId, abilityData)
        };
    }
    
    private static readonly Dictionary<string, Func<AbilityData, Ability>> CustomDataAbilityFactories = new ();
    
    public static void RegisterCustom(string abilityId, Func<AbilityData, Ability> factory)
    {
        CustomDataAbilityFactories[abilityId] = factory;
    }

    private HashSet<ParsedVector> GetRelativeTiles()
    {
        if (HighlightRangeId is null || !RangeDataAsset.Exists(HighlightRangeId)) return new HashSet<ParsedVector>();
        if (!RangeDataAsset.NonStaticRange(HighlightRangeId)) 
            return _relativeTiles ??= 
                TileParser.ParseTiles(RangeDataAsset.Data[HighlightRangeId].Tiles ?? new List<RangeTileData>());
        return TileParser.ParseTiles(RangeDataAsset.Data[HighlightRangeId].Tiles ?? new List<RangeTileData>());
    }
}
