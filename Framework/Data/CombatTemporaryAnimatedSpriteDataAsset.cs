using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

#pragma warning disable CS8618
public class CombatTemporaryAnimatedSpriteData
{
    public string Texture;
    
    public Rectangle SourceRect;

    [ContentSerializer(Optional = true)]
    public bool AttachedToMonster = false;

    [ContentSerializer(Optional = true)]
    public AnimationSpecData? AnimationSpec;
    
    [ContentSerializer(Optional = true)]
    public float Interval = 100f;
    
    [ContentSerializer(Optional = true)]
    public int Frames = 1;
    
    [ContentSerializer(Optional = true)]
    public int Loops;
    
    [ContentSerializer(Optional = true)]
    public bool Flicker;
    
    [ContentSerializer(Optional = true)]
    public bool Flip;
    
    [ContentSerializer(Optional = true)]
    public float AlphaFade;
    
    [ContentSerializer(Optional = true)]
    public float Scale = 1f;
    
    [ContentSerializer(Optional = true)]
    public float ScaleChange;
    
    [ContentSerializer(Optional = true)]
    public float Rotation;
    
    [ContentSerializer(Optional = true)]
    public float RotationChange;

    public class AnimationSpecData
    {
        public string Type;

        public string SpawnArea;

        public string? Movement;
    }
}
#pragma warning restore CS8618

[DataAsset("CombatTemporaryAnimatedSprites")]
public class CombatTemporaryAnimatedSpriteDataAsset : DataAsset<CombatTemporaryAnimatedSpriteData>
{
    private CombatTemporaryAnimatedSpriteDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new CombatTemporaryAnimatedSpriteDataAsset(path);
        DataAsset<CombatTemporaryAnimatedSpriteData>.RegisterAsset(helper);
    }
}
