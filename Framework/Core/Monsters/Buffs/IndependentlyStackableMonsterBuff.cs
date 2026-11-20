using CombatExtended.Framework.Data;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Monsters.Buffs;

public class IndependentlyStackableMonsterBuff : StackableMonsterBuff
{
    
    public readonly LinkedList<int> StackTimers = new ();
    
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
