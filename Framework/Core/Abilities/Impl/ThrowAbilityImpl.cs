using CombatExtended.Framework.Core.Misc.Projectiles;
using CombatExtended.Framework.Data;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;

namespace CombatExtended.Framework.Core.Abilities.Impl;

public class ThrowAbilityImpl : Ability
{
    
    public const string ThrowAbilityType = "Throw";
    
    public bool Returnable { get; set; }
    
    public bool ReturnOnWarp { get; set; }

    public string ProjectileId { get; set; }
    
    private CustomProjectile? _projectile;
    
    private ThrowAbilityImpl(string id, AbilityData data) : base(id, data)
    {
        Returnable = data.Returnable;
        ReturnOnWarp = data.ReturnOnWarp;
        ProjectileId = data.ProjectileId!;
    }

    protected override void OnTrigger(Farmer player, GameLocation location, Vector2 cursorTile, Vector2 playerTile)
    {
        if (!ProjectileDataAsset.Data.TryGetValue(ProjectileId, out var projectileData))
        {
            Log.Error($"Unknown projectile {ProjectileId}.");
            return;
        }
        var item = Game1.player.Items.FirstOrDefault(i => i is not null && i.QualifiedItemId == projectileData.ItemId);
        if (item is null) return;
        if (item.Stack > 1)
        {
            item.Stack--;
            item = item.getOne();
        }
        else player.removeItemFromInventory(item);
        var velocity = SUtility.getVelocityTowardPoint(player.Position, 
            Helper.Input.GetCursorPosition().AbsolutePixels, projectileData.Velocity);
        Action<GameLocation, Farmer> onDestroy = (loc, f) =>
        {
            if (Returnable)
            {
                if (!f.addItemToInventoryBool(item)) 
                    Game1.createItemDebris(item, f.getStandingPosition(), 1);
            } else Game1.createItemDebris(item, _projectile?.position.Value ?? f.getStandingPosition(), 
                _projectile?.GetDebrisSpawnDirection() ?? 1);
            _projectile = null;
            Helper.Events.Player.Warped -= OnWarped_ClearAnyThrownItemInstanceAndReturn;
        };
        _projectile = CustomProjectile.Launch(location, player.Position, velocity, projectileData, item, onDestroy, true);
        if (Returnable || ReturnOnWarp)
            Helper.Events.Player.Warped += OnWarped_ClearAnyThrownItemInstanceAndReturn;
    }

    public static ThrowAbilityImpl CreateCustom(string abilityId, AbilityData data)
    {
        return new ThrowAbilityImpl(abilityId, data);
    }

    private void OnWarped_ClearAnyThrownItemInstanceAndReturn(object? sender, WarpedEventArgs e)
    {
        if (!e.IsLocalPlayer) return;
        var player = e.Player;
        var thrownItemProjectiles = e.OldLocation.projectiles
            .OfType<CustomProjectile>()
            .Where(projectile => projectile == _projectile)
            .ToList();
        var thrownItemDebris = e.OldLocation.debris
            .Where(debris => debris.item is not null && debris.item == _projectile?.Item)
            .ToList();
        var returned = false;
        if (thrownItemProjectiles.Any())
        {
            var item = thrownItemProjectiles.First().Item;
            if (item is not null && !player.addItemToInventoryBool(item)) 
                Game1.createItemDebris(item, player.getStandingPosition(), 1);
            returned = true;
        }

        if (thrownItemDebris.Any() && !returned)
        {
            var item = thrownItemDebris.First().item;
            if (!player.addItemToInventoryBool(item)) 
                Game1.createItemDebris(item, player.getStandingPosition(), 1);
        }

        e.OldLocation.debris.RemoveWhere(thrownItemDebris.Contains);
        e.OldLocation.projectiles.RemoveWhere(thrownItemProjectiles.Contains);
        _projectile = null;
        Helper.Events.Player.Warped -= OnWarped_ClearAnyThrownItemInstanceAndReturn;
    }
}
