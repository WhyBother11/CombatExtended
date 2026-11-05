using CombatExtended.Framework.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CombatExtended.Framework.Data;

#nullable disable
public class DamageTypeData
{

    public string DisplayName;

    public string Color;

}
#nullable restore

[DataAsset("DamageTypes")]
public class DamageTypeDataAsset : DataAsset<DamageTypeData>
{
    
    private Dictionary<string, Color>? _damageTypeColors;
    private Dictionary<string, Color> InstanceDamageTypeColors => 
        _damageTypeColors ??= Data.ToDictionary(
            entry => entry.Key,
            entry => GetColor(entry.Value));

    public static Dictionary<string, Color> DamageTypeColors =>
        ((DamageTypeDataAsset)_instance).InstanceDamageTypeColors;
    
    private DamageTypeDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new DamageTypeDataAsset(path);
        DataAsset<DamageTypeData>.RegisterAsset(helper);
    }

    protected override void Load(AssetRequestedEventArgs e)
    {
        e.LoadFromModFile<Dictionary<string, DamageTypeData>>("assets/damage_types.json", AssetLoadPriority.Exclusive);
    }

    public static bool Exists(string damageTypeId)
    {
        return Data.ContainsKey(damageTypeId);
    }

    protected override void OnInvalidated()
    {
        base.OnInvalidated();
        _damageTypeColors = null;
    }

    private static Color GetColor(DamageTypeData data)
    {
        Color? color = null;
        if (!data.Color.StartsWith("-1"))
        {
            try
            {
                var rgb = ArgUtility.SplitBySpace(data.Color);
                color = new Color(
                    int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2])
                );
            }
            catch (Exception e) when (e is FormatException or OverflowException or IndexOutOfRangeException)
            {
                Log.Warn($"Cannot parse color data for damage type {data.DisplayName}, defaulting to physical damage color.");
            }
        }
        return color ?? new Color(byte.MaxValue, 130, 0);
    }
}
