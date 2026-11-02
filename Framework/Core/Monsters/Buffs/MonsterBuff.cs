using CombatExtended.Framework.Data;
using CombatExtended.Framework.UI.Animations;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters.Buffs;

public class MonsterBuff
{

    public static readonly string MonsterModDataBuffsKey = $"{ModBindings.Id}_Buffs";

    /// <summary>
    /// Unique id for this monster buff
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// Name of this buff.
    /// </summary>
    public string DisplayName { get; set; }
    
    /// <summary>
    /// Description for this buff.
    /// </summary>
    public string Description { get; set; }
    
    public int DurationTicks { get; private set; }
    
    /// <summary>
    /// Whether this buff should be considered a debuff.
    /// </summary>
    public bool IsDebuff { get; set; }
    
    public int TicksElapsed { get; set; }

    /// <summary>
    /// How often should be buff effect triggered in ticks.
    /// <remarks>0 means buff has no additional effects.</remarks>
    /// </summary>
    public int DoEffectEveryTick { get; set; }
    
    protected int EffectTimesTriggered { get; set; }
    
    /// <summary>
    /// For animations that may not be properly implemented in <see cref="HandleApplied"/>.
    /// <remarks>0 means buff has no such animation.</remarks>
    /// </summary>
    public int DoAnimationEveryTick { get; set; }
    
    public string? TemporaryAnimatedSprite { get; set; }
    
    public MonsterBuffAttributes Attributes { get; set; }
    
    public MonsterBuffAttributes EffectAttributes { get; set; }
    
    /// <summary>
    /// Character that applied this buff.
    /// </summary>
    public Character? AppliedBy { get; set; }
    
    public bool IsDurationEndHandled { get; set; }
    
    protected readonly MonsterBuffData? Data;
    
    protected readonly List<TemporaryAnimatedSprite> TasList = new ();
    
