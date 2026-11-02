namespace CombatExtended.Integrations.GMCM;

public static class EnableGmcmIntegration
{
    public static void AddGmcmOptions(
        this ModEntry modEntry,
        IGenericModConfigMenu configMenu)
    {
        configMenu.AddBoolOption(
            mod: modEntry.ModManifest,
            name: () => modEntry.Helper.Translation.Get("config.option.no-immunities"),
            tooltip: () => modEntry.Helper.Translation.Get("config.option.no-immunities.tooltip"),
            getValue: () => modEntry.Config.NoImmunities,
            setValue: value => modEntry.Config.NoImmunities = value
        );
        configMenu.AddKeybindList(
            mod: modEntry.ModManifest,
            name: () => modEntry.Helper.Translation.Get("config.option.hotbar-key"),
            tooltip: () => modEntry.Helper.Translation.Get("config.option.hotbar-key.tooltip"),
            getValue: () => Config.HotbarKey,
            setValue: value => Config.HotbarKey = value);
        configMenu.AddKeybindList(
            mod: modEntry.ModManifest,
            name: () => modEntry.Helper.Translation.Get("config.option.use-ability-key"),
            tooltip: () => modEntry.Helper.Translation.Get("config.option.use-ability-key.tooltip"),
            getValue: () => Config.UseAbilityKey,
            setValue: value => Config.UseAbilityKey = value);
    }
}