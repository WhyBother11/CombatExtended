using CombatExtended.Framework.Data;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters.Buffs;


public class StackableMonsterBuff : MonsterBuff
{
    
    public const string StackableMonsterBuffType = "Stackable";
    
    /// <summary>
    /// Current buff stacks amount.
    /// </summary>
    public int Stacks { get; private set; }
    
    /// <summary>
    /// Maximum amount of stacks possible.
    /// </summary>
    public int MaxStacks { get; private set; }
    
    public MonsterBuffAttributes AttributesPerStack { get; set; }
    
    public MonsterBuffAttributes EffectsPerStack { get; set; }

    protected StackableMonsterBuff(
        string id,
        string displayName,
        string description,
        float duration,
        bool isDebuff,
        int maxStacks,
        MonsterBuffAttributes? attributes = null,
        MonsterBuffAttributes? effectAttributes = null,
        int doEffectEveryTick = 0,
        int doAnimationEveryTick = 0,
        MonsterBuffAttributes? attributesPerStack = null,
        MonsterBuffAttributes? effectAttributesPerStack = null
    ) : base(id, displayName, description, duration, isDebuff, attributes, effectAttributes, doEffectEveryTick, doAnimationEveryTick)
    {
        MaxStacks = maxStacks;
        AttributesPerStack = attributesPerStack ?? new MonsterBuffAttributes();
        EffectsPerStack = effectAttributesPerStack ?? new MonsterBuffAttributes();
    }

    protected StackableMonsterBuff(string id, MonsterBuffData data) : base(id, data)
    {
        if (data.StackableData is null)
        {
            Log.Error($"Stackable buff {id} has not stackable data.");
            AttributesPerStack = MonsterBuffAttributes.CreateEmpty();
            EffectsPerStack = MonsterBuffAttributes.CreateEmpty();
            return;
        }

        var stackableData = data.StackableData;
        MaxStacks = stackableData.MaxStacks;
        
        if (stackableData.AttributesPerStack is not null)
            AttributesPerStack = MonsterBuffAttributes.Create(stackableData.AttributesPerStack);
        else AttributesPerStack = MonsterBuffAttributes.CreateEmpty();
        if (stackableData.EffectsPerStack is not null)
            EffectsPerStack = MonsterBuffAttributes.Create(stackableData.EffectsPerStack);
        else EffectsPerStack = MonsterBuffAttributes.CreateEmpty();
    }

    protected override void OnApplied(Monster monster, GameLocation location)
    {
        Stacks += 1;
        monster.modData[$"{ModBindings.Id}_Stacks_{Id}"] = Stacks.ToString();
    }

    /// <summary>
    /// Apply additional stack.
    /// </summary>
    /// <param name="monster">The monster to apply stack to.</param>
    /// <param name="location">The monster location.</param>
    public void ApplyAdditionalStack(Monster monster, GameLocation location)
    {
        TicksElapsed = 0;
        if (Stacks != MaxStacks) 
            UpdateMonsterBuffAttributesForExtraStacks(monster, 1);
        //monster.modData[$"{ModBindings.Id}_Stacks_{Id}"] = Stacks.ToString();
        OnNewStack(monster, location);
    }

    protected override void RemoveBuffFromMonster(Monster monster)
    {
        base.RemoveBuffFromMonster(monster);
        monster.modData[$"{ModBindings.Id}_Stacks_{Id}"] = null;
    }

    protected void UpdateMonsterBuffAttributesForExtraStacks(Monster monster, int delta)
    {
        var effectiveStacks = Stacks - 1;
        if (effectiveStacks > 0)
        {
            monster.RemoveMonsterBuffAttributes(GetAttributesWithStacks(AttributesPerStack, effectiveStacks));
            for (var i = 0; i < EffectTimesTriggered; i++)
            {
                monster.RemoveMonsterBuffAttributes(GetAttributesWithStacks(EffectsPerStack, effectiveStacks));
            }
        }

        Stacks = Math.Max(0, Stacks + delta);
        monster.modData[$"{ModBindings.Id}_Stacks_{Id}"] = Stacks.ToString();
        effectiveStacks = Stacks - 1;
        if (effectiveStacks <= 0) return;
        monster.ApplyMonsterBuffAttributes(GetAttributesWithStacks(AttributesPerStack, effectiveStacks));
        for (var i = 0; i < EffectTimesTriggered; i++)
        {
            monster.ApplyMonsterBuffAttributes(GetAttributesWithStacks(EffectsPerStack, effectiveStacks));
        }
    }

    /// <summary>
    /// Raised after additional stack is applied.
    /// </summary>
    /// <param name="monster">The monster.</param>
    /// <param name="location">The monster location.</param>
    protected virtual void OnNewStack(Monster monster, GameLocation location) {}

    protected override void OnEnd(Monster monster, GameLocation location)
    {
        base.OnEnd(monster, location);
        UpdateMonsterBuffAttributesForExtraStacks(monster, -Stacks);
    }

    private static MonsterBuffAttributes GetAttributesWithStacks(MonsterBuffAttributes attributes, int stacks)
    {
        var effectiveStacks = stacks - 1;
        if (effectiveStacks <= 0) return MonsterBuffAttributes.CreateEmpty();
        return new MonsterBuffAttributes
        {
            Resilience = attributes.Resilience * effectiveStacks,
            Health = attributes.Health * effectiveStacks,
            MaxHealth = attributes.MaxHealth * effectiveStacks,
            MissChance = attributes.MissChance * effectiveStacks,
            ArmourClass = attributes.ArmourClass * effectiveStacks,
            StunTime = attributes.StunTime * effectiveStacks,
            Damages = attributes.Damages.Select(damage => new MonsterBuffAttributes.DamageData
            {
                DamageType = damage.DamageType,
                MinDamage = damage.MinDamage * effectiveStacks,
                MaxDamage = damage.MaxDamage * effectiveStacks
            }).ToList(),
            Resistances = attributes.Resistances.Select(resistance => new MonsterBuffAttributes.ResistanceData
            {
                DamageType = resistance.DamageType,
                Delta = resistance.Delta * effectiveStacks,
            }).ToList()
        };
    }

    public static StackableMonsterBuff Create(string id, MonsterBuffData data)
    {
        if (data.StackableData is not null && data.StackableData.IndependentStacks)
            return IndependentlyStackableMonsterBuff.Create(id, data);
        return new StackableMonsterBuff(id, data);
    }
}
