using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

public class WeaponExtensionData
{
    [ContentSerializer(Optional = true)] 
    public string? Abilities;

    [ContentSerializer(Optional = true)] 
    public List<DamageTypeWeaponExtensionData>? DamageTypes;

    [ContentSerializer(Optional = true)] 
    public Dictionary<string, WeaponStateData>? States;

    [ContentSerializer(Optional = true)] 
    public string? ActionPoints;
    
    [ContentSerializer(Optional = true)]
    public List<string> ActionsOnEquip = new ();
    
    [ContentSerializer(Optional = true)]
    public List<string> ActionsOnUnequip = new ();
}

public class DamageTypeWeaponExtensionData
{
    #nullable disable
    public string Type;

    public int MinDamage;

    public int MaxDamage;
    #nullable restore
}

public class WeaponStateData
{
    
    #nullable disable
    public string Id;
    #nullable restore

    [ContentSerializer(Optional = true)] 
    public int? MinDamage;

    [ContentSerializer(Optional = true)] 
    public int? MaxDamage;

    [ContentSerializer(Optional = true)] 
    public string? Texture;

    [ContentSerializer(Optional = true)] 
    public int? SpriteIndex;

    [ContentSerializer(Optional = true)] 
    public float? Knockback;

    [ContentSerializer(Optional = true)] 
    public int? Speed;

    [ContentSerializer(Optional = true)] 
    public int? Precision;

    [ContentSerializer(Optional = true)] 
    public int? Defense;

    [ContentSerializer(Optional = true)] 
    public int? AreaOfEffect;

    [ContentSerializer(Optional = true)] 
    public float? CritChance;

    [ContentSerializer(Optional = true)] 
    public float? CritMultiplier;

    [ContentSerializer(Optional = true)] 
    public List<DamageTypeWeaponExtensionData>? DamageTypes;
}

[DataAsset("WeaponExtensionData")]
public class WeaponExtensionDataAsset : DataAsset<WeaponExtensionData>
{
    private static WeaponExtensionDataAsset Instance => (WeaponExtensionDataAsset)_instance;
    
    private Dictionary<string, string>? _abilities;
    private Dictionary<string, string> InstanceAbilities => _abilities ??= Data
        .Where(entry => entry.Value.Abilities != null)
        .ToDictionary(
            entry => entry.Key, 
            entry => entry.Value.Abilities!);

    public static Dictionary<string, string> Abilities => Instance.InstanceAbilities;

    private Dictionary<string, Dictionary<string, WeaponStateData>>? _weaponStates;
    private Dictionary<string, Dictionary<string, WeaponStateData>> InstanceWeaponStates => _weaponStates ??= Data
        .Where(entry => entry.Value.States != null)
        .ToDictionary(
            entry => entry.Key, 
            entry => entry.Value.States!);

    public static Dictionary<string, Dictionary<string, WeaponStateData>> WeaponStates => Instance.InstanceWeaponStates;
    

    private Dictionary<string, List<DamageTypeWeaponExtensionData>>? _damageTypes;
    private Dictionary<string, List<DamageTypeWeaponExtensionData>> InstanceDamageTypes => _damageTypes ??= Data
        .Where(entry => entry.Value.DamageTypes != null)
        .Where(entry => entry.Key != DamageType.Physical)
        .ToDictionary(
            entry => entry.Key,
            entry => entry.Value.DamageTypes!
        );
    
    public static Dictionary<string, List<DamageTypeWeaponExtensionData>> DamageTypes => Instance.InstanceDamageTypes;
    
    private Dictionary<string, string>? _actionPoints;
    private Dictionary<string, string> InstanceActionPoints => _actionPoints ??= Data
        .Where(entry => entry.Value.ActionPoints != null)
        .ToDictionary(
            entry => entry.Key, 
            entry => entry.Value.ActionPoints!);

    public static Dictionary<string, string> ActionPoints => Instance.InstanceActionPoints;


    private WeaponExtensionDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new WeaponExtensionDataAsset(path);
        DataAsset<WeaponExtensionData>.RegisterAsset(helper);
    }
    
    public static List<DamageTypeWeaponExtensionData> GetDefaultExtraDamageTypes(string weaponId)
    {
        var result = new List<DamageTypeWeaponExtensionData>();
        if (!DamageTypes.TryGetValue(weaponId, out var damageTypes))
            return result;
        
        foreach (var damageType in damageTypes)
        {
            if (DamageTypeDataAsset.Exists(damageType.Type)) result.Add(damageType);
            else Log.Warn($"Unknown damage type {damageType.Type} for weapon extension on {weaponId}");
        }
        return result;
    }

    protected override void OnInvalidated()
    {
        base.OnInvalidated();
        _abilities = null;
        _damageTypes = null;
        _weaponStates = null;
        _actionPoints = null;
    }
}
