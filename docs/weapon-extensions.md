## Weapon extensions

Weapon extension data is located in `Mods/WhyBother11.CombatExtended/WeaponExtensionData`. This is a dictionary with key being existing weapon id and value being
data model described below. They are used to extend vanilla weapons.

### WeaponExtensionData

| Field          | Type                                  | Description                                                                                                          |
|----------------|---------------------------------------|----------------------------------------------------------------------------------------------------------------------|
| `Abilities`    | `string`                              | *(Optional)* The space delimited string of ability IDs that should be available when this weapon is equipped.        |
| `DamageTypes`  | `List<DamageTypeWeaponExtensionData>` | *(Optional)* The list of data for extra damage instances dealt to hit enemies. The data model is described below.    |
| `States`       | `Dictionary<string, WeaponStateData>` | *(Optional)* The list of data for weapon states. The key is weapon state ID and value is data model described below. |
| `ActionPoints` | `string`                              | *(Optional)* The space delimited string of action point IDs that should be available when this weapon is equipped.   |

### DamageTypeWeaponExtensionData

| Field       | Type     | Description                                                                       |
|-------------|----------|-----------------------------------------------------------------------------------|
| `Type`      | `string` | The associated damage type id from `Mods/WhyBother11.CombatExtended/DamageTypes`. |
| `MinDamage` | `int`    | The minimum damage of this damage instance.                                       |
| `MaxDamage` | `int`    | The maximum damage of this damage instance.                                       |

### WeaponStateData

| Field            | Type                                  | Description                                                                          |
|------------------|---------------------------------------|--------------------------------------------------------------------------------------|
| `Id`             | `string`                              | The ID of this weapon state.                                                         |
| `MinDamage`      | `int`                                 | *(Optional)* The overriding minimum damage of this weapon.                           |
| `MaxDamage`      | `int`                                 | *(Optional)* The overriding maximum damage of this weapon.                           |
| `Texture`        | `string`                              | *(Optional)* The asset name containing overriding sprite for this weapon.            |
| `SpriteIndex`    | `int`                                 | *(Optional)* The index within `Texture` of overriding sprite for this weapon.        |
| `Knockback`      | `float`                               | *(Optional)* Overrides how far the target is pushed when hit.                        |
| `Speed`          | `int`                                 | *(Optional)* Overrides how fast the player can swing the weapon.                     |
| `Precision`      | `int`                                 | *(Optional)* Overrides how much chance that a strike will miss is reduced.           |
| `Defense`        | `int`                                 | *(Optional)* Overrides how much damage received by the player is reduced.            |
| `AreaOfEffect`   | `int`                                 | *(Optional)* Overrides how much the area of effect is increased.                     |
| `CritChance`     | `float`                               | *(Optional)* Overrides the chance of a critical hit.                                 |
| `CritMultiplier` | `float`                               | *(Optional)* Overrides the multiplier applied to the base damage for a critical hit. |
| `DamageTypes`    | `List<DamageTypeWeaponExtensionData>` | *(Optional)* Overrides list of extra damage instances dealt to hit enemies.          |

### Remarks

Any active state is applied to all instances of corresponding weapon and only while it's in player's inventory. When weapon is removed from
inventory, its state is restored to default (but will be set to any currently active state if added back).

`WeaponStateData` fields will override corresponding fields for affected weapon. If the field is not present here, it will be left intact.

`DamageTypeWeaponExtensionData` can have multiple damage instances with the same `DamageType`. They will be treated as separate damage instances.

### Weapon trigger actions

All trigger actions should be prepended with `WhyBother11.CombatExtended_`. To not clutter documentation, it is omitted here.
Required arguments are surrounded with `<>`, while optional with `[]`.

### `SetWeaponState`

Format: `SetWeaponState <WeaponId> <StateId>`

Used to change current weapon state. `WeaponId` corresponds to weapon whose state should be changed. If `StateId` is `_default`, the weapon is reverted to default state,
as defined in `Data/Weapons`. Otherwise, if `StateId` is present in this weapon's entry in `Mods/WhyBother11.CombatExtended/WeaponExtensionData`, this state is applied to the weapon.

### Weapon game state queries

All game state queries should be prepended with `WhyBother11.CombatExtended_`. To not clutter documentation, it is omitted here.
Required arguments are surrounded with `<>`, while optional with `[]`. If argument has `+` after it, then it can be added multiple times, and if
argument is required, it should be present at least one time.

### `CURRENT_WEAPON`

Format: `CURRENT_WEAPON <WeaponId>+`

Used to check player's current weapon. `WeaponId` corresponds to multiple possible weapon IDs to check against.
Query returns `true` if any `WeaponId` is equal to ID of currently held weapon, `false` otherwise.

### `CURRENT_WEAPON_TYPE`

Format: `CURRENT_WEAPON_TYPE <WeaponType>+`

Used to check player's current weapon type. `WeaponType` corresponds to multiple possible weapon types to check against.
`WeaponType` can be one of `sword`, `dagger` and `club`, each corresponding to the respective weapon type.
Query returns `true` if any `WeaponType` is equal to type of currently held weapon, `false` otherwise.

### `WEAPON_STATE`

Format: `WEAPON_STATE <WeaponId> <StateId>`

Used to check weapon's current state. `WeaponId` corresponds to a weapon whose state should be checked. `StateId` is ID of state to check, including `_default`
to check that weapon has no active state. Query returns `true` if given weapon has specified state, `false` otherwise.