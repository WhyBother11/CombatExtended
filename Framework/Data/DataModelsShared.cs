using Microsoft.Xna.Framework.Content;

namespace CombatExtended.Framework.Data;

public class MonsterAttributesData
{

    [ContentSerializer(Optional = true)] 
    public float? Resilience;

    [ContentSerializer(Optional = true)] 
    public float? Health;

    [ContentSerializer(Optional = true)] 
    public float? MaxHealth;

    [ContentSerializer(Optional = true)] 
    public float? MissChance;

    [ContentSerializer(Optional = true)] 
    public float? ArmourClass;

    [ContentSerializer(Optional = true)] 
    public float? StunTime;

    [ContentSerializer(Optional = true)] 
    public float? Speed;
    
    [ContentSerializer(Optional = true)] 
    public float? Weight;
    
    [ContentSerializer(Optional = true)] 
    public float? HitMissRate;

    [ContentSerializer(Optional = true)] 
    public List<DamageAttributesData>? DamageData;

    [ContentSerializer(Optional = true)] 
    public List<ResistanceAttributesData>? ResistanceData;
    
    public class ResistanceAttributesData
    {
        #nullable disable
        public string Id;
        
        public string DamageType;

        public float Delta;
        #nullable restore
    }
    
    public class DamageAttributesData
    {
        #nullable disable
        public string Id;
        
        public string DamageType;

        public float Damage;
        #nullable restore

        [ContentSerializer(Optional = true)] 
        public float? MaxDamage;
    }
}