    protected MonsterBuff(
        string id,
        string displayName,
        string description,
        float duration,
        bool isDebuff,
        MonsterBuffAttributes? attributes = null,
        MonsterBuffAttributes? effectAttributes = null,
        int doEffectEveryTick = 0,
        int doAnimationEveryTick = 0,
        string? temporaryAnimatedSprite = null)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        DurationTicks = duration.ToTicks();
        IsDebuff = isDebuff;
        Attributes = attributes ?? MonsterBuffAttributes.CreateEmpty();
        EffectAttributes = effectAttributes ?? MonsterBuffAttributes.CreateEmpty();
        DoEffectEveryTick = doEffectEveryTick;
        DoAnimationEveryTick = doAnimationEveryTick;
        TemporaryAnimatedSprite = temporaryAnimatedSprite;
        TicksElapsed = 0;
    }

    protected MonsterBuff(string id, MonsterBuffData data)
    {
        Id = id;
        DisplayName = data.DisplayName;
        Description = data.Description;
        DurationTicks = data.Duration.ToTicks();
        IsDebuff = data.IsDebuff;
        
        if (data.Attributes is not null)
            Attributes = MonsterBuffAttributes.Create(data.Attributes);
        else Attributes = MonsterBuffAttributes.CreateEmpty();
        if (data.Effects is not null)
            EffectAttributes = MonsterBuffAttributes.Create(data.Effects);
        else EffectAttributes = MonsterBuffAttributes.CreateEmpty();
        
        DoEffectEveryTick = data.DoEffectEverySec?.ToTicks() ?? 0;
        DoAnimationEveryTick = data.DoAnimationEverySec?.ToTicks() ?? 0;
        if (data.TemporaryAnimatedSprite is not null)
        {
            if (!CombatTemporaryAnimatedSpriteDataAsset.Data.ContainsKey(data.TemporaryAnimatedSprite)) 
                Log.Error($"Unknown temporary animated sprite {TemporaryAnimatedSprite}");
        }
        TemporaryAnimatedSprite = data.TemporaryAnimatedSprite;
        Data = data;
    }

    /// <summary>
    /// Raised when buff is applied.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <param name="location">Monster location.</param>
    public void HandleApplied(Monster monster, GameLocation location)
    {
        AddBuffToMonster(monster);
        IsDurationEndHandled = false;
        monster.ApplyMonsterBuffAttributes(Attributes);
        OnApplied(monster, location);
    }

    protected virtual void OnApplied(Monster monster, GameLocation location) {}
    
    public virtual void OnRenewed(Monster monster, GameLocation location) {}

    /// <summary>
    /// Do some effect every <see cref="MonsterBuff.DoEffectEveryTick"/> ticks.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <param name="location">Monster location.</param>
    /// <param name="tick">Current tick.</param>
    public virtual void DoEffect(Monster monster, GameLocation location, uint tick)
    {
        monster.ApplyMonsterBuffAttributes(EffectAttributes);
        EffectTimesTriggered++;
    }

    /// <summary>
    /// Do some animations every <see cref="MonsterBuff.DoAnimationEveryTick"/> ticks.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <param name="location">Monster location.</param>
    /// <param name="tick">Current tick.</param>
    public virtual void DoAnimation(Monster monster, GameLocation location, uint tick)
    {
        if (DoAnimationEveryTick <= 0 || TemporaryAnimatedSprite is null ||
            !CombatTemporaryAnimatedSpriteDataAsset.Data.TryGetValue(TemporaryAnimatedSprite, out var data)) return;
        var monsterX = monster.Position.X;
        var monsterY = monster.Position.Y;
        var position = new Vector2(monsterX, monsterY);
        if (data.AnimationSpec is not null)
        {
            switch (data.AnimationSpec.SpawnArea)
            {
                case "Top":
                    if (data.AnimationSpec.Type == "Random")
                    {
                        position.X = Game1.random.Next((int)monsterX, (int)monsterX + monster.Sprite.getWidth() * 4);
                        position.Y = Game1.random.Next((int)monsterY, (int)monsterY + monster.Sprite.getHeight() * 2);
                    }
                    else position = monster.GetApproximatePositionOnTop();
                    break;
                case "Bottom":
                    if (data.AnimationSpec.Type == "Random")
                    {
                        position.X = Game1.random.Next((int)monsterX, (int)monsterX + monster.Sprite.getWidth() * 4);
                        position.Y = Game1.random.Next((int)monsterY + monster.Sprite.getHeight() * 2, (int)monsterY + monster.Sprite.getHeight() * 4);
                    }
                    else position = monster.GetApproximatePositionOnBottom();
                    break;
                case "Full":
                    if (data.AnimationSpec.Type == "Random")
                    {
                        position.X = Game1.random.Next((int)monsterX, (int)monsterX + monster.Sprite.getWidth() * 4);
                        position.Y = Game1.random.Next((int)monsterY, (int)monsterY + monster.Sprite.getHeight() * 4);
                    }
                    break;
            }
        }
        var tas = data.AttachedToMonster
            ? new AttachedTextureTemporaryAnimatedSprite(
                texturePath: data.Texture,
                sourceRect: data.SourceRect,
                animationInterval: data.Interval,
                animationLength: data.Frames,
                numberOfLoops: data.Loops,
                position: position,
                layerDepth: 0f,
                color: Color.White,
                attachTo: monster,
                scale: data.Scale,
                alphaFade: data.AlphaFade
            )
            : new TextureTemporaryAnimatedSprite(
                texturePath: data.Texture,
                sourceRect: data.SourceRect,
                animationInterval: data.Interval,
                animationLength: data.Frames,
                numberOfLoops: data.Loops,
                position: position,
                layerDepth: 0f,
                color: Color.White,
                scale: data.Scale,
                alphaFade: data.AlphaFade
            );
        if (data.AnimationSpec is not null)
        {
            if (data.AnimationSpec.Movement is not null)
            {
                var time = data.Interval / 1000 * data.Frames * data.Loops * 64;
                var motion = data.AnimationSpec.Movement switch
                {
                    "Down" => new Vector2(0f, (monsterY + monster.Sprite.getHeight() - position.Y) / time),
                    "Up" => new Vector2(0f, -(monsterY + monster.Sprite.getHeight() - position.Y) / time),
                    _ => Vector2.Zero
                };
                tas.motion = motion;
            }
        }
        TasList.Add(tas);
        location.temporarySprites.Add(tas);
    }

    /// <summary>
    /// End this monster buff.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <param name="location">Monster location.</param>
    public void End(Monster monster, GameLocation location)
    {
        RemoveBuffFromMonster(monster);
        IsDurationEndHandled = true;
        monster.RemoveMonsterBuffAttributes(Attributes);
        for (var i = 0; i < EffectTimesTriggered; i++)
        {
            monster.RemoveMonsterBuffAttributes(EffectAttributes);
        }
        OnEnd(monster, location);
    }

    /// <summary>
    /// Raised when buff expires.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <param name="location">Monster location.</param>
    protected virtual void OnEnd(Monster monster, GameLocation location) {}

    /// <summary>
    /// Raised when monster with this buff dies.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <param name="location">Monster location.</param>
    public virtual void OnMonsterDeath(Monster monster, GameLocation location) {}

    /// <summary>
    /// Used after <see cref="OnMonsterDeath"/> or <see cref="End"/> to stop buff animations.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    /// <param name="location">Monster location.</param>
    public virtual void StopAnimating(Monster monster, GameLocation location)
    {
        foreach (var temporaryAnimatedSprite in TasList)
        {
            location.temporarySprites.Remove(temporaryAnimatedSprite);
        }
    }

    /// <summary>
    /// Add buff into <paramref name="monster"/> modData.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    protected virtual void AddBuffToMonster(Monster monster)
    {
        if (!monster.modData.TryGetValue(MonsterModDataBuffsKey, out var buffsStr))
        {
            monster.modData[MonsterModDataBuffsKey] = Id;
            return;
        }
        // this is kinda eh
        var buffs = buffsStr.Split(' ');
        var index = buffs.Length;
        Array.Resize(ref buffs, buffs.Length + 1);
        buffs[index] = Id;
        monster.modData[MonsterModDataBuffsKey] = string.Join(' ', buffs);
    }

    /// <summary>
    /// Removes buff from <paramref name="monster"/> modData.
    /// </summary>
    /// <param name="monster">Monster instance.</param>
    protected virtual void RemoveBuffFromMonster(Monster monster)
    {
        if (!monster.modData.TryGetValue(MonsterModDataBuffsKey, out var buffsStr))
        {
            // should be impossible but sure
            return;
        }
        // this is also eh
        var buffs = buffsStr.Split(' ').ToList();
        buffs.RemoveWhere(buffId => buffId == Id);
        monster.modData[MonsterModDataBuffsKey] = string.Join(' ', buffs);
    } 

    /// <summary>
    /// Create <see cref="MonsterBuff"/> instance based on buff id.
    /// </summary>
    /// <param name="buffId"><see cref="MonsterBuff.Id"/></param>
    /// <param name="durationSecondsOverride">Overriden duration of buff.</param>
    /// <returns>Created instance of monster buff.</returns>
    /// <exception cref="ArgumentException">Buff id is unknown.</exception>
    public static MonsterBuff Create(string buffId, float durationSecondsOverride = 0)
    {
        if (CustomMonsterBuffFactories.TryGetValue(buffId, out var factory))
            return factory();
        if (!MonsterBuffDataAsset.Data.TryGetValue(buffId, out var data))
            throw new ArgumentException($"Unknown monster buff ID {buffId}");
        if (CustomDataMonsterBuffFactories.TryGetValue(buffId, out var dataFactory))
            return dataFactory(data);
        var buff = data.Type switch
        {
            StackableMonsterBuff.StackableMonsterBuffType => StackableMonsterBuff.Create(buffId, data),
            _ => new MonsterBuff(buffId, data)
        };
        if (durationSecondsOverride > 0) buff.DurationTicks = durationSecondsOverride.ToTicks();
        return buff;
    }
    
    private static readonly Dictionary<string, Func<MonsterBuff>> CustomMonsterBuffFactories = new ();
    private static readonly Dictionary<string, Func<MonsterBuffData, MonsterBuff>> CustomDataMonsterBuffFactories = new ();

    public static void RegisterCustom(string abilityId, Func<MonsterBuff> factory)
    {
        CustomMonsterBuffFactories[abilityId] = factory;
    }
    
    public static void RegisterCustom(string abilityId, Func<MonsterBuffData, MonsterBuff> factory)
    {
        CustomDataMonsterBuffFactories[abilityId] = factory;
    }
}
