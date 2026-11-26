using CombatExtended.Framework.Util;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using LinkEffectManager = CombatExtended.Framework.Managers.LinkEffectManager;

namespace CombatExtended.Framework.UI.Animations;

[Registrable]
public static class LinkEffects
{

    public static void Register(IModHelper helper)
    {
        Helper.Events.Display.RenderedWorld += OnRenderedWorld_DrawLink;
    }

    private static void OnRenderedWorld_DrawLink(object? sender, RenderedWorldEventArgs e)
    {
        foreach (var (_, manager) in LinkEffectManager.GetAll())
        {
            foreach (var (_, links) in manager.Links)
            {
                foreach (var (_, link) in links)
                {
                    link.Draw(e.SpriteBatch);
                }
            }
        }
    }
}
