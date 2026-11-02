using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CombatExtended.Framework.Data;

public class ResistanceData
{
    
    #nullable disable
    public string Type;

    public string Value;

    #nullable restore
}

public class TagData
{
    
    #nullable disable
    public string Id;

    public string Priority;
    #nullable restore

    [ContentSerializer(Optional = true)] 
    public List<ResistanceData>? ResistanceData;
}

[DataAsset("Tags")]
public class TagDataAsset : DataAsset<TagData>
{
    private TagDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new TagDataAsset(path);
        DataAsset<TagData>.RegisterAsset(helper);
    }

    protected override void Load(AssetRequestedEventArgs e)
    {
        e.LoadFromModFile<Dictionary<string, TagData>>("assets/tags.json", AssetLoadPriority.Exclusive);
    }
}
