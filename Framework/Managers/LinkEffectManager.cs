using System.Runtime.CompilerServices;
using CombatExtended.Framework.Core.Misc.LinkEffect;
using CombatExtended.Framework.Core.Misc.LinkEffect.Impl;
using CombatExtended.Framework.Core.Projectiles;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Managers;

public class LinkEffectManager
{
    public static LinkEffectManager Instance => PerScreenInstance.Value;
    private static readonly PerScreen<LinkEffectManager> PerScreenInstance = new(()  => new LinkEffectManager());
    
    private const int BaseTicks = 20;
    
    public readonly Dictionary<object, Dictionary<string, LinkEffect>> Links = new();
    
    private readonly ConditionalWeakTable<Monster, Dictionary<string, int>> _monsterTouchEffectTimers = new();
    
    private readonly Dictionary<string, int> _doEffectAdjustedTicks = new();

    public static void Register(IModHelper helper)
    {
        PerScreenInstance.Value = new LinkEffectManager();
        helper.Events.GameLoop.UpdateTicked += PerScreenInstance.Value.OnUpdateTicked;
        helper.Events.Player.Warped += PerScreenInstance.Value.OnWarped_ClearLinks;
    }

    public static void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= PerScreenInstance.Value.OnUpdateTicked;
        helper.Events.Player.Warped -= PerScreenInstance.Value.OnWarped_ClearLinks;
    }
    
    public void BeginProjectileLink(CustomProjectile projectile, string linkId, LinkEffectData data)
    {
        var link = new ProjectileLinkEffect(projectile, linkId, data);
        projectile.AddDestroyListener((_, _) => { Links.Remove(projectile); });
        
        if (!Links.TryAdd(projectile, new Dictionary<string, LinkEffect> { { linkId, link } })) 
            Links[projectile][linkId] = link;
    }

    public void BeginMonsterLink(Monster monster, string linkId, LinkEffectData data)
    {
        var link = new MonsterLinkEffect(monster, linkId, data);
        if (link.DoMonsterEffectEveryTick > 0) 
            _doEffectAdjustedTicks[linkId] = Math.Max(BaseTicks, link.DoMonsterEffectEveryTick / BaseTicks * BaseTicks);
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
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree) return;
        // checking for link effects on linked monsters
        foreach (var (entity, links) in Links)
        {
            if (entity is not Monster monster) continue;
            foreach (var (_, link) in links)
            {
                link.TicksElapsed += BaseTicks;
                if (_doEffectAdjustedTicks.TryGetValue(link.Id, out var adjustedTicks))
                {
                    if (link.TicksElapsed % adjustedTicks == 0)
                        RaiseLinkEffectTrigger(Triggers.LinkEffectTriggered, link.Id, monster);
                }
            }
        }
        if (!Links.Any()) return;
        // link touch effect elapsing
        foreach (var (_, timers) in _monsterTouchEffectTimers)
        {
            foreach (var (id, _) in timers)
            {
                timers[id] -= BaseTicks;
            }
        }
        // link touch effect triggering
        foreach (var monster in Game1.player.currentLocation.characters.OfType<Monster>())
        {
            foreach (var (entity, links) in Links)
            {
                if (entity == monster) continue;
                foreach (var (id, link) in links)
                {
                    if (link.DoMonsterTouchEffectEveryTick <= 0) continue;
                    var timers = _monsterTouchEffectTimers.GetValue(monster, _ => new Dictionary<string, int>());
                    if (timers.TryGetValue(id, out var timer) && timer > 0) continue;
                    if (link.GetHitBox()
                        .Any(rect =>
                        {
                            var box = Game1.GlobalToLocal(Game1.viewport, monster.GetBoundingBox());
                            return box.Intersects(rect);
                        }))
                    {
                        RaiseLinkEffectTrigger(Triggers.MonsterTouchedLink, link.Id, monster);
                        timers[id] = link.DoMonsterTouchEffectEveryTick;
                    }
                }
            }
        }
    }

    private void OnWarped_ClearLinks(object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer) return;
        Links.Clear();
        _monsterTouchEffectTimers.Clear();
    }

    private static void RaiseLinkEffectTrigger(string trigger, string linkId, Monster monster)
    {
        var item = ItemRegistry.Create(linkId);
        item.PackMonsterInfo(monster);
        TriggerActionManager.Raise(trigger, new object[]{ monster }, monster.currentLocation, Game1.player, item);
    }

    public static IEnumerable<KeyValuePair<int, LinkEffectManager>> GetAll() => PerScreenInstance.GetActiveValues();
}
