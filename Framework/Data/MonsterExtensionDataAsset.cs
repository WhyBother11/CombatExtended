using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CombatExtended.Framework.Data;

public class MonsterExtensionData
{
    [ContentSerializer(Optional = true)] 
    public string? Tags;

    [ContentSerializer(Optional = true)] 
    public int? ExtraArmourClass;
}

[DataAsset("MonsterExtensionData")]
public class MonsterExtensionDataAsset : DataAsset<MonsterExtensionData>
{
    private Dictionary<string, string>? _tags;
    private Dictionary<string, string> InstanceTags => _tags ??= Data
        .Where(entry => entry.Value.Tags != null)
        .ToDictionary(
            entry => entry.Key,
            entry => entry.Value.Tags!
        );
    public static Dictionary<string, string> Tags => ((MonsterExtensionDataAsset) _instance).InstanceTags;

    private Dictionary<string, int>? _extraArmourClasses;
    private Dictionary<string, int> InstanceExtraArmourClasses => _extraArmourClasses ??= Data
        .Where(entry => entry.Value.ExtraArmourClass != null)
        .ToDictionary(
            entry => entry.Key,
            entry => entry.Value.ExtraArmourClass!.Value
        );

    public static Dictionary<string, int> ExtraArmourClasses =>
        ((MonsterExtensionDataAsset)_instance).InstanceExtraArmourClasses;
    
    
    private MonsterExtensionDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new MonsterExtensionDataAsset(path);
        DataAsset<MonsterExtensionData>.RegisterAsset(helper);
    }

    protected override void Load(AssetRequestedEventArgs e)
    {
        e.LoadFromModFile<Dictionary<string, MonsterExtensionData>>("assets/monster_extensions.json", AssetLoadPriority.Exclusive);
    }

    protected override void OnInvalidated()
    {
        base.OnInvalidated();
        _tags = null;
        _extraArmourClasses = null;
    }
}
