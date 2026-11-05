using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Core.Monsters.Tags;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using CombatExtended.Integrations.Compatibility.Tags;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters;

/// <summary>
/// Contains logic for enabling monster extensions.
/// </summary>
[Registrable]
public static class MonsterExtensionEnabler
{

    private static int _latestAssignedMonsterId;
    
    private static readonly string MonsterExtensionsAppliedModDataKey = "ExtensionsApplied".PrependId();
    private static readonly string MonsterIdModDataKey = "Id".PrependId();

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
        if (monster.modData.ContainsKey(MonsterExtensionsAppliedModDataKey)) return;
        var resistance = new CombinedResistance();
        var tags = monster.GetTags().WithCompatibility(monster);
        foreach (var tagData in TagDataAsset.Data.Values)
        {
            if (tagData.Condition is not null && GameStateQuery.CheckConditions(tagData.Condition, location))
                tags.Add(Tag.MapToTag(tagData));
        }
        tags.Sort((tag1, tag2) => tag1.Priority.CompareTo(tag2.Priority));
        resistance.ApplyTags(tags);
        if (Config.NoImmunities)
            resistance.NoImmunities();
        monster.UpdateResistance(resistance);
        var ac = monster.GetArmourClass().WithCompatibility(monster);
        monster.AddArmourClass(ac);
        if (monster.modData.ContainsKey(MonsterExtensionsAppliedModDataKey)) return;
        monster.modData[MonsterExtensionsAppliedModDataKey] = "true";
        monster.modData[MonsterIdModDataKey] = $"{Game1.player.UniqueMultiplayerID}_{GetNextMonsterId()}";
    }
    #endregion
}
