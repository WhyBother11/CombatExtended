using StardewModdingAPI.Utilities;

namespace CombatExtended;

public class ModConfig
{
    
    public bool NoImmunities { get; set; } = false;
    
    public int MasteryForMjolnirWorthiness { get; set; } = 4;

    public KeybindList HotbarKey { get; set; } = KeybindList.Parse("B");
    
    public KeybindList UseAbilityKey { get; set; } = KeybindList.Parse("U");
}