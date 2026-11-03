using System.Runtime.CompilerServices;
using CombatExtended.Framework.Core.Monsters.Buffs;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.Util;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.Managers.MonsterBuffs;

/// <summary>
/// <see cref="IMonsterBuffManager"/> implementation for host player.
/// </summary>
public class HostMonsterBuffManager : IMonsterBuffManager
{
    /// <summary>
    /// How often should be buff dictionary updated.
    /// </summary>
    private const int BaseTicks = 10;
    /// <summary>
    /// How often should be buff list replaced with copy without expired buffs or removed if there are no active buffs.
    /// </summary>
    private const int TicksForBuffListUpdating = 120;
    
    /// <summary>
    /// Single source of truth for all monster buffs.
    /// <remarks>Key is <see cref="GameLocation.Name"/>.</remarks>
    /// <remarks>Value is a dictionary of all tracked monsters with any buff applied.</remarks>
    /// </summary>
    private readonly ConditionalWeakTable<Monster, Dictionary<string, MonsterBuff>> _buffs = new();
    
    /// <summary>
    /// Cache for <see cref="MonsterBuff.DoEffectEveryTick"/>, adjusted for base ticks amount and converted to uint.
    /// </summary>
    private readonly Dictionary<string, uint> _doEffectUintTicks = new();
    /// <summary>
    /// Cache for <see cref="MonsterBuff.DoAnimationEveryTick"/>, adjusted for base ticks amount and converted to uint.
    /// </summary>
    private readonly Dictionary<string, uint> _doAnimationUintTicks = new();

    public void Register(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked += OnUpdateTicked_UpdateBuffDictionary;
        helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived_CheckBuffRequest;
    }

    public void Unregister(IModHelper helper)
    {
        helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked_UpdateBuffDictionary;
        helper.Events.Multiplayer.ModMessageReceived -= OnMessageReceived_CheckBuffRequest;
    }

    [HarmonyPatch]
    public void Monster_takeDamage_Postfix(Monster __instance)
    {
        if (__instance.Health > 0 || !_buffs.TryGetValue(__instance, out var buffs)) return;
        foreach (var monsterBuff in buffs.Values)
        {
            monsterBuff.OnMonsterDeath(__instance, __instance.currentLocation);
            monsterBuff.StopAnimating(__instance, __instance.currentLocation);
        }
        _buffs.Remove(__instance);
    }

    /// <summary>
    /// Do buff update logic.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">See <see cref="UpdateTickedEventArgs"/>.</param>
    private void OnUpdateTicked_UpdateBuffDictionary(object? sender, UpdateTickedEventArgs e)
    {
        if (!e.IsMultipleOf(BaseTicks) || !Context.IsPlayerFree || !Context.IsMainPlayer) return;
        foreach (var (monster, monsterBuffs) in _buffs)
        {
            if (monster.Health <= 0) continue;
            foreach (var (monsterBuffId, monsterBuff) in monsterBuffs)
            {
                if (monsterBuff.TicksElapsed >= monsterBuff.DurationTicks)
                {
                    if (monsterBuff.IsDurationEndHandled) continue;
                    monsterBuff.End(monster, monster.currentLocation);
                    monsterBuff.StopAnimating(monster, monster.currentLocation);
                    HandleMonsterBuffTrigger(Triggers.MonsterBuffExpired, monsterBuff, monster, monster.currentLocation);
                    //RaiseMonsterBuffTrigger(Triggers.MonsterBuffExpired, monsterBuffId, monster, monster.currentLocation);
                    continue;
                }

                if (monsterBuff.DoEffectEveryTick > 0 && e.IsMultipleOf(_doEffectUintTicks[monsterBuffId]))
                {
                    monsterBuff.DoEffect(monster, monster.currentLocation, e.Ticks);
                    HandleMonsterBuffTrigger(Triggers.MonsterBuffContinuousEffect, monsterBuff, monster, monster.currentLocation);
                }
                if (monsterBuff.DoAnimationEveryTick > 0 && e.IsMultipleOf(_doAnimationUintTicks[monsterBuffId]))
                {
                    monsterBuff.DoAnimation(monster, monster.currentLocation, e.Ticks);
                }

                // tick elapsing
                monsterBuff.TicksElapsed += BaseTicks;
                switch (monsterBuff)
                {
                    case IndependentlyStackableMonsterBuff independent:
                        var stacks = 0;
                        for (var node = independent.StackTimers.First; node is not null; node = node.Next)
                        {
                            if (node.Value > 0) stacks++;
                            node.Value -= BaseTicks;
                        }
                        if (independent.Stacks == stacks) continue;
                        var expired = stacks - independent.Stacks;
                        independent.OnIndependentStacksExpired(monster, expired);
                        break;
                }
            }
        }
        if (!e.IsMultipleOf(TicksForBuffListUpdating)) return;
        foreach (var (monster, monsterBuffs) in _buffs)
        {
            var modified = new Dictionary<string, MonsterBuff>();
            foreach (var (monsterBuffId, monsterBuff) in monsterBuffs)
            {
                if (!monsterBuff.IsDurationEndHandled) 
                    modified.Add(monsterBuffId, monsterBuff);
            }
            if (modified.Values.Any())
                _buffs.AddOrUpdate(monster, modified);
            else _buffs.Remove(monster);
        }
    }

