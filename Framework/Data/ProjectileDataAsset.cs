using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

#pragma warning disable CS8618
public class ProjectileData
{
    public string Id;
    
    [ContentSerializer(Optional = true)]
    public int MinDamage = 0;
    
    [ContentSerializer(Optional = true)]
    public int MaxDamage = 0;

    [ContentSerializer(Optional = true)] 
    public string DamageType = Core.Damage.DamageType.Physical;
    
    [ContentSerializer(Optional = true)]
    public int Bounces = 0;

    [ContentSerializer(Optional = true)] 
    public int Pierces = 1;
    
    [ContentSerializer(Optional = true)]
    public int MaxDistance = 4;
    
    [ContentSerializer(Optional = true)]
    public float Velocity = 10;

    [ContentSerializer(Optional = true)] 
    public float Rotation = 0f;
    
    [ContentSerializer(Optional = true)]
    public float RotationVelocity = 32f;
    
    [ContentSerializer(Optional = true)]
    public int TailLength = 1;

    [ContentSerializer(Optional = true)] 
    public string? ItemId;
    
    [ContentSerializer(Optional = true)] 
    public string? Texture;

    [ContentSerializer(Optional = true)] 
    public int? SpriteIndex;

    [ContentSerializer(Optional = true)] 
    public string? Animation;
}

[DataAsset("Projectiles")]
public class ProjectileDataAsset : DataAsset<ProjectileData>
{
    private ProjectileDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new ProjectileDataAsset(path);
        DataAsset<ProjectileData>.RegisterAsset(helper);
    }
    
    public static bool Exists(string projectileId)
    {
        return Data.ContainsKey(projectileId);
    }
}
