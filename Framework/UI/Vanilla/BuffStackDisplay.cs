using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CombatExtended.Framework.UI.Vanilla;

/// <summary>
/// Enables displaying stack amount on buff icons for stackable buffs.
/// </summary>
[Registrable]
public static class BuffStackDisplay
{
    #nullable disable
    private static IModHelper _helper;
    #nullable restore
    
    private static readonly Dictionary<string, Func<int>> TrackedBuffs = new ();

    private const int StackMarginLeft = 8;
    private const int StackMarginTop = 8;

    public static void Register(IModHelper helper)
    {
        _helper = helper;
        helper.Events.Display.RenderedHud += OnRenderedHud_DrawStacks;
        helper.Events.GameLoop.DayEnding += OnDayEnding_StopTrackingAllBuffs;
    }

    /// <summary>
    /// Start tracking buff with given <paramref name="buffId"/> to display its stacks.
    /// </summary>
    /// <param name="buffId">ID of buff.</param>
    /// <param name="stackLookup">Function that returns amount of stacks.</param>
    public static void StartTracking(string buffId, Func<int> stackLookup)
    {
        TrackedBuffs.Add(buffId, stackLookup);
    }
    
    /// <summary>
    /// Stop tracking buff with given <paramref name="buffId"/>.
    /// </summary>
    /// <param name="buffId">ID of buff.</param>
    public static void StopTracking(string buffId)
    {
        TrackedBuffs.Remove(buffId);
    }

    /// <summary>
    /// Draw stacks of all present stackable buffs.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">See <see cref="RenderedHudEventArgs"/>.</param>
    private static void OnRenderedHud_DrawStacks(object? sender, RenderedHudEventArgs e)
    {
        if (TrackedBuffs.Count == 0) return;
        var buffDictionary = _helper.Reflection.GetField<Dictionary<ClickableTextureComponent, Buff>>(
            Game1.buffsDisplay, "buffs").GetValue();
        var buffDisplay = buffDictionary
            .Where(entry => TrackedBuffs.ContainsKey(entry.Value.id))
            .ToDictionary(entry => entry.Key, entry => entry.Value);
        foreach (var comp in buffDisplay)
        {
            DrawStacks(e.SpriteBatch, comp.Key, comp.Value.id);
        }
    }

    /// <summary>
    /// Draw stacks for buff with given <paramref name="buffId"/> if it is present and tracked.
    /// </summary>
    /// <param name="b"><see cref="SpriteBatch"/>.</param>
    /// <param name="component">Game component for displaying buff.</param>
    /// <param name="buffId">ID of tracked buff.</param>
    private static void DrawStacks(SpriteBatch b, ClickableTextureComponent component, string buffId)
    {
        var stacks = TrackedBuffs[buffId].Invoke().ToString();
        var position = new Vector2(component.bounds.X + StackMarginLeft, component.bounds.Y + StackMarginTop);
        var xOffset = 0;
        foreach (var charNum in stacks)
        {
            b.DrawNumber(charNum, new Vector2(position.X + xOffset, position.Y), 2f);
            xOffset += DrawUtil.DigitWidth + 4;
        }
    }

    /// <summary>
    /// Stop tracking all registered buffs on day ending, as buffs are removed.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e"><see cref="DayEndingEventArgs"/>.</param>
    private static void OnDayEnding_StopTrackingAllBuffs(object? sender, DayEndingEventArgs e)
    {
        foreach (var buff in TrackedBuffs.Keys)
        {
            StopTracking(buff);
        }
    }
}
