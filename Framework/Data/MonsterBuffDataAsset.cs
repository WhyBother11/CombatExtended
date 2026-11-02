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
    public float? DoEffectEverySec;

    [ContentSerializer(Optional = true)]
    public float? DoAnimationEverySec;

    [ContentSerializer(Optional = true)] 
    public string? TemporaryAnimatedSprite;

    [ContentSerializer(Optional = true)] 
    public MonsterBuffAttributesData? Attributes;

    [ContentSerializer(Optional = true)] 
    public MonsterBuffAttributesData? Effects;

    [ContentSerializer(Optional = true)] 
    public MonsterBuffStackableData? StackableData;
    
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

public class MonsterBuffAttributesData
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
    public List<DamageAttributesData>? DamageData;

    [ContentSerializer(Optional = true)] 
    public List<ResistanceAttributesData>? ResistanceData;
    
    public class ResistanceAttributesData
    {
        #nullable disable
        public string DamageType;

        public float Delta;
        #nullable restore
    }
    
    public class DamageAttributesData
    {
        #nullable disable
        public string DamageType;

        public float Damage;
        #nullable restore

        [ContentSerializer(Optional = true)] 
        public float? MaxDamage;
    }
}

[DataAsset("MonsterBuffs")]
public class MonsterBuffDataAsset : DataAsset<MonsterBuffData>
{
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
}
