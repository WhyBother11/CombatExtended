using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace CombatExtended.Framework.UI.Interface;

public class AbilityHotbar : IClickableMenu
{
    
    private static AbilityHotbar? _hotbar;
    public static AbilityHotbar Hotbar () => _hotbar ??= new AbilityHotbar();
    
    private readonly Texture2D _hotbarTexture = Helper.ModContent.Load<Texture2D>("assets/hotbar_tiles.png");

    private const int Size = 56;
    private const int AbilitySize = 16;
    private const int AbilityPositionDelta = (Size - AbilitySize * 2)/2;

    private const int ScaledSize = Size * 2;
    private const int ScaledAbilitySize = AbilitySize * 4;
    private const int ScaledAbilityPositionDelta = AbilityPositionDelta * 2;
    
    private int Selected { get; set; } = -1;
    
    private int Hovered { get; set; } = -1;

    private List<Ability> _weaponAbilities = new ();
    private List<Ability> _farmerAbilities = new ();
    private List<Ability> _abilities = new ();
    
    private int _colAmount;
    private int _rowAmount;

    private AbilityHotbar()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        xPositionOnScreen = 16;
        _rowAmount = (int) Math.Min(_abilities.Count, Game1.uiViewport.Height * 0.6 / ScaledSize);
        _colAmount = _rowAmount == 0 ? 0 : (int)MathF.Ceiling((float)_abilities.Count / _rowAmount);
        yPositionOnScreen = Game1.uiViewport.Height / 2 - _rowAmount * ScaledSize / 2;
        width = ScaledSize * _colAmount;
        height = ScaledSize * _rowAmount;
    }

    public void NotifyAbilitiesUpdated()
    {
        Hovered = -1;
        _weaponAbilities = AbilityManager.WeaponAbilities.Values.ToList();
        // some stability ig
        //_weaponAbilities.Sort((a1, a2) => a1.Id.CompareTo(a2.Id));
        _farmerAbilities = AbilityManager.FarmerAbilities.Values.ToList();
        //_farmerAbilities.Sort((a1, a2) => a1.Id.CompareTo(a2.Id));
        var abilities = new List<Ability>();
        abilities.AddRange(_weaponAbilities);
        abilities.AddRange(_farmerAbilities);
        _abilities = abilities;
        UpdatePosition();
    }
    
    public Ability Current => _abilities[Selected];

    public override void draw(SpriteBatch b)
    {
        if (Game1.activeClickableMenu != null || _abilities.Count <= 0) return;
        foreach (var (ability, index) in _abilities.Select((ability, index) => (ability, index)))
        {
            var xOffset = index / _rowAmount * ScaledSize;
            DrawAbility(b, ability, index, xOffset);
        }
        if (Hovered != -1)
        {
            var description = Game1.parseText(_abilities[Hovered].Description, Game1.smallFont, 260);
            var displayName = Game1.parseText(_abilities[Hovered].DisplayName, Game1.smallFont, 260);
            drawToolTip(b, description, displayName, null);
        }
    }

    private void DrawAbility(SpriteBatch b, Ability ability, int index, int xOffset)
    {
        var yIndex = index % _rowAmount;
        var position = new Vector2(xPositionOnScreen + xOffset, yPositionOnScreen + yIndex * ScaledSize);
        b.Draw(_hotbarTexture, position, new Rectangle(index == Selected ? Size : 0, 0, Size, Size), Color.White, scale: 2f);
        var abilityPosition = new Vector2(position.X + ScaledAbilityPositionDelta, position.Y + ScaledAbilityPositionDelta);
        var abilitySourceRect = Game1.getSourceRectForStandardTileSheet(ability.Texture, ability.SpriteIndex,
            AbilitySize, AbilitySize);
        b.Draw(ability.Texture, abilityPosition, abilitySourceRect, scale: 4);

        if (ability.IsCooldownEndHandled) return;
        var shouldHandleDuration = ability is ContinuousAbility { IsActive: true };
        var color = shouldHandleDuration ? Color.Green : Color.Red;
        var partToFill = shouldHandleDuration && ability is ContinuousAbility continuousAbility
            ? GetDurationPartToFill(continuousAbility)
            : GetCooldownPartToFill(ability);
        var fillHeight = partToFill * ScaledAbilitySize;
        var fillPosition = new Vector2(abilityPosition.X, abilityPosition.Y + ScaledAbilitySize - fillHeight);
        var rect = new Rectangle((int)fillPosition.X, (int)fillPosition.Y, ScaledAbilitySize, (int) Math.Ceiling(fillHeight));
        b.Draw(Game1.staminaRect, rect, color * 0.5f);
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        var yIndex = (y - yPositionOnScreen) / ScaledSize;
        var xIndex = (x - xPositionOnScreen) / ScaledSize;
        var index = _rowAmount * xIndex + yIndex;
        if (Selected == index)
            Selected = -1;
        else Selected = index;
    }

    public override void performHoverAction(int x, int y)
    {
        if (!isWithinBounds(x, y))
        {
            Hovered = -1;
            return;
        }
        var yIndex = (y - yPositionOnScreen) / ScaledSize;
        var xIndex = (x - xPositionOnScreen) / ScaledSize;
        var index = _rowAmount * xIndex + yIndex;
        if (index < 0 || index >= _abilities.Count)
        {
            Hovered = -1;
            return;
        }

        Hovered = index;
        base.performHoverAction(x, y);
    }

    public static void Register(IModHelper helper)
    {
        helper.Events.Display.RenderedHud += EventHandlers.OnRenderedHud_Draw;
        helper.Events.Display.WindowResized += EventHandlers.OnWindowResized_UpdatePosition;
        helper.Events.Input.ButtonPressed += EventHandlers.OnButtonPressed;
        helper.Events.Input.ButtonsChanged += EventHandlers.OnButtonReleased_ClearHighlighting;
        helper.Events.Input.CursorMoved += EventHandlers.OnCursorMoved;
        helper.Events.GameLoop.UpdateTicked += EventHandlers.OnUpdateTicked_RecalculateHighlightedTiles;
        helper.Events.GameLoop.ReturnedToTitle += (_, _) => { _hotbar = null; };
        helper.Events.GameLoop.DayEnding += (_, _) => { _hotbar = null; };
    }

    private static float GetCooldownPartToFill(Ability ability)
    {
        var cooldownTicks = ability.CooldownTicks;
        if (AbilityManager.CooldownTicksOverrides.TryGetValue(ability.Id, out var cooldownTicksOverride))
            cooldownTicks = cooldownTicksOverride;
        return 1 - (float)ability.CooldownTicksElapsed / cooldownTicks;
    }
    
    private static float GetDurationPartToFill(ContinuousAbility continuousAbility)
    {
        var durationTicks = continuousAbility.DurationTicks;
        if (AbilityManager.DurationTicksOverrides.TryGetValue(continuousAbility.Id, out var durationTicksOverride))
            durationTicks = durationTicksOverride;
        return 1 - (float)continuousAbility.DurationTicksElapsed / durationTicks;
    }
    
    private static class EventHandlers
    {

        private static bool _hotbarKeyHeld;

        private static int _prevTileX;
        private static int _prevTileY;

        public static void OnRenderedHud_Draw(object? sender, RenderedHudEventArgs e)
        {
            if (!Context.IsPlayerFree) return;
            Hotbar().draw(e.SpriteBatch);
        }

        public static void OnWindowResized_UpdatePosition(object? sender, WindowResizedEventArgs e)
        {
            if (!Context.IsPlayerFree) return;
            Hotbar().UpdatePosition();
        }

        public static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree) return;
            var hotbar = Hotbar();
            SButton.MouseLeft.TryGetController(out var controllerButtons);
            if (e.Button == SButton.MouseLeft || e.Button == controllerButtons.ToSButton())
            {
                var scaled = SUtility.ModifyCoordinatesForUIScale(e.Cursor.ScreenPixels);
                var x = (int)scaled.X;
                var y = (int)scaled.Y;
                if (hotbar.isWithinBounds(x, y) && hotbar._abilities.Any())
                {
                    Log.Trace("Registered click on ability hotbar.");
                    hotbar.receiveLeftClick(x, y);
                    AbilityManager.ClearHighlightedTiles();
                    // set false to recalculate highlighted abilities if corresponding keys are pressed
                    _hotbarKeyHeld = false;
                    if (e.Button == SButton.MouseLeft) 
                        Helper.Input.Suppress(SButton.MouseLeft);
                    else 
                        Helper.Input.Suppress(controllerButtons.ToSButton());
                }
            }
            if (!_hotbarKeyHeld && Config.HotbarKey.IsDown() && hotbar.Selected != -1 &&
                hotbar.Selected < hotbar._abilities.Count)
            {
                Log.Trace("Registered hotbar key holding with ability selected.");
                _hotbarKeyHeld = true;
                var ability = hotbar.Current;
                AbilityManager.HighlightAbility(ability);
            }

            if (Config.UseAbilityKey.IsDown() && hotbar.Selected != -1 &&
                hotbar.Selected < hotbar._abilities.Count)
            {
                Log.Trace("Registered use ability key usage.");
                var ability = hotbar.Current;
                AbilityManager.TriggerAbility(ability);
            }
        }

        /// <summary>
        /// Stop highlighting affected by ability tiles once one of <see cref="ModConfig.HotbarKey"/> combinations is released.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e"><see cref="ButtonsChangedEventArgs"/>.</param>
        public static void OnButtonReleased_ClearHighlighting(object? sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsPlayerFree) return;
            var released = e.Released.ToList();
            if (!released.Any()) return;
            if (Config.HotbarKey.AnyEquals(released))
            {
                _hotbarKeyHeld = false;
                AbilityManager.ClearHighlightedTiles();
            }
        }

        /// <summary>
        /// Recalculate affected tiles when the cursor moves to account for abilities relative to cursor.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">See <see cref="CursorMovedEventArgs"/>.</param>
        public static void OnCursorMoved(object? sender, CursorMovedEventArgs e)
        {
            var scaled = SUtility.ModifyCoordinatesForUIScale(e.NewPosition.ScreenPixels);
            var x = (int)scaled.X;
            var y = (int)scaled.Y;
            var hotbar = Hotbar();
            hotbar.performHoverAction(x, y);
            if (!Context.IsPlayerFree) return;
            if (ShouldHighlight) 
                AbilityManager.HighlightAbility(hotbar.Current);
        }

        /// <summary>
        /// Recalculate affected tiles when the player moves to account for abilities relative to player.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">See <see cref="UpdateTickedEventArgs"/>.</param>
        public static void OnUpdateTicked_RecalculateHighlightedTiles(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree) return;
            var player = Game1.player;
            var playerTileX = (int) player.Tile.X;
            var playerTileY = (int) player.Tile.Y;
            if (e.IsMultipleOf(5) && (playerTileX != _prevTileX || playerTileY != _prevTileY) && ShouldHighlight)
            {
                _prevTileX = playerTileX;
                _prevTileY = playerTileY;
                AbilityManager.HighlightAbility(Hotbar().Current);
            }
        }

        private static bool ShouldHighlight => 
            _hotbarKeyHeld && Hotbar().Selected != -1 && Hotbar().Selected < Hotbar()._abilities.Count;
    }
}
