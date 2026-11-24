using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

public class MonsterStatusData
{
    #nullable disable
    public string DisplayName;

    public string Description;
    #nullable restore
    
    [ContentSerializer(Optional = true)]
    public string? Condition;
    
    [ContentSerializer(Optional = true)] 
    public float DoEffectEverySec;

    [ContentSerializer(Optional = true)]
    public float DoAnimationEverySec;
    
    [ContentSerializer(Optional = true)] 
    public List<string>? AnimationTemporaryAnimatedSprites;
    
    [ContentSerializer(Optional = true)] 
    public List<string>? TemporaryAnimatedSprites;
    
    [ContentSerializer(Optional = true)] 
    public List<string>? ContextTags;
    
    [ContentSerializer(Optional = true)] 
    public MonsterStatusAttributesData? Attributes;
    
    [ContentSerializer(Optional = true)] 
    public MonsterStatusAttributesData? Effects;
}

public class MonsterStatusAttributesData
{

    [ContentSerializer(Optional = true)] 
    public float? Resilience;
    
    [ContentSerializer(Optional = true)] 
    public float? Health;

    [ContentSerializer(Optional = true)] 
    public float? MaxHealth;

    [ContentSerializer(Optional = true)] 
    public float? MissChance;

    [ContentSerializer(Optional = true)] 
    public float? ArmourClass;

    [ContentSerializer(Optional = true)] 
    public float? StunTime;

    [ContentSerializer(Optional = true)] 
    public float? Speed;
    
    [ContentSerializer(Optional = true)] 
    public float? Weight;
    
    [ContentSerializer(Optional = true)] 
    public float? HitMissRate;

    [ContentSerializer(Optional = true)] 
    public List<DamageAttributesData>? DamageData;

    [ContentSerializer(Optional = true)] 
    public List<ResistanceAttributesData>? ResistanceData;
    
    public class ResistanceAttributesData
    {
        #nullable disable
        public string Id;
        
        public string DamageType;

        public float Delta;
        #nullable restore
    }
    
    public class DamageAttributesData
    {
        #nullable disable
        public string Id;
        
        public string DamageType;

        public float Damage;
        #nullable restore

        [ContentSerializer(Optional = true)] 
        public float? MaxDamage;
    }
}

[DataAsset("MonsterConditions")]
public class MonsterStatusDataAsset : DataAsset<MonsterStatusData>
{
    private Dictionary<string, List<string>>? _contextTags;
    private Dictionary<string, List<string>> InstanceContextTags => _contextTags ??= Data
        .Where(entry => entry.Value.ContextTags != null)
        .ToDictionary(
            entry => entry.Key, 
            entry => entry.Value.ContextTags!);

    public static Dictionary<string, List<string>> ContextTags => ((MonsterStatusDataAsset)_instance).InstanceContextTags;
    
    private MonsterStatusDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new MonsterStatusDataAsset(path);
        DataAsset<MonsterStatusData>.RegisterAsset(helper);
    }
    
    public static bool Exists(string monsterConditionId)
    {
        return Data.ContainsKey(monsterConditionId);
    }

    public static bool HasContextTag(string abilityId, string tag)
    {
        return ContextTags.TryGetValue(abilityId, out var contextTags) && contextTags.Contains(tag);
    }
}