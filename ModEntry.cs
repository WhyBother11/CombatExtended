using System.Reflection;
using CombatExtended.Framework.Managers;
using CombatExtended.Framework.Managers.MonsterBuffs;
using CombatExtended.Framework.UI.Interface;
using CombatExtended.Framework.Util;
using CombatExtended.Integrations.GMCM;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;
using AbilityManager = CombatExtended.Framework.Managers.AbilityManager;
using ActionPointManager = CombatExtended.Framework.Managers.ActionPointManager;
using StackableBuffManager = CombatExtended.Framework.Managers.StackableBuffManager;
using CooldownManager = CombatExtended.Framework.Managers.CooldownManager;

namespace CombatExtended;

public class ModEntry : Mod
{
    #nullable disable
    public static ModEntry Instance { get; private set; }
    
    public ModConfig Config { get; private set; }
    
    public ModLog Log { get; private set; }
    
    public IMonsterBuffManager MonsterBuffManager { get; private set; }
    #nullable restore

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = helper.ReadConfig<ModConfig>();
        Log = new ModLog(Monitor);
        Harmony harmony = new(ModManifest.UniqueID);
        harmony.PatchAll();
        
        var annotatedMethods = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsDefined(typeof(TriggerActionGSQ_Holder)))
            .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)).ToList();
        RegisterMethods(annotatedMethods);

        var dataAssetClasses = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsDefined(typeof(DataAsset)));
        RegisterDataAssets(dataAssetClasses);
        
        var registrableClasses = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsDefined(typeof(Registrable)));
        Register(registrableClasses);
        
        AbilityHotbar.Register(helper);
        ActionPointIcons.Register(helper);
        
        helper.Events.GameLoop.DayStarted += OnDayStarted_RegisterManagers;
        helper.Events.GameLoop.DayEnding += OnDayEnding_UnregisterManagers;
        helper.Events.GameLoop.GameLaunched += OnGameLaunched_AddGmcmIntegration;
    }

    private static void Register(IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            var registerMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "Register");
            registerMethod.Invoke(null, new object[]{ ModBindings.Helper });
        }
    }
    
    private static void RegisterDataAssets(IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            var registerMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == "RegisterAsset");
            registerMethod.Invoke(null, new object[]
            {
                ModBindings.Helper, type.GetCustomAttribute<DataAsset>()!.Name.Prepend($"Mods/{Id}/")
            });
        }
    }

    private static void RegisterMethods(IEnumerable<MethodInfo> methods)
    {
        foreach (var method in methods)
        {
            switch (method)
            {
                case var _ when method.IsDefined(typeof(GSQ)): RegisterGameStateQuery(method); break;
                case var _ when method.IsDefined(typeof(Trigger)): RegisterTrigger(method); break;
                case var _ when method.IsDefined(typeof(TriggerAction)): RegisterTriggerAction(method); break;
            }
        }
    }

    private static void RegisterGameStateQuery(MethodInfo method)
    { 
        GameStateQuery.Register(method.Name.PrependId(), method.CreateDelegate<GameStateQueryDelegate>());
    }

    private static void RegisterTrigger(MethodInfo method)
    {
        TriggerActionManager.RegisterTrigger(method.Name.PrependId());
    }

    private static void RegisterTriggerAction(MethodInfo method)
    {
        TriggerActionManager.RegisterAction(method.Name.PrependId(), method.CreateDelegate<TriggerActionDelegate>());
    }
    
    private void OnDayStarted_RegisterManagers(object? sender, DayStartedEventArgs e)
    {
        if (Context.IsMainPlayer)
        {
            MonsterBuffManager = new HostMonsterBuffManager();
            MonsterBuffManager.Register(Helper);
        } 
        else if (!Context.IsOnHostComputer)
        {
            MonsterBuffManager = new FarmhandMonsterBuffManager();
            MonsterBuffManager.Register(Helper);
        }
        
        WeaponManager.Register(Helper);
        CooldownManager.Register(Helper);
        AbilityManager.Register(Helper);
        StackableBuffManager.Register(Helper);
        ActionPointManager.Register(Helper);
    }

    private void OnDayEnding_UnregisterManagers(object? sender, DayEndingEventArgs e)
    {
        MonsterBuffManager.Unregister(Helper);
        WeaponManager.Unregister(Helper);
        CooldownManager.Unregister(Helper);
        AbilityManager.Unregister(Helper);
        StackableBuffManager.Unregister(Helper);
        ActionPointManager.Unregister(Helper);
    }
    
    private void OnGameLaunched_AddGmcmIntegration(object? sender, GameLaunchedEventArgs e)
    {
        var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenu>("spacechase0.GenericModConfigMenu");
        if (configMenu is null)
            return;

        configMenu.Register(
            mod: ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => Helper.WriteConfig(Config)
        );

        this.AddGmcmOptions(
            configMenu: configMenu
        );
    }
}
