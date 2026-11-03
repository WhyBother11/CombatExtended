using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Managers.MonsterBuffs;

/// <summary>
/// Manager to hande monster buffs.
/// </summary>
public interface IMonsterBuffManager
{
    /// <summary>
    /// Register manager for event handling.
    /// </summary>
    /// <param name="helper">See <see cref="IModHelper"/>.</param>
    public void Register(IModHelper helper);
    
    /// <summary>
    /// Stop this manager's event handling.
    /// </summary>
    /// <param name="helper">See <see cref="IModHelper"/>.</param>
    public void Unregister(IModHelper helper);

    /// <summary>
    /// Apply given <paramref name="buffId"/> to <paramref name="monster"/> on behalf of <paramref name="applier"/> if any.
    /// </summary>
    /// <param name="monster">Monster to apply buff to.</param>
    /// <param name="loc">Monster location.</param>
    /// <param name="buffId">ID of buff to be given.</param>
    /// <param name="applier">Who applied this buff ('player' or 'monster').</param>
    /// <param name="applierId">ID of whoever applied this buff.</param>
    public void ApplyBuff(Monster monster, GameLocation loc, string buffId, string? applier = null, string? applierId = null);

    /// <summary>
    /// Remove given <paramref name="buffId"/> from <paramref name="monster"/>.
    /// </summary>
    /// <param name="monster">Monster to remove buff from.</param>
    /// <param name="loc">Monster location.</param>
    /// <param name="buffId">ID of buff to be removed.</param>
    public void RemoveBuff(Monster monster, GameLocation loc, string buffId);
}
