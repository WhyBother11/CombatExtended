using StardewModdingAPI.Utilities;

namespace CombatExtended;

public class ModConfig
{
    
    public bool NoImmunities { get; set; } = false;

    public KeybindList HotbarKey { get; set; } = KeybindList.Parse("H");
    
    public KeybindList HotbarNextKey { get; set; } = KeybindList.Parse("N");
    
    public KeybindList UseAbilityKey { get; set; } = KeybindList.Parse("U");
    
    public float UpdateRate { get; set; } = 1f;
}