using CombatExtended.Framework.Data;

namespace CombatExtended.Framework.Core.Monsters.Statuses;

public class MonsterStatusAttributes : MonsterAttributes
{

    public static MonsterStatusAttributes CreateEmpty()
    {
        return new MonsterStatusAttributes();
    }

    public static MonsterStatusAttributes Create(MonsterStatusAttributesData data)
    {
        var attrs = new MonsterStatusAttributes();
        if (data.Resilience is not null) attrs.Resilience = data.Resilience.Value;
        if (data.Health is not null) attrs.Health = data.Health.Value;
        if (data.MaxHealth is not null) attrs.MaxHealth = data.MaxHealth.Value;
        if (data.MissChance is not null) attrs.MissChance = data.MissChance.Value;
        if (data.ArmourClass is not null) attrs.ArmourClass = data.ArmourClass.Value;
        if (data.Weight is not null) attrs.Weight = data.Weight.Value;
        if (data.HitMissRate is not null) attrs.HitMissRate = data.HitMissRate.Value;
        if (data.StunTime is not null) attrs.StunTime = data.StunTime.Value;
        if (data.Speed is not null) attrs.Speed = data.Speed.Value;
        if (data.DamageData is not null) attrs.Damages = data.DamageData.Select(GetDamageData).ToList();
        if (data.ResistanceData is not null) attrs.Resistances = data.ResistanceData.Select(GetResistanceData).ToList();
        return attrs;
    }
}
