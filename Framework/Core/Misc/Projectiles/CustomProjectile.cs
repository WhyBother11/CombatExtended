using CombatExtended.Framework.Core.Damage;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.UI.Animations;
using CombatExtended.Framework.Util;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Core.Misc.Projectiles;

[HarmonyPatch]
public class CustomProjectile : Projectile
{
    
    public string Id { get; set; }

    public readonly Item? Item;

    public ProjectileData Data { get; set; }
    
    public Farmer Owner { get; private set; }
    
    private readonly Action<GameLocation, Farmer>? _onDestroy;

    private readonly GameLocation _location;
    
    private readonly ProjectileAnimator? _animator;

    private bool _shouldDestroy;

    private readonly bool _destroyOnWallContact;

    public CustomProjectile(
        float xVelocity,
        float yVelocity,
        Vector2 position,
        GameLocation location,
        Farmer owner,
        ProjectileData data,
        Item? item = null,
        Action<GameLocation, Farmer>? onDestroy = null,
        bool destroyOnWallContact = false,
        ProjectileAnimator? animator = null)
    {
        Id = data.Id;
        Data = data;
        Owner = owner;
        maxTravelDistance.Value = data.MaxDistance * Game1.tileSize;
        theOneWhoFiredMe.Set(location, owner);
        bouncesLeft.Value = data.Bounces;
        tailLength.Value = data.TailLength;
        piercesLeft.Value = data.Pierces;
        rotation = data.Rotation;
        rotationVelocity.Value = data.RotationVelocity;
        this.xVelocity.Value = xVelocity;
        this.yVelocity.Value = yVelocity;
        this.position.Value = new Vector2(position.X, position.Y);
        if (data.ItemId is not null)
        {
            Item = item ?? ItemRegistry.Create(data.ItemId);
            itemId.Value = data.ItemId;
        }
        _location = location;
        if (animator is not null) 
            _animator = animator;
        else if (Item is not null)
        {
            var itemData = ItemRegistry.GetDataOrErrorItem(itemId.Value);
            _animator = new ProjectileAnimator(itemData.GetTexture(), itemData.SpriteIndex);
        } else if (data.Texture is not null && Data.SpriteIndex is not null)
            _animator = new ProjectileAnimator(
                Helper.GameContent.Load<Texture2D>(data.Texture), data.SpriteIndex!.Value,
                data.Animation?.Split(' ').Select(int.Parse).ToArray(), tailLength.Value);
        else Log.Error($"Projectile {data.Id} has no item or texture with sprite index set");
        damagesMonsters.Value = true;
        _onDestroy = onDestroy;
        _destroyOnWallContact = destroyOnWallContact;
    }
    
    public override void updatePosition(GameTime time)
    {
        xVelocity.Value += acceleration.X;
        yVelocity.Value += acceleration.Y;
        position.X += xVelocity.Value;
        position.Y += yVelocity.Value;
        if (_destroyOnWallContact && _location.getTileIndexAt((int)(position.X / 64), (int)(position.Y / 64), "Buildings") != -1)
            _shouldDestroy = true;
    }
    
    public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
    {
        var farmer = this.GetLocalFarmer(location);

        if (n is not Monster monster) return;
        if (Data.DamageType is DamageType.Physical) 
            location.damageMonster(monster.GetBoundingBox(), Data.MinDamage, Data.MaxDamage, false, farmer, true);
        else
        {
            var damage = Game1.random.Next(Data.MinDamage, Data.MaxDamage + 1);
            DamageType.TakeExtraTypePureDamage(monster, damage, Data.DamageType, farmer);
        }
        piercesLeft.Value -= 1;

        var item = ItemRegistry.Create("(O)168");
        item.PackProjectileInfo(this);
        item.PackMonsterInfo(monster);
        TriggerActionManager.Raise(
            Triggers.ProjectileMonsterHit,
            new object[] { monster },
            location: _location,
            player: Owner,
            targetItem: item
        );
    }

    public override void behaviorOnCollisionWithOther(GameLocation location)
    {
        // ignoring
    }

    public override void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player)
    {
        // ignoring
    }

    public override void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation,
        GameLocation location)
    {
        // ignoring
    }

    public override void draw(SpriteBatch b)
    {
        var scale = 4f * localScale;
        var vector2 = position.Value;
        _animator?.DispatchDraw(b, vector2, this, rotation, scale);

        if (height.Value > 0.0)
            b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, vector2 + new Vector2(32f, 32f)),
                Game1.shadowTexture.Bounds, Color.White * alpha.Value * 0.75f, 0.0f,
                new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 2f,
                SpriteEffects.None, (float)((vector2.Y - 1.0) / 10000.0));
        // Todo: copy tail draw to animator
    }

    public int GetDebrisSpawnDirection()
    {
        var yVelocityValue = yVelocity.Value;
        var xVelocityValue = xVelocity.Value;
        if (Math.Abs(yVelocityValue) >= Math.Abs(xVelocityValue)) 
            return yVelocityValue <= 0 ? FacingDirection.Down : FacingDirection.Up;
        return xVelocityValue <= 0 ? FacingDirection.Right : FacingDirection.Left;
    }

    public static CustomProjectile Launch(GameLocation loc, Vector2 position, Vector2 velocity, ProjectileData data, 
        Item? item = null, Action<GameLocation, Farmer>? onDestroy = null, bool destroyOnWallContact = false)
    {
        var projectile = new CustomProjectile(
            xVelocity: velocity.X, yVelocity: velocity.Y, position: position, location: loc, 
            owner: Game1.player, data: data, onDestroy: onDestroy, item: item, destroyOnWallContact: destroyOnWallContact
        );
        loc.projectiles.Add(projectile);
        return projectile;
    }

    [HarmonyPatch(typeof(Projectile), nameof(update))]
    [HarmonyPostfix]
    private static void Projectile_update_Postfix(Projectile __instance, ref bool __result)
    {
        if (__instance is not CustomProjectile customProjectile) return;
        if (customProjectile._shouldDestroy) 
            __result = true;
        if (__result) customProjectile._onDestroy?.Invoke(customProjectile._location, customProjectile.Owner);
    }
}
