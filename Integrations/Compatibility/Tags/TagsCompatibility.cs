using CombatExtended.Framework.Core.Monsters.Tags;
using StardewValley.Monsters;

namespace CombatExtended.Integrations.Compatibility.Tags;

public static class TagsCompatibility
{
    public static List<Tag> WithCompatibility(this List<Tag> tags, Monster monster)
    {
        var textureName = monster.Sprite.textureName.Value;
        return tags
            .FTM(textureName)
            .CustomMonsters(monster);
    }

    private static List<Tag> CustomMonsters(this List<Tag> tags, Monster monster)
    {
        if (Tag.TryGetTags(monster.Name, out var tagList))
        {
            return tagList;
        }

        return tags;
    }
    
    private static List<Tag> FTM(this List<Tag> tags, string textureName)
    {
        if (Tag.TryGetTags(textureName, out var tagList))
        {
            return tagList;
        }
        return tags;
    }
}