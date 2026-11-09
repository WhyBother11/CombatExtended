using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI;

namespace CombatExtended.Framework.Data;

public class RangeTileData
{
    #nullable disable
    public string Id;
    
    public string Type;

    public string RelativeTo;
    #nullable restore

    [ContentSerializer(Optional = true)] 
    public List<Rectangle>? Rectangles;

    [ContentSerializer(Optional = true)] 
    public float? Radius;

    [ContentSerializer(Optional = true)]
    public List<Line>? Lines;

    [ContentSerializer(Optional = true)] 
    public List<CursorLine>? CursorLines;
    
    public class Line
    {
        #nullable disable
        public string Id;
        #nullable restore
        
        public int Angle;
        
        [ContentSerializer(Optional = true)]
        public int MaxLength = 30;

        [ContentSerializer(Optional = true)] 
        public Coordinate? Delta;
        
        [ContentSerializer(Optional = true)] 
        public int Width = 1;
    }
    
    public class Rectangle
    {
        #nullable disable
        public string Id;
        
        public Coordinate From;

        public Coordinate To;
        #nullable restore
    }
    
    public class Coordinate
    {
        public int X = 0;

        public int Y = 0;
    }
    
    public class CursorLine
    {
        #nullable disable
        public string Id;
        #nullable restore
        
        [ContentSerializer(Optional = true)] 
        public Coordinate? Delta;
        
        [ContentSerializer(Optional = true)]
        public int MaxLength = 30;

        [ContentSerializer(Optional = true)] 
        public int Width = 1;
    }
}

public class RangeData
{
    #nullable disable
    public List<RangeTileData> Tiles;
    #nullable restore
}

[DataAsset("Ranges")]
public class RangeDataAsset : DataAsset<RangeData>
{
    private RangeDataAsset(string path) : base(path) {}
    
    public static void RegisterAsset(IModHelper helper, string path)
    {
        _instance = new RangeDataAsset(path);
        DataAsset<RangeData>.RegisterAsset(helper);
    }
    
    public static bool Exists(string rangeId)
    {
        return Data.ContainsKey(rangeId);
    }

    /// <summary>
    /// Whether range tiles should not be cached by ability and instead recalculated whenever cursor or player position changes
    /// (as for now for any range containing CursorTiles property).
    /// </summary>
    /// <param name="rangeId">Range id.</param>
    /// <returns><c>true</c> if range is considered non-static, <c>false</c> otherwise.</returns>
    public static bool NonStaticRange(string rangeId)
    {
        if (!Exists(rangeId)) return true;
        return Data[rangeId].Tiles.Any(data => data.CursorLines is not null);
    }
}
