using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

public class LinkEffectData
{
    #nullable disable
    public string Texture;

    public string Type;

    public List<int> Frames;
    #nullable restore

    [ContentSerializer(Optional = true)]
    public float Duration = -2;

    [ContentSerializer(Optional = true)] 
    public Rectangle Size = new (0, 0, 16, 16);

    [ContentSerializer(Optional = true)] 
    public float Scale = 4f;

    [ContentSerializer(Optional = true)]
    public LinkEffectTriggerEffectData? TriggerEffectData;
}

public class LinkEffectTriggerEffectData
{
    [ContentSerializer(Optional = true)]
    public float DoMonsterEffectEverySec = 0;

    [ContentSerializer(Optional = true)]
    public float DoMonsterTouchEffectEverySec = 0;
}

[DataAsset("LinkEffects")]
public class LinkEffectDataAsset : DataAsset<LinkEffectData>
{
    private LinkEffectDataAsset(string path) : base(path) { }
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new LinkEffectDataAsset(path);
        DataAsset<LinkEffectData>.RegisterAsset(helper);
    }
}
