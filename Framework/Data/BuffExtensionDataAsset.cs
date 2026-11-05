using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;
using StardewValley.GameData.Buffs;

namespace CombatExtended.Framework.Data;

public class BuffExtensionData
{

    [ContentSerializer(Optional = true)] 
    public BuffExtensionStackableData? StackableData;

    [ContentSerializer(Optional = true)] 
    public ExtraBuffAttributesData? ExtraAttributes;

}

public class BuffExtensionStackableData
{
    public int MaxStacks;
    
    [ContentSerializer(Optional = true)]
    public bool IndependentStacks = false;

    [ContentSerializer(Optional = true)] 
    public BuffAttributesData? AttributesPerStack;

    [ContentSerializer(Optional = true)] 
    public ExtraBuffAttributesData? ExtraAttributesPerStack;
}

public class ExtraBuffAttributesData
{
    [ContentSerializer(Optional = true)] 
    public float ArmourClass = 0;

    [ContentSerializer(Optional = true)] 
    public float FlatCriticalChance = 0;
    
    [ContentSerializer(Optional = true)] 
    public float FlatCriticalMultiplier = 0;
}

[DataAsset("BuffExtensionData")]
public class BuffExtensionDataAsset : DataAsset<BuffExtensionData>
{
    private BuffExtensionDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new BuffExtensionDataAsset(path);
        DataAsset<BuffExtensionData>.RegisterAsset(helper);
    }
}
