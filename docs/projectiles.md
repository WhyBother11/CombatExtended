## Projectiles

Projectile data is located in `Mods/WhyBother11.CombatExtended/Projectiles`. This is a dictionary with key being projectile id and value being
data model described below.

### ProjectileData

Either `ItemId` or `Texture` with `SpriteIndex` should be set.

| Field              | Type     | Description                                                                                                                                 |
|--------------------|----------|---------------------------------------------------------------------------------------------------------------------------------------------|
| `Id`               | `string` | The ID of this projectile.                                                                                                                  |
| `MinDamage`        | `int`    | *(Optional)* The minimum amount of damage to enemy when this projectile hits it. Defaults to `0`.                                           |
| `MinDamage`        | `int`    | *(Optional)* The maximum amount of damage to enemy when this projectile hits it. Defaults to `0`.                                           |
| `DamageType`       | `string` | *(Optional)* The damage type from `Mods/WhyBother11.CombatExtended/DamageTypes` for damage dealt by this projectile. Defaults to `Physical` |
| `Bounces`          | `int`    | *(Optional)* The number of times the projectile can bounce off walls before being destroyed. Defaults to `0`.                               |
| `Pierces`          | `int`    | *(Optional)* The number of times the projectile can hit enemies before being destroyed. Defaults to `1`.                                    |
| `MaxDistance`      | `int`    | *(Optional)* The maximum distance in tiles this projectile can travel. Defaults to `4`.                                                     |
| `Velocity`         | `float`  | *(Optional)* The speed of this projectile. Defaults to `10`.                                                                                |
| `Rotation`         | `float`  | *(Optional)* The initial rotation applied to the sprite. Measured in radians. Defaults to `0`.                                              |
| `RotationVelocity` | `float`  | *(Optional)* The rotation speed of this projectile. Measured in radians. Defaults to `32`.                                                  |
| `TailLength`       | `int`    | *(Optional)* The amount of segments in projectile tail. Defaults to `1`.                                                                    |
| `ItemId`           | `string` | *(Optional)* The *qualified* ID of item whose sprite should be drawn. If set, overrides both `Texture` and `SpriteIndex`.                   |
| `Texture`          | `string` | *(Optional)* The asset name containing sprite for this projectile. The sprite should be 16x16.                                              |
| `SpriteIndex`      | `int`    | *(Optional)* The index within `Texture` of sprite for this projectile.                                                                      |
| `Animation`        | `string` | *(Optional)* A custom animation for this projectile. The format is described below.                                                         |

#### Projectile animation

Projectile animation specs is a string in the format of `[<SpriteIndex> <FrameAmount>]+`. `SpriteIndex` corresponds to a sprite from `Texture`. This sprite is
drawn for `FrameAmount` for frames. Once it was drawn `FrameAmount` of times, the next pair is handled, looping back to the beginning
if the end was reached.

### Projectile triggers

All triggers here provide `Projectile context`. All triggers should be prepended with `WhyBother11.CombatExtended_`. To not clutter documentation, it is omitted here.

### `ProjectileMonsterHit`

Raised when *added by the mod* projectile hits an enemy.

### Projectile trigger actions

All trigger actions should be prepended with `WhyBother11.CombatExtended_`. To not clutter documentation, it is omitted here.
Required arguments are surrounded with `<>`, while optional with `[]`.

### `LaunchProjectile`

Format: `LaunchProjectile <ProjectileId> <Direction> [OptionalArg1] [OptionalArg2]`.

Used to launch a projectile. `ProjectileId` corresponds to projectile data from `Mods/WhyBother11.CombatExtended/Projectiles`. `Direction` with `OptionalArg1` and `OptionalArg2` 
handle in which direction should projectile be fired. This can be one of following:

- `_cursor`: fires projectile from player position in direction of cursor.
- `_closestMonster`: fires projectile towards closest monster in a radius of `OptionalArg1` tiles (expects `float` type, defaults 
to `5`). This can be additionally used in `Monster context` to launch projectile from a monster position. To enable this, set `OptionalArg2`
to `true`. Otherwise, projectile will be fired from player position.
- In other cases, `Direction` is assumed to be an angle in degrees, relative to the vector pointed up (e.g. `0` will launch projectile
up, while `90` in the right direction).

### Projectile game state queries

All game state queries should be prepended with `WhyBother11.CombatExtended_`. To not clutter documentation, it is omitted here.
Required arguments are surrounded with `<>`, while optional with `[]`. If argument has `+` after it, then it can be added multiple times, and if argument is required, it should be present at least one time.

#### `PROJECTILE_ID`

Format: `PROJECTILE_ID <ProjectileId>+`

Can be used only in `Projectile context`. Used to check ID of projectile in context. `ProjectileId` corresponds to multiple possible projectile IDs to check against.
Query returns `true` if any `ProjectileId` is equal to ID of projectile in context, `false` otherwise.
