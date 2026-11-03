global using static CombatExtended.ModBindings;
global using SUtility = StardewValley.Utility;
global using SObject = StardewValley.Object;
global using Event = CombatExtended.Framework.Events.Events;
using CombatExtended.Framework.Managers;
using CombatExtended.Framework.Managers.MonsterBuffs;
using StardewModdingAPI;

namespace CombatExtended;

public static class ModBindings
{
    public static IModHelper Helper => ModEntry.Instance.Helper;
    
    public static IManifest Manifest => ModEntry.Instance.ModManifest;
    
    public static ModLog Log => ModEntry.Instance.Log;
    
    public static ModConfig Config => ModEntry.Instance.Config;
    
    public static IMonsterBuffManager MonsterBuffManager => ModEntry.Instance.MonsterBuffManager;
    
    public static AbilityManager AbilityManager => AbilityManager.Instance;
    
    public static StackableBuffManager StackableBuffManager => StackableBuffManager.Instance;
    
    public static ActionPointManager ActionPointManager => ActionPointManager.Instance;
    
    public static CooldownManager CooldownManager => CooldownManager.Instance;
    
    public static string Id => ModEntry.Instance.ModManifest.UniqueID;
}
