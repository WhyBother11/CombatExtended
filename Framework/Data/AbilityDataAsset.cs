using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

#pragma warning disable CS8618
public class AbilityData
{
    
    #nullable disable
    public string Type;

    public string DisplayName;

    public string Description;

    public float Cooldown;

    public string Texture;

    public int SpriteIndex;
    #nullable restore


    [ContentSerializer(Optional = true)] 
    public string? HighlightRangeId;

    [ContentSerializer(Optional = true)]
    public List<string>? ContextTags;

    [ContentSerializer(Optional = true)] 
    public string? TriggerActionCondition;

    [ContentSerializer(Optional = true)] 
    public List<string>? ActionsOnTrigger;

    [ContentSerializer(Optional = true)]
    public string? CooldownEndActionCondition;
    
    [ContentSerializer(Optional = true)] 
    public List<string>? ActionsOnCooldownEnd;

    #region DataAbilityContinious

    [ContentSerializer(Optional = true)]
    public bool AttachedToWeapon = false;
    
    [ContentSerializer(Optional = true)] 
    public float? Duration;

    [ContentSerializer(Optional = true)]
    public int DoEffectEverySec = 0;

    [ContentSerializer(Optional = true)] 
    public string? EndActionCondition;
    
    [ContentSerializer(Optional = true)]
    public List<string>? ActionsOnEnd;
    
    [ContentSerializer(Optional = true)] 
    public string? EffectActionCondition;
    
    [ContentSerializer(Optional = true)]
    public List<string>? ActionsOnEffect;
    #endregion

    #region DataAbilityThrow
    [ContentSerializer(Optional = true)] 
    public bool Returnable = false;
    
    [ContentSerializer(Optional = true)] 
    public bool ReturnOnWarp = true;

    [ContentSerializer(Optional = true)] 
    public string? ProjectileId;

    [ContentSerializer(Optional = true)] 
    public float? Angle;
    #endregion
}

[DataAsset("Abilities")]
public class AbilityDataAsset : DataAsset<AbilityData>
{
    private AbilityDataAsset(string path) : base(path) {}

    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new AbilityDataAsset(path);
        DataAsset<AbilityData>.RegisterAsset(helper);
    }
    
    private Dictionary<string, List<string>>? _contextTags;
    private Dictionary<string, List<string>> InstanceContextTags => _contextTags ??= Data
        .Where(entry => entry.Value.ContextTags != null)
        .ToDictionary(
            entry => entry.Key, 
            entry => entry.Value.ContextTags!);

    public static Dictionary<string, List<string>> ContextTags => ((AbilityDataAsset)_instance).InstanceContextTags;
    
    public static bool Exists(string abilityId)
    {
        return Data.ContainsKey(abilityId);
    }

    public static bool HasContextTag(string abilityId, string tag)
    {
        return ContextTags.TryGetValue(abilityId, out var contextTags) && contextTags.Contains(tag);
    }

    protected override void OnInvalidated()
    {
        base.OnInvalidated();
        _contextTags = null;
    }
}
