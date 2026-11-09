using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

#pragma warning disable CS8618
public class DamageModifiersData
{
    public string Condition;
    
    public List<DamageModifierData> Modifiers;
    
}

public class DamageModifierData
{
    public const string ModifierGroupDefault = "_default";

    public string Id;
    
    public string Stage;

    public float MinValue;
    
    [ContentSerializer(Optional = true)] 
    public float? MaxValue;

    [ContentSerializer(Optional = true)] 
    public string DamageType = Core.Damage.DamageType.Physical;

    [ContentSerializer(Optional = true)] 
    public string Group = ModifierGroupDefault;
}
#pragma warning restore CS8618

[DataAsset("DamageModifiers")]
public class DamageModifiersDataAsset : DataAsset<DamageModifiersData>
{
    
    private DamageModifiersDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new DamageModifiersDataAsset(path);
        DataAsset<DamageModifiersData>.RegisterAsset(helper);
    }
}
