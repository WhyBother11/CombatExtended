using CombatExtended.Framework.Core.Abilities.Impl;
using CombatExtended.Framework.Core.Util;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
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
    
    /// <summary>
    /// How many seconds should pass before using this ability again.
    /// </summary>
    public float Cooldown { 
        get => _cooldown;
        set
        {
            _cooldown = value;
            CooldownTicks = value.ToTicks();
        }
    }
    private float _cooldown;

    public int CooldownTicks { get; private set; }
    
    public Texture2D Texture { get; set; }
    
    public int SpriteIndex { get; set; }

    public int CooldownTicksElapsed { get; set; }

    public bool IsCooldownEndHandled { get; set; } = true;
    
    protected readonly AbilityData? Data;
    
    private HashSet<ParsedVector>? _relativeTiles;
    
    protected Ability(
        string id,
        string displayName,
        string description,
        float cooldown,
        string texture,
        int spriteIndex)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        Cooldown = cooldown;
        Texture = Helper.GameContent.Load<Texture2D>(texture);
        SpriteIndex = spriteIndex;
        CooldownTicksElapsed = CooldownTicks;
    }
    
    protected Ability(string id, AbilityData data)
    {
        Id = id;
        DisplayName = data.DisplayName;
        Description = data.Description;
        Cooldown = data.Cooldown;
        Texture = Helper.GameContent.Load<Texture2D>(data.Texture);
        SpriteIndex = data.SpriteIndex;
        CooldownTicksElapsed = CooldownTicks;
        Data = data;
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
        if (CustomAbilityFactories.TryGetValue(abilityId, out var factory))
            return factory();
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
    
    private static readonly Dictionary<string, Func<Ability>> CustomAbilityFactories = new ();
    private static readonly Dictionary<string, Func<AbilityData, Ability>> CustomDataAbilityFactories = new ();

    public static void RegisterCustom(string abilityId, Func<Ability> factory)
    {
        CustomAbilityFactories[abilityId] = factory;
    }
    
    public static void RegisterCustom(string abilityId, Func<AbilityData, Ability> factory)
    {
        CustomDataAbilityFactories[abilityId] = factory;
    }

    private HashSet<ParsedVector> GetRelativeTiles()
    {
        if (Data?.HighlightRangeId is null || !RangeDataAsset.Exists(Data.HighlightRangeId)) return new HashSet<ParsedVector>();
        if (!RangeDataAsset.NonStaticRange(Data.HighlightRangeId)) 
            return _relativeTiles ??= 
                TileParser.ParseTiles(RangeDataAsset.Data[Data.HighlightRangeId].Tiles ?? new List<RangeTileData>());
        return TileParser.ParseTiles(RangeDataAsset.Data[Data.HighlightRangeId].Tiles ?? new List<RangeTileData>());
    }
}
