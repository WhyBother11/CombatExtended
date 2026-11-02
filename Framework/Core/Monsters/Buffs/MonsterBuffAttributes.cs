using CombatExtended.Framework.Data;

namespace CombatExtended.Framework.Core.Monsters.Buffs;

#pragma warning disable CS8618
public class MonsterBuffAttributes
{
    
    public float Resilience { get; set; }
    
    public float Health { get; set; }
    
    public float MaxHealth { get; set; }
    
    public float MissChance { get; set; }
    
    public float ArmourClass { get; set; }
    
    public float StunTime { get; set; }
    
    public List<DamageData> Damages { get; set; } = new ();
    
    public List<ResistanceData> Resistances { get; set; } = new ();
    
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

    public static MonsterBuffAttributes CreateEmpty()
    {
        return new MonsterBuffAttributes();
    }

    public static MonsterBuffAttributes Create(MonsterBuffAttributesData data)
    {
        var attrs = new MonsterBuffAttributes();
        if (data.Resilience is not null) attrs.Resilience = data.Resilience.Value;
        if (data.Health is not null) attrs.Health = data.Health.Value;
        if (data.MaxHealth is not null) attrs.MaxHealth = data.MaxHealth.Value;
        if (data.MissChance is not null) attrs.MissChance = data.MissChance.Value;
        if (data.ArmourClass is not null) attrs.ArmourClass = data.ArmourClass.Value;
        if (data.StunTime is not null) attrs.StunTime = data.StunTime.Value;
        if (data.DamageData is not null) attrs.Damages = data.DamageData.Select(GetDamageData).ToList();
        if (data.ResistanceData is not null) attrs.Resistances = data.ResistanceData.Select(GetResistanceData).ToList();
        return attrs;
    }

    public static DamageData GetDamageData(MonsterBuffAttributesData.DamageAttributesData attributesData)
    {
        return new DamageData
        {
            DamageType = attributesData.DamageType,
            MinDamage = attributesData.Damage,
            MaxDamage = attributesData.MaxDamage.GetValueOrDefault(attributesData.Damage)
        };
    }

    public static ResistanceData GetResistanceData(MonsterBuffAttributesData.ResistanceAttributesData attributesData)
    {
        return new ResistanceData
        {
            DamageType = attributesData.DamageType,
            Delta = attributesData.Delta
        };
    }
}
