using System.Diagnostics.CodeAnalysis;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters.Statuses;

public class MonsterStatus
{
    public static readonly string MonsterModDataStatusesKey = $"{ModBindings.Id}_Statuses";
    
    /// <summary>
    /// Unique ID for this monster condition.
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// The name of this condition.
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    /// The description for this condition.
    /// </summary>
    public string Description { get; set; }
    
    public int TicksElapsed { get; set; }

    /// <summary>
    /// How often should be condition effect triggered in ticks.
    /// <remarks>0 means condition has no additional effects.</remarks>
    /// </summary>
    public int DoEffectEveryTick { get; set; }
    
    protected int EffectTimesTriggered { get; set; }
    
    public int DoAnimationEveryTick { get; set; }
    
    public List<string>? AnimationTemporaryAnimatedSprite { get; set; }
    
    public List<string>? TemporaryAnimatedSprites { get; set; }
    
    public MonsterStatusAttributes Attributes { get; set; }
    
    public MonsterStatusAttributes EffectAttributes { get; set; }
    
    protected readonly List<TemporaryAnimatedSprite> TasList = new ();

    protected MonsterStatus(string id, MonsterStatusData data)
    {
        Id = id;
        DisplayName = data.DisplayName;
        Description = data.Description;
        
        if (data.Attributes is not null)
            Attributes = MonsterStatusAttributes.Create(data.Attributes);
        else Attributes = MonsterStatusAttributes.CreateEmpty();
        if (data.Effects is not null)
            EffectAttributes = MonsterStatusAttributes.Create(data.Effects);
        else EffectAttributes = MonsterStatusAttributes.CreateEmpty();
        
        DoEffectEveryTick = data.DoEffectEverySec.ToTicks();
        DoAnimationEveryTick = data.DoAnimationEverySec.ToTicks();
        AnimationTemporaryAnimatedSprite = data.AnimationTemporaryAnimatedSprites;
        TemporaryAnimatedSprites = data.TemporaryAnimatedSprites;
    }

    public virtual void Apply(Monster monster, GameLocation location)
    {
        AddStatusToMonster(monster);
        monster.ApplyMonsterStatusAttributes(Attributes);
        if (TemporaryAnimatedSprites is not null)
        {
            foreach (var tas in TemporaryAnimatedSprites)
            {
                //if (CombatTemporaryAnimatedSpriteDataAsset.Data.TryGetValue(tas, out var data))
                //    StartTemporaryAnimatedSprite(monster, location, data);
                //else Log.Warn($"Unknown temporary animated sprite {tas}");
            }
        }
    }
    
    /// <summary>
    /// Remove this monster status.
    /// </summary>
    /// <param name="monster">The monster instance.</param>
    /// <param name="location">The monster location.</param>
    public virtual void Remove(Monster monster, GameLocation location)
    {
        RemoveStatusFromMonster(monster);
        monster.RemoveMonsterStatusAttributes(Attributes);
        for (var i = 0; i < EffectTimesTriggered; i++)
        {
            monster.RemoveMonsterStatusAttributes(EffectAttributes);
        }
    }
    
    public virtual void DoEffect(Monster monster, GameLocation location)
    {
        monster.ApplyMonsterStatusAttributes(EffectAttributes);
        EffectTimesTriggered++;
    }
    
    protected virtual void AddStatusToMonster(Monster monster)
    {
        if (!monster.modData.TryGetValue(MonsterModDataStatusesKey, out var statusesStr))
        {
            monster.modData[MonsterModDataStatusesKey] = Id;
            return;
        }
        
        var statuses = statusesStr.Split(' ');
        var index = statuses.Length;
        Array.Resize(ref statuses, statuses.Length + 1);
        statuses[index] = Id;
        monster.modData[MonsterModDataStatusesKey] = string.Join(' ', statuses);
    }

    protected virtual void RemoveStatusFromMonster(Monster monster)
    {
        if (!monster.modData.TryGetValue(MonsterModDataStatusesKey, out var statusesStr))
        {
            // should be impossible but sure
            return;
        }
        var statuses = statusesStr.Split(' ').ToList();
        statuses.RemoveWhere(buffId => buffId == Id);
        monster.modData[MonsterModDataStatusesKey] = string.Join(' ', statuses);
    }

    public static MonsterStatus Create(string id)
    {
        if (!MonsterStatusDataAsset.Data.TryGetValue(id, out var data))
            throw new ArgumentException($"Unknown monster status ID {id}");
        return new MonsterStatus(id, data);
    }
    
    public static List<MonsterStatus> GetMonsterStatuses(Monster monster)
    {
        return GetMonsterStatuses(monster.Name);
    }

    public static List<MonsterStatus> GetMonsterStatuses(string monsterName)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterName) || MonsterExtensionDataAsset.Data[monsterName].Statuses == null)
            return new List<MonsterStatus>();
        var monsterStatuses = ArgUtility.SplitBySpace(MonsterExtensionDataAsset.Statuses[monsterName]);
        return monsterStatuses
            .Select(id =>
            {
                if (string.IsNullOrEmpty(id)) return null;
                var res = GetMonsterStatus(id);
                if (res == null)
                {
                    Log.Warn($"Unknown monster status {id} for {monsterName} was ignored");
                }
                return res;
            })
            .OfType<MonsterStatus>()
            .ToList();
    }
    
    public static bool TryGetMonsterStatuses(string monsterId, [NotNullWhen(true)] out List<MonsterStatus>? monsterStatuses)
    {
        if (!MonsterExtensionDataAsset.Data.ContainsKey(monsterId) || MonsterExtensionDataAsset.Data[monsterId].Statuses == null)
        {
            monsterStatuses = null;
            return false;
        }

        monsterStatuses = GetMonsterStatuses(monsterId);
        return true;
    }
    
    private static MonsterStatus? GetMonsterStatus(string id)
    {
        if (MonsterStatusDataAsset.Data.ContainsKey(id))
        {
            return MonsterStatus.Create(id);
        }
        return null;
    }
}
