using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
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
        MaxStacks = stackableData.MaxStacks.CheckIfIndefiniteAndModify(v => v);
        AttributesPerStack = stackableData.AttributesPerStack is not null
            ? MonsterBuffAttributes.Create(stackableData.AttributesPerStack) : MonsterBuffAttributes.CreateEmpty();
        EffectsPerStack = stackableData.EffectsPerStack is not null
            ? MonsterBuffAttributes.Create(stackableData.EffectsPerStack) : MonsterBuffAttributes.CreateEmpty();
    }

    protected override void OnApplied(Monster monster, GameLocation location)
    {
        Stacks += 1;
        monster.modData[$"{ModBindings.Id}_Stacks_{Id}"] = Stacks.ToString();
    }

    public override void DoEffect(Monster monster, GameLocation location)
    {
        base.DoEffect(monster, location);
        var effectiveStacks = Stacks - 1;
        if (effectiveStacks <= 0) return;
        monster.ApplyMonsterBuffAttributes(GetAttributesWithStacks(EffectsPerStack, effectiveStacks));
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
        OnNewStack(monster, location);
    }

    protected override void AddBuffToMonster(Monster monster)
    {
        base.AddBuffToMonster(monster);
        // will be called only when buff is applied but not on additional stacks
        monster.modData[$"{ModBindings.Id}_Stacks_{Id}"] = Stacks.ToString();
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
        Stacks = Math.Clamp(0, Stacks + delta, MaxStacks);
        monster.modData[$"{ModBindings.Id}_Stacks_{Id}"] = Stacks.ToString();
        effectiveStacks = Stacks - 1;
        if (effectiveStacks <= 0) return;
        // recalculate revertable buff attributes
        monster.ApplyMonsterBuffAttributesRevertable(GetAttributesWithStacks(AttributesPerStack, effectiveStacks));
        for (var i = 0; i < EffectTimesTriggered; i++)
        {
            monster.ApplyMonsterBuffAttributesRevertable(GetAttributesWithStacks(EffectsPerStack, effectiveStacks));
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
            Weight = attributes.Weight * effectiveStacks,
            StunTime = attributes.StunTime * effectiveStacks,
            Speed = attributes.Speed * effectiveStacks,
            Damages = attributes.Damages.Select(damage => new MonsterAttributes.DamageData
            {
                DamageType = damage.DamageType,
                MinDamage = damage.MinDamage * effectiveStacks,
                MaxDamage = damage.MaxDamage * effectiveStacks
            }).ToList(),
            Resistances = attributes.Resistances.Select(resistance => new MonsterAttributes.ResistanceData
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