    private static void HandleMonsterBuffTrigger(string trigger, MonsterBuff monsterBuff, Monster monster,
        GameLocation location)
    {
        if (monsterBuff.AppliedBy is Farmer player && player.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
        {
            var message = new MonsterBuffTriggerMessage(trigger, monster, location, monsterBuff.Id);
            Helper.Multiplayer.SendMessage(message, Messages.MonsterBuffTriggerMessageType, 
                new []{ Id }, new []{ player.UniqueMultiplayerID });
        }
        else Utility.RaiseMonsterBuffTrigger(trigger, monsterBuff.Id, monster, location);
    }

    /// <summary>
    /// Receive messages from farmhands with buff requests.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e"><see cref="ModMessageReceivedEventArgs"/>.</param>
    private void OnMessageReceived_CheckBuffRequest(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != Id || !Context.IsOnHostComputer) return;
        if (e.Type == Messages.ApplyBuffRequestMessageType)
        {
            Log.Trace($"Received {e.Type} message");
            var request = e.ReadAs<BuffRequest>();
            if (Utility.TryFindMonster(request.MonsterId, request.LocationName, out var monster))
            {
                ApplyBuffForFarmhandRequest(monster, Game1.getLocationFromName(request.LocationName), request.BuffId, request.Applier, request.ApplierId);
            }
        }
        if (e.Type == Messages.RemoveBuffRequestMessageType)
        {
            Log.Trace($"Received {e.Type} message");
            var request = e.ReadAs<BuffRequest>();
            if (Utility.TryFindMonster(request.MonsterId, request.LocationName, out var monster))
            {
                RemoveBuffForFarmhandRequest(monster, Game1.getLocationFromName(request.LocationName), request.BuffId);
            }
        }
    }

    public void ApplyBuff(Monster monster, GameLocation loc, string buffId, string? applier = null, string? applierId = null)
    {
        ApplyBuffNoTriggerRaised(monster, loc, buffId, applier, applierId);
        Utility.RaiseMonsterBuffTrigger(Triggers.MonsterBuffApplied, buffId, monster, loc);
    }

    public void RemoveBuff(Monster monster, GameLocation loc, string buffId)
    {
        var buffs =  _buffs.GetValue(monster, _ => new Dictionary<string, MonsterBuff>());
        if (buffs.TryGetValue(buffId, out var existingBuff))
        {
            // mark for removal
            existingBuff.TicksElapsed = existingBuff.DurationTicks;
            Log.Trace($"Removed buff {buffId} from Monster {monster.Name}_{monster.GetCharacterId()} on location {loc.Name}");
            Utility.RaiseMonsterBuffTrigger(Triggers.MonsterBuffRemoved, existingBuff.Id, monster, loc);
        }
    }

    private void RemoveBuffForFarmhandRequest(Monster monster, GameLocation loc, string buffId)
    {
        var buffs =  _buffs.GetValue(monster, _ => new Dictionary<string, MonsterBuff>());
        if (buffs.TryGetValue(buffId, out var existingBuff))
        {
            // mark for removal
            existingBuff.TicksElapsed = existingBuff.DurationTicks;
            Log.Trace($"Removed buff {buffId} from Monster {monster.Name}_{monster.GetCharacterId()} on location {loc.Name}");
        }
    }
    
    
    private void ApplyBuffForFarmhandRequest(Monster monster, GameLocation loc, string buffId, string? applier = null,
        string? applierId = null)
    {
        ApplyBuffNoTriggerRaised(monster, loc, buffId, applier, applierId);
    }

    private void ApplyBuffNoTriggerRaised(Monster monster, GameLocation loc, string buffId, string? applier = null,
        string? applierId = null)
    {
        var buffs =  _buffs.GetValue(monster, _ => new Dictionary<string, MonsterBuff>());
        if (buffs.TryGetValue(buffId, out var existingBuff) && !existingBuff.IsDurationEndHandled)
        {
            // check stacks and update
            if (existingBuff is StackableMonsterBuff stackable)
            {
                stackable.ApplyAdditionalStack(monster, loc);
            }

            existingBuff.TicksElapsed = 0;
            existingBuff.OnRenewed(monster, loc);
            Log.Trace($"Updated buff {buffId} for Monster {monster.Name}_{monster.GetCharacterId()} on location {loc.Name}");
        }
        else
        {
            var buff = MonsterBuff.Create(buffId);
            if (applier is not null && applierId is not null)
            {
                Character? charApplier = applier switch
                {
                    "player" => Game1.GetPlayer(long.Parse(applierId)),
                    "monster" => loc.characters.FirstOrDefault(npc => npc is Monster mon && mon.GetCharacterId() == applierId),
                    _ => throw new ArgumentException($"Unknown buff applier type {applier}")
                };
                buff.AppliedBy = charApplier;
            }
            buffs[buffId] = buff;
            Log.Trace($"Applied buff {buffId} for Monster {monster.Name}_{monster.GetCharacterId()} on location {loc.Name}");
            buff.HandleApplied(monster, loc);
            // cache to not calculate the same uint tick amount that accounts for BaseTicks divisibility
            if (buff.DoEffectEveryTick > 0)
                _doEffectUintTicks[buffId] = (uint) Math.Max(BaseTicks, buff.DoEffectEveryTick / BaseTicks * BaseTicks);
            if (buff.DoAnimationEveryTick > 0)
                _doAnimationUintTicks[buffId] = (uint) Math.Max(BaseTicks, buff.DoAnimationEveryTick / BaseTicks * BaseTicks);
        }
    }
}
