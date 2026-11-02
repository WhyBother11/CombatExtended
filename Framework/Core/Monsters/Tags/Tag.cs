using System.Diagnostics.CodeAnalysis;
using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Data;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters.Tags;

/// <summary>
/// Monster tag
/// </summary>
public class Tag
{
    /// <summary>
    /// Unique id for this tag.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Tag priority.
    /// <remarks>Tags with lesser priority will be applied first, allowing to override them for other tags.</remarks>
    /// </summary>
    public int Priority { get; set; } = -9999;

    /// <summary>
    /// <see cref="CombinedResistance"/> for that should be applied with this tag./>
    /// </summary>
    public CombinedResistance Resistance { get; set; } = new (new Dictionary<string, Resistance>());
    
    public static List<Tag> GetTags(Monster monster)
    {
        return GetTags(monster.Name);
    }

    public static List<Tag> GetTags(string monsterName)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterName) || MonsterExtensionDataAsset.Data[monsterName].Tags == null)
            return new List<Tag>();
        var tags = ArgUtility.SplitBySpace(MonsterExtensionDataAsset.Tags[monsterName]);
        return tags
            .Select(id =>
            {
                if (string.IsNullOrEmpty(id)) return null;
                var res = GetTag(id);
                if (res == null)
                {
                    Log.Warn($"Unknown tag {id} for {monsterName} was ignored");
                }
                return res;
            })
            .OfType<Tag>()
            .ToList();
    }
    
    public static bool TryGetTags(string monsterId, [NotNullWhen(true)] out List<Tag>? tags)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterId) || MonsterExtensionDataAsset.Data[monsterId].Tags == null)
        {
            tags = null;
            return false;
        }

        tags = GetTags(monsterId);
        return true;
    }
    
    private static Tag? GetTag(string id)
    {
        if (TagDataAsset.Data.TryGetValue(id, out var tagData))
        {
            return MapToTag(tagData);
        }
        return null;
    }

    private static Tag MapToTag(TagData tagData)
    {
        var tag = new Tag { Id = tagData.Id, Resistance = CombinedResistance.FromTag(tagData) };
        if (int.TryParse(tagData.Priority, out var priority)) tag.Priority = priority;
        return tag;
    }
}