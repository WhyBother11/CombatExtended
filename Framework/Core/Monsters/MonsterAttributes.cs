using CombatExtended.Framework.Data;

namespace CombatExtended.Framework.Core.Monsters;

public class MonsterAttributes
{
    public float Resilience { get; set; }
    
    public float Health { get; set; }
    
    public float MaxHealth { get; set; }
    
    public float MissChance { get; set; }
    
    public float ArmourClass { get; set; }
    
    public float Weight { get; set; }
    
    public float HitMissRate { get; set; }
    
    public float StunTime { get; set; }
    
    public float Speed { get; set; }
    
    public List<DamageData> Damages { get; set; } = new ();
    
    public List<ResistanceData> Resistances { get; set; } = new ();
    
    #nullable disable
    public class ResistanceData
    {
        public string DamageType { get; set; }
        
        public float Delta { get; set; }
    }
    
    public class DamageData
    {
        public string DamageType { get; set; }
        
        public float MinDamage { get; set; }
        
        public float MaxDamage { get; set; }
    }
    #nullable restore

    public static DamageData GetDamageData(MonsterAttributesData.DamageAttributesData attributesData)
    {
        return new DamageData
        {
            DamageType = attributesData.DamageType,
            MinDamage = attributesData.Damage,
            MaxDamage = attributesData.MaxDamage.GetValueOrDefault(attributesData.Damage)
        };
    }

    public static ResistanceData GetResistanceData(MonsterAttributesData.ResistanceAttributesData attributesData)
    {
        return new ResistanceData
        {
            DamageType = attributesData.DamageType,
            Delta = attributesData.Delta
        };
    }
}
