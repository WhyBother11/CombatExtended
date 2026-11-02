namespace CombatExtended.Framework.Util;

[AttributeUsage(AttributeTargets.Class)]
public class TriggerActionGSQ_Holder : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class GSQ : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class Trigger : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class TriggerAction : Attribute { }

[AttributeUsage(AttributeTargets.Class)]
public class DataAsset : Attribute
{
    public string Name { get; private set; }
    
    public DataAsset(string name) => Name = name;
}

[AttributeUsage(AttributeTargets.Class)]
public class Registrable : Attribute { }
