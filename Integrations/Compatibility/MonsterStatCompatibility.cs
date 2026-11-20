using StardewValley.Monsters;

namespace CombatExtended.Integrations.Compatibility;

public static class MonsterStatCompatibility
{
    /// <summary>
    /// Monster stat handler for custom monsters.
    /// </summary>
    /// <param name="stat">The stat coming only based on monster's name.</param>
    /// <param name="monster">The monster to check if it is custom and apply monster stat accordingly if there exists data for it.</param>
    /// <param name="statLookupCSharp">The stat lookup action to get it from custom data for monsters added with C#. This lookup uses monster's name.</param>
    /// <param name="statLookupFramework">The stat lookup action to get it from custom data for monsters added with frameworks. This lookup uses monster's texture name.</param>
    /// <param name="statFactory">The stat factory to get the actual value of stat. This will be used if either of <paramref name="statLookupCSharp"/> and <paramref name="statLookupFramework"/> return <c>true</c> as first value.</param>
    /// <typeparam name="TS">The type of monster stat.</typeparam>
    /// <returns><paramref name="statFactory"/> result if either of <paramref name="statLookupFramework"/> and <paramref name="statLookupCSharp"/> (overwrites framework lookup if that happens) returns <c>true</c>, <paramref name="stat"/> otherwise.</returns>
    public static TS WithCompatibility<TS>(this TS stat, Monster monster, Func<Monster, (bool, TS)> statLookupCSharp, 
        Func<string, (bool, TS)> statLookupFramework, Func<TS, TS> statFactory)
    {
        var textureName = monster.Sprite.textureName.Value;
        return stat
            .Framework(textureName, statLookupFramework, statFactory);
        //.CSharp(monster, statLookupCSharp, statFactory);
    }

    /*/// <summary>
    /// Monster stat handler for custom C# monsters.
    /// </summary>
    /// <param name="stat">The stat that should be handled for custom C# monsters.</param>
    /// <param name="monster">The custom monster.</param>
    /// <param name="statLookup">The stat lookup action to get it from custom data. The argument is <paramref name="monster"/>. Return first value is whether the stat data exists, the second one is the stat itself (will be used only if the first value is <c>true</c>).</param>
    /// <param name="statFactory">The stat factory to get the actual value of stat. Will be used if <paramref name="statLookup"/> returns <c>true</c> as first return value.</param>. The argument is second value from <paramref name="statLookup"/>, the return is the modified stat.
    /// <typeparam name="TS">The type of monster stat.</typeparam>
    /// <returns><paramref name="statFactory"/> result if <paramref name="statLookup"/> returns <c>true</c>, <paramref name="stat"/> otherwise.</returns>
    private static TS CSharp<TS>(this TS stat, Monster monster, Func<Monster, (bool, TS)> statLookup, 
        Func<TS, TS> statFactory)
    {
        var (exists, value) = statLookup.Invoke(monster);
        return exists ? statFactory.Invoke(value) : stat;
    }*/

    /// <summary>
    /// Monster stat handler for custom framework monsters.
    /// </summary>
    /// <param name="stat">The stat that should be handled for custom framework (FTM and SpaceCore) monsters.</param>
    /// <param name="textureName">The custom monster's texture, as it's the only way to distinguish custom monsters done with frameworks.</param>
    /// <param name="statLookup">The stat lookup action to get it from custom data. The argument is <paramref name="textureName"/>. Return first value is whether the stat data exists, the second one is the stat itself (will be used only if the first value is <c>true</c>).</param>
    /// <param name="statFactory">The stat factory to get the actual value of stat. Will be used if <paramref name="statLookup"/> returns <c>true</c> as first return value.</param>. The argument is second value from <paramref name="statLookup"/>, the return is the modified stat.
    /// <typeparam name="TS">The type of monster stat.</typeparam>
    /// <returns><paramref name="statFactory"/> result if <paramref name="statLookup"/> returns <c>true</c>, <paramref name="stat"/> otherwise.</returns>
    private static TS Framework<TS>(this TS stat, string textureName, Func<string, (bool, TS)> statLookup, 
        Func<TS, TS> statFactory)
    {
        var (exists, value) = statLookup.Invoke(textureName);
        return exists ? statFactory.Invoke(value) : stat;
    }
}
