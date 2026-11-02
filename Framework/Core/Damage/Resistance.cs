using CombatExtended.Framework.Core.Monsters.Tags;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;

namespace CombatExtended.Framework.Core.Damage;

/// <summary>
/// Enum for storing different values of damage resistances.
/// </summary>
public enum Resistance
{ 
    /// <summary>
    /// Immune to damage.
    /// </summary>
    Immune = 999,
    /// <summary>
    /// Takes 0.5x of normal damage.
    /// </summary>
    Resistant = 2,
    /// <summary>
    /// Takes 0.75x of normal damage.
    /// </summary>
    Enduring = 1,
    /// <summary>
    /// Takes normal damage.
    /// </summary>
    Normal = 0,
    /// <summary>
    /// Takes 1.5x of normal damage.
    /// </summary>
    Sensitive = -1,
    /// <summary>
    /// Takes 2x of normal damage.
    /// </summary>
    Vulnerable = -2
}

public static class ResistanceExtensions
{
    
    /// <summary>
    /// Get damage multiplier for given <see cref="Resistance"/>.
    /// </summary>
    /// <param name="resistance">Resistance instance.</param>
    /// <returns>Damage multiplier.</returns>
    public static double GetMult(this Resistance resistance)
    {
        return resistance switch
        {
            Resistance.Immune => 0.0,
            >= Resistance.Resistant => 0.5,
            Resistance.Enduring => 0.75,
            Resistance.Normal => 1.0,
            Resistance.Sensitive => 1.5,
            <= Resistance.Vulnerable => 2.0,
        };
    }

    public static Resistance Lower(this Resistance resistance)
    {
        return resistance switch
        {
            Resistance.Immune => Resistance.Immune,
            Resistance.Resistant => Resistance.Enduring,
            Resistance.Enduring => Resistance.Normal,
            Resistance.Normal => Resistance.Sensitive,
            Resistance.Sensitive or <= Resistance.Vulnerable => Resistance.Vulnerable,
            > Resistance.Resistant => Resistance.Resistant,
        };
    }

    public static Resistance Higher(this Resistance resistance)
    {
        return resistance switch
        {
            Resistance.Immune => Resistance.Immune,
            Resistance.Enduring or >= Resistance.Resistant => Resistance.Resistant,
            Resistance.Normal => Resistance.Enduring,
            Resistance.Sensitive => Resistance.Normal,
            Resistance.Vulnerable => Resistance.Sensitive,
            < Resistance.Vulnerable => Resistance.Vulnerable
        };
    }

    public static Resistance GetByCode(int code)
    {
        return code switch
        {
            999 => Resistance.Immune,
            <= -2 => Resistance.Vulnerable,
            -1 => Resistance.Sensitive,
            0 => Resistance.Normal,
            1 => Resistance.Enduring,
            >= 2 => Resistance.Resistant
        };
    }

    public static int GetCode(this Resistance resistance)
    {
        return resistance switch
        {
            Resistance.Immune => 999,
            <= Resistance.Vulnerable => -2,
            Resistance.Sensitive => -1,
            Resistance.Normal => 0,
            Resistance.Enduring => 1,
            >= Resistance.Resistant => 2,
        };
    }
    
}

public class CombinedResistance
{
    
    private Dictionary<string, int> Resistances { get; }

    public CombinedResistance()
    {
        Resistances = new Dictionary<string, int>();
    }

    public CombinedResistance(Dictionary<string, Resistance> resistances)
    {
        Resistances = resistances.ToDictionary(entry => entry.Key, entry => entry.Value.GetCode());
    }
    
    public void NoImmunities()
    {
        var resistanceLowerThanImmune = Resistance.Immune.Lower();
        foreach (var (id, resistance) in Resistances)
        {
            if (resistance != 999) continue;
            Resistances[id] = resistanceLowerThanImmune.GetCode();
        }
    }

    public double GetMult(string damageTypeId)
    {
        if (Resistances.TryGetValue(damageTypeId, out var res))
            return ResistanceExtensions.GetByCode(res).GetMult();
        return 1.0;
    }

    public int Get(string damageTypeId) => Resistances.GetValueOrDefault(damageTypeId, 0);

    public void LowerResistance(string damageTypeId, int amount)
    {
        if (Get(damageTypeId) == 999) return;
        var newRes = Get(damageTypeId) - amount;
        if (newRes == 999) newRes = 998;
        Resistances[damageTypeId] = newRes;
    }

    public void IncreaseResistance(string damageTypeId, int amount)
    {
        if (Get(damageTypeId) == 999) return;
        var newRes = Get(damageTypeId) + amount;
        if (newRes == 999) newRes = 1000;
        Resistances[damageTypeId] = newRes;
    }
    
    /// <summary>
    /// Applies <param name="tag"></param> resistance.
    /// </summary>
    /// <param name="tag">Tag whose resistances should be applied.</param>
    public void ApplyTag(Tag tag)
    {
        var otherCombinedResistance = tag.Resistance;
        foreach (var (damageTypeId, resistance) in otherCombinedResistance.Resistances)
        {
            Resistances[damageTypeId] = resistance;
        }
    }

    /// <summary>
    /// Applies tag resistance for each tag in <param name="tags"></param>
    /// </summary>
    /// <param name="tags">Sorted by priority ascending tags whose resistances should be applied.</param>
    public void ApplyTags(IEnumerable<Tag> tags)
    {
        foreach (var tag in tags)
        {
            ApplyTag(tag);
        }
    }
    
    /// <summary>
    /// Construct combined resistance based on given <param name="tagData"></param>
    /// </summary>
    /// <param name="tagData">Tag whose data should be used to construct combined resistances.</param>
    /// <returns>Combined resistances</returns>
    public static CombinedResistance FromTag(TagData tagData)
    {
        var resistances = new Dictionary<string, Resistance>();
        tagData.ResistanceData?.ForEach(rData =>
        {
            Utility.TryWithWarning<ArgumentException>(Log, () =>
                {
                    var resistance = Enum.Parse<Resistance>(rData.Value);
                    resistances.Add(rData.Type, resistance);
                }, $"Cannot parse resistance value {rData.Value}");
        });
        return new CombinedResistance(resistances);
    }
}
