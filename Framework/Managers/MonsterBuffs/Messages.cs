using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Managers.MonsterBuffs;

public static class Messages
{
    public static readonly string ApplyBuffRequestMessageType = "ApplyBuffRequest".PrependId();
    
    public static readonly string RemoveBuffRequestMessageType = "RemoveBuffRequest".PrependId();
    
    public static readonly string MonsterBuffTriggerMessageType = "MonsterBuffTrigger".PrependId();
}

public class BuffRequest
{
    
    public string MonsterId { get; set; }
    
    public string BuffId { get; set; }
    
    public string LocationName { get; set; }
    
    public string? Applier { get; set; }
    
    public string? ApplierId { get; set; }

    public BuffRequest(Monster monster, GameLocation loc, string buffId, string? applier = null, string? applierId = null)
    {
        MonsterId = monster.GetCharacterId();
        BuffId = buffId;
        LocationName = loc.Name;
        Applier = applier;
        ApplierId = applierId;
    }
}

/// <summary>
/// A message sent to specific farmhand player for monster buff related triggers.
/// </summary>
public class MonsterBuffTriggerMessage
{
    public string Trigger { get; set; }
    
    public string MonsterId { get; set; }
    
    public string BuffId { get; set; }
    
    public string LocationName { get; set; }

    public MonsterBuffTriggerMessage(string trigger, Monster monster, GameLocation loc, string buffId)
    {
        Trigger = trigger;
        MonsterId = monster.GetCharacterId();
        BuffId = buffId;
        LocationName = loc.Name;
    }
}
