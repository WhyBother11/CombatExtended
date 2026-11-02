using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Graphics;

namespace CombatExtended.Framework.Core.Misc.ActionPoints;

public class ActionPoint
{
    public string Id { get; private set; }
    
    public string DisplayName { get; set; }
    
    public string Description { get; set; }
    
    public int CurrentAmount { get; set; }
    
    public int MaxAmount { get; set; }

    public int ReplenishTimeTicks { get; private set; }
    
    public Texture2D Texture { get; set; }
    
    public int SpriteIndex { get; set; }
    
    public int ReplenishTicksElapsed { get; set; }

    protected ActionPoint(string id, ActionPointData data)
    {
        Id = id;
        DisplayName = data.DisplayName;
        Description = data.Description;
        MaxAmount = data.MaxAmount;
        CurrentAmount = MaxAmount;
        ReplenishTimeTicks = data.ReplenishTime.ToTicks();
        Texture = Helper.GameContent.Load<Texture2D>(data.Texture);
        SpriteIndex = data.SpriteIndex;
    }

    /// <summary>
    /// Replenish <paramref name="amount"/> number of points, resetting <see cref="ReplenishTicksElapsed"/> as well.
    /// </summary>
    /// <param name="amount">Number of points to replenish.</param>
    public void Replenish(int amount = 1)
    {
        ReplenishTicksElapsed = 0;
        Add(amount);
    }

    /// <summary>
    /// Add <paramref name="amount"/> number of points directly, capping at <see cref="MaxAmount"/>.
    /// </summary>
    /// <param name="amount">Number of points to add.</param>
    public void Add(int amount = 1)
    {
        if (CurrentAmount >= MaxAmount) return;
        CurrentAmount = Math.Min(CurrentAmount + amount, MaxAmount);
    }

    /// <summary>
    /// Spend <paramref name="amount"/> number of action points if available, or all of them otherwise.
    /// </summary>
    public void Spend(int amount = 1)
    {
        if (CurrentAmount <= 0) return;
        CurrentAmount = Math.Max(CurrentAmount - amount, 0);
    }

    public static ActionPoint Create(string id)
    {
        if (!ActionPointDataAsset.Exists(id))
            throw new ArgumentException($"Unknown action point ID {id}");
        return new ActionPoint(id, ActionPointDataAsset.Data[id]);
    }
}
