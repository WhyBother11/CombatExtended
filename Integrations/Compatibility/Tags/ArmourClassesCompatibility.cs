using CombatExtended.Framework.Core.Damage;
using StardewValley.Monsters;

namespace CombatExtended.Integrations.Compatibility.Tags;

public static class ArmourClassesCompatibility
{
    public static int WithCompatibility(this int ac, Monster monster)
    {
        var textureName = monster.Sprite.textureName.Value;
        return ac
            .FTM(textureName)
            .CustomMonsters(monster);
    }

    private static int CustomMonsters(this int ac, Monster monster)
    {
        if (ArmourClass.TryGetMonsterExtraArmourClass(monster.Name, out var armourClass))
        {
            return ArmourClass.DefaultMonsterArmourClass + armourClass.Value;
        }

        return ac;
    }

    private static int FTM(this int ac, string textureName)
    {
        if (ArmourClass.TryGetMonsterExtraArmourClass(textureName, out var armourClass))
        {
            return ArmourClass.DefaultMonsterArmourClass + armourClass.Value;
        }
        return ac;
    }
}