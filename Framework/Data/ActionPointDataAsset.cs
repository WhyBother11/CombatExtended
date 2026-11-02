using CombatExtended.Framework.Util;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

public class ActionPointData
{
    #nullable disable
    public int MaxAmount;
    
    public string DisplayName;
    
    public string Description;

    public float ReplenishTime = -1f;

    public string Texture;

    public int SpriteIndex;
    #nullable restore
}

[DataAsset("ActionPoints")]
public class ActionPointDataAsset : DataAsset<ActionPointData>
{
    private ActionPointDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new ActionPointDataAsset(path);
        DataAsset<ActionPointData>.RegisterAsset(helper);
    }
    
    public static bool Exists(string id) => Data.ContainsKey(id);
}
