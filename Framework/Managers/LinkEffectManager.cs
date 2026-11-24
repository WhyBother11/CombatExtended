using CombatExtended.Framework.Core.Misc.Projectiles;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.UI.Animations;
using CombatExtended.Framework.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Managers;

[Registrable]
public class LinkEffectManager
{
    public static LinkEffectManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<LinkEffectManager> PerScreenInstance = new(()  => new LinkEffectManager());
    
    private const int BaseTicks = 20;
    
    public readonly Dictionary<object, Dictionary<string, LinkEffect>> Links = new();
    
    private readonly Dictionary<string, int> _doEffectAdjustedTicks = new();
    private readonly Dictionary<string, int> _doTouchEffectAdjustedTicks = new();

    public static void Register(IModHelper helper)
    {
        PerScreenInstance.Value = new LinkEffectManager();
        helper.Events.GameLoop.UpdateTicked += PerScreenInstance.Value.OnUpdateTicked;
    }
    
    public void BeginProjectileLink(CustomProjectile projectile, string linkId, LinkEffectData data)
    {
        var link = new ProjectileLinkEffect(projectile, linkId, data);
        projectile.AddDestroyListener((_, _) => { Links.Remove(projectile); });
        
        _doTouchEffectAdjustedTicks[linkId] = Math.Max(BaseTicks, link.DoMonsterTouchEffectEverySec / BaseTicks * BaseTicks);
        if (!Links.TryAdd(projectile, new Dictionary<string, LinkEffect> { { linkId, link } })) 
            Links[projectile][linkId] = link;
    }

    public void BeginMonsterLink(Monster monster, string linkId, LinkEffectData data)
    {
        var link = new MonsterLinkEffect(monster, linkId, data);
        _doEffectAdjustedTicks[linkId] = Math.Max(BaseTicks, link.DoMonsterEffectEverySec / BaseTicks * BaseTicks);
        _doTouchEffectAdjustedTicks[linkId] = Math.Max(BaseTicks, link.DoMonsterTouchEffectEverySec / BaseTicks * BaseTicks);
        if (!Links.TryAdd(monster, new Dictionary<string, LinkEffect> { { linkId, link } }))
        {
            var index = Links.Count % 5;
            var offset = index switch
            {
                <= 0 => Vector2.Zero,
                1 => new Vector2(-monster.GetBoundingBox().Width / 4f, 0),
                2 => new Vector2(0, -monster.GetBoundingBox().Height / 4f),
                3 => new Vector2(monster.GetBoundingBox().Width / 4f, 0),
                >= 4 => new Vector2(0, monster.GetBoundingBox().Height / 4f),
            };
            link.Offset = offset;
            Links[monster][linkId] = link;
        }
    }
    
    public void Remove(object target) => Links.Remove(target);

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks)) return;
        foreach (var (entity, links) in Links)
        {
            if (entity is not Monster monster) continue;
            foreach (var (_, link) in links)
            {
                link.TicksElapsed += BaseTicks;
                if (_doEffectAdjustedTicks.TryGetValue(link.Id, out var adjustedTicks))
                {
                    if (link.TicksElapsed % adjustedTicks == 0)
                        RaiseLinkEffectTrigger(Triggers.LinkedMonsterEffect, link.Id, monster);
                }
            }
        }
    }

    private void RaiseLinkEffectTrigger(string trigger, string linkId, Monster monster)
    {
        var item = ItemRegistry.Create(linkId);
        item.PackMonsterInfo(monster);
        TriggerActionManager.Raise(trigger, new object[]{ monster }, monster.currentLocation, Game1.player, item);
    }

    public static IEnumerable<KeyValuePair<int, LinkEffectManager>> GetAll() => PerScreenInstance.GetActiveValues();
}
