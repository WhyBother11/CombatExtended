using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

public class MonsterBuffData
{
    #nullable disable
    public string Type;

    public string DisplayName;

    public string Description;

    public float Duration;

    public bool IsDebuff;
    #nullable restore

    [ContentSerializer(Optional = true)] 
    public float DoEffectEverySec = 0;

    [ContentSerializer(Optional = true)]
    public float DoAnimationEverySec = 0;

    [ContentSerializer(Optional = true)] 
    public List<string>? AnimationTemporaryAnimatedSprites;

    [ContentSerializer(Optional = true)] 
    public List<string>? TemporaryAnimatedSprites;

    [ContentSerializer(Optional = true)] 
    public MonsterBuffAttributesData? Attributes;

    [ContentSerializer(Optional = true)] 
    public MonsterBuffAttributesData? Effects;

    [ContentSerializer(Optional = true)] 
    public MonsterBuffStackableData? StackableData;

    [ContentSerializer(Optional = true)] 
    public string? ApplyCondition;
    
    [ContentSerializer(Optional = true)] 
    public List<string>? ContextTags;
    
    public class MonsterBuffStackableData
    {
        public int MaxStacks;
        
        [ContentSerializer(Optional = true)]
        public bool IndependentStacks = false;
        
        [ContentSerializer(Optional = true)] 
        public MonsterBuffAttributesData? AttributesPerStack;
        
        [ContentSerializer(Optional = true)] 
        public MonsterBuffAttributesData? EffectsPerStack;
    }
}

public class MonsterBuffAttributesData : MonsterAttributesData { }

[DataAsset("MonsterBuffs")]
public class MonsterBuffDataAsset : DataAsset<MonsterBuffData>
{
    
    private Dictionary<string, List<string>>? _contextTags;
    private Dictionary<string, List<string>> InstanceContextTags => _contextTags ??= Data
        .Where(entry => entry.Value.ContextTags != null)
        .ToDictionary(
            entry => entry.Key, 
            entry => entry.Value.ContextTags!);

    public static Dictionary<string, List<string>> ContextTags => ((MonsterBuffDataAsset)_instance).InstanceContextTags;
    
    private MonsterBuffDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new MonsterBuffDataAsset(path);
        DataAsset<MonsterBuffData>.RegisterAsset(helper);
    }
    
    public static bool Exists(string monsterBuffId)
    {
        return Data.ContainsKey(monsterBuffId);
    }

    public static bool HasContextTag(string monsterBuffId, string tag)
    {
        return ContextTags.TryGetValue(monsterBuffId, out var contextTags) && contextTags.Contains(tag);
    }
}
