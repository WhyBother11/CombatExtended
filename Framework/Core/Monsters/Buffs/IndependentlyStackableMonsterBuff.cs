using CombatExtended.Framework.Data;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters.Buffs;

public class IndependentlyStackableMonsterBuff : StackableMonsterBuff
{
    
    public readonly LinkedList<int> StackTimers = new ();
    
    protected IndependentlyStackableMonsterBuff(
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
    ) : base(id, displayName, description, duration, isDebuff, maxStacks, attributes, effectAttributes,
        doEffectEveryTick, doAnimationEveryTick, attributesPerStack, effectAttributesPerStack)
    { }
    
    protected IndependentlyStackableMonsterBuff(string id, MonsterBuffData data) : base(id, data) {}

    public void OnIndependentStacksExpired(Monster monster, int expiredAmount)
    {
        UpdateMonsterBuffAttributesForExtraStacks(monster, -expiredAmount);
    }
    
    protected override void OnNewStack(Monster monster, GameLocation location)
    {
        if (Stacks == MaxStacks) StackTimers.RemoveFirst();
        StackTimers.AddLast(DurationTicks);
    }

    public new static IndependentlyStackableMonsterBuff Create(string id, MonsterBuffData data)
    {
        return new IndependentlyStackableMonsterBuff(id, data);
    }
}
