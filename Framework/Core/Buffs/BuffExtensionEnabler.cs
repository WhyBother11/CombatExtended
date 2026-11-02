using System.Runtime.CompilerServices;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace CombatExtended.Framework.Core.Buffs;

public static class BuffExtensionEnabler
{
    
    private static readonly PerScreen<ConditionalWeakTable<Buff, BuffExtension>> Extensions = new (
        () => new ConditionalWeakTable<Buff, BuffExtension>());

    /// <summary>
    /// Add armour class attached to applied <paramref name="buff"/>.
    /// </summary>
    /// <param name="buff">Applied buff.</param>
    /// <param name="armourClass">Armour class to be added.</param>
    public static void AddArmourClass(Buff buff, int armourClass)
    {
        if (Extensions.Value.TryGetValue(buff, out var extension))
            extension.ArmourClass += armourClass;
        else Extensions.Value.GetValue(buff, _ => new BuffExtension()).ArmourClass = armourClass;
    }
    
    /// <summary>
    /// Get armour class attached to given <paramref name="buff"/>.
    /// </summary>
    /// <param name="buff">Buff instance.</param>
    /// <returns>Buff's armour class.</returns>
    public static int GetArmourClass(Buff buff) => Extensions.Value.TryGetValue(buff, out var extension) ? extension.ArmourClass : 0;
    
    private class BuffExtension
    {
        public int ArmourClass;
    }
}
