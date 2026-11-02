using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using CombatExtended.Integrations.Compatibility.Tags;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters;

/// <summary>
/// Contains logic for enabling monster extensions.
/// </summary>
[Registrable]
public static class MonsterExtensionEnabler
{

    private static int _latestAssignedMonsterId;

    public static void Register(IModHelper helper)
    {
        helper.Events.Player.Warped += OnWarped_ApplyMonsterExtensions;
        helper.Events.World.NpcListChanged += OnNpcListChanged_ApplyTags;
    }

    public static int GetNextMonsterId()
    {
        return _latestAssignedMonsterId++;
    }

    #region Events
    /// <summary>
    /// Applies monster extensions whenever local player warps. 
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">See <see cref="WarpedEventArgs"/></param>
    private static void OnWarped_ApplyMonsterExtensions(object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer) return;
        foreach (var npc in e.NewLocation.characters)
        {
            if (npc.IsMonster) ApplyMonsterExtensions((Monster) npc, e.NewLocation);
        }
    }

    /// <summary>
    /// Applies monster extensions to new monsters whenever they are added to player's location. 
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">See <see cref="NpcListChangedEventArgs"/></param>
    private static void OnNpcListChanged_ApplyTags(object? sender, NpcListChangedEventArgs e)
    {
        foreach (var npc in e.Added)
        {
            if (npc.IsMonster) ApplyMonsterExtensions((Monster) npc, e.Location);
        }
    }
    #endregion

    #region ApplyMonsterExtensions
    private static void ApplyMonsterExtensions(Monster monster, GameLocation location)
    {
        if (monster.modData.ContainsKey("ExtensionsApplied".PrependId())) return;
        var resistance = GetLocationResistance(location);
        var tags = monster.GetTags().WithCompatibility(monster);
        tags.Sort((tag1, tag2) => tag1.Priority.CompareTo(tag2.Priority));
        resistance.ApplyTags(tags);
        if (Config.NoImmunities)
            resistance.NoImmunities();
        monster.UpdateResistance(resistance);
        var ac = monster.GetArmourClass().WithCompatibility(monster);
        monster.AddArmourClass(ac);
        if (monster.modData.ContainsKey("ExtensionsApplied".PrependId())) return;
        monster.modData["ExtensionsApplied".PrependId()] = "true";
        monster.modData["Id".PrependId()] = $"{Game1.player.UniqueMultiplayerID}_{GetNextMonsterId()}";
    }

    /// <summary>
    /// Get default <see cref="CombinedResistance"/> based on <paramref name="location"/>.
    /// </summary>
    /// <param name="location">Current location.</param>
    /// <returns><see cref="CombinedResistance"/> with applicable resistances.</returns>
    private static CombinedResistance GetLocationResistance(GameLocation location)
    {
        switch (location)
        {
            case MineShaft mine:
                var level = mine.mineLevel;
                switch (level)
                {
                    case >= 40 and < 80:
                        return CombinedResistance.FromTag(TagDataAsset.Data["Mines40_80"]);
                    case >= 80 and < 120:
                        return CombinedResistance.FromTag(TagDataAsset.Data["Mines80_120"]);
                }
                break;
            case VolcanoDungeon:
                return CombinedResistance.FromTag(TagDataAsset.Data["VolcanoDungeon"]);
        }
        return new CombinedResistance();
    }
    #endregion
}
