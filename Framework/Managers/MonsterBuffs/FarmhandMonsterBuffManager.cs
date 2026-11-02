using CombatExtended.Framework.TriggersActionsGSQ.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.Managers.MonsterBuffs;

/// <summary>
/// <see cref="IMonsterBuffManager"/> implementation for farmhand players.
/// </summary>
public class FarmhandMonsterBuffManager : IMonsterBuffManager
{
    #nullable disable
    private IModHelper _helper;
    #nullable restore

    public void Register(IModHelper helper)
    {
        _helper = helper;
        helper.Events.Multiplayer.ModMessageReceived += OnMessageReceived_CheckHostMonsterBuffManagerMessages;
    }

    public void Unregister(IModHelper helper) { }

    public void ApplyBuff(Monster monster, GameLocation loc, string buffId, string? applier = null, string? applierId = null)
    {
        var request = new BuffRequest(monster, loc, buffId, applier, applierId);
        _helper.Multiplayer.SendMessage(request, Messages.ApplyBuffRequestMessageType, modIDs: new []{ Id });
        Utility.RaiseMonsterBuffTrigger(Triggers.MonsterBuffApplied, buffId, monster, loc);
    }

    public void RemoveBuff(Monster monster, GameLocation loc, string buffId)
    {
        var request = new BuffRequest(monster, loc, buffId);
        _helper.Multiplayer.SendMessage(request, Messages.ApplyBuffRequestMessageType, modIDs: new []{ Id });
        Utility.RaiseMonsterBuffTrigger(Triggers.MonsterBuffRemoved, buffId, monster, loc);
    }

    private static void OnMessageReceived_CheckHostMonsterBuffManagerMessages(object? sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != Id || Context.IsOnHostComputer) return;
        if (e.Type == Messages.MonsterBuffTriggerMessageType)
        {
            Log.Trace($"Received {e.Type} message");
            var message = e.ReadAs<MonsterBuffTriggerMessage>();
            var location = Game1.getLocationFromName(message.LocationName);
            if (Utility.TryFindMonster(message.MonsterId, message.LocationName, out var monster))
            {
                Utility.RaiseMonsterBuffTrigger(message.Trigger, message.BuffId, monster, location);
            }
        }
    }
}
