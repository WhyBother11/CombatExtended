# Weapon extensions

Weapon extension data is located in `Mods/WhyBother11.CombatExtended/WeaponExtensionData`. This is a dictionary with key being existing 
*unqualified* weapon ID and value being data model described below. They are used to extend vanilla weapons.

## WeaponExtensionData

| Field              | Type                                  | Description                                                                                                                                                                                                                                                                         |
|--------------------|---------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Abilities`        | `string`                              | *(Optional)* The space delimited string of ability IDs that should be available while this weapon is equipped.                                                                                                                                                                      |
| `DamageTypes`      | `List<DamageTypeWeaponExtensionData>` | *(Optional)* The list of data models for extra unconditional damage (with specified damage type) added to weapon's minimum and maximum damage when dealing damage to enemies. Damage added here is subject to game's normal damage formula. The data model used is described below. |
| `States`           | `Dictionary<string, WeaponStateData>` | *(Optional)* The dictionary of data models for weapon states. The key is weapon state ID and value is data model described below.                                                                                                                                                   |
| `ActionPoints`     | `string`                              | *(Optional)* The space delimited string of action point IDs that should be available when this weapon is equipped.                                                                                                                                                                  |
| `ActionsOnEquip`   | `List<string>`                        | *(Optional)* The list of actions that should be triggered when the weapon is equipped.                                                                                                                                                                                              |
| `ActionsOnUnequip` | `List<string>`                        | *(Optional)* The list of actions that should be triggered when the weapon is unequipped.                                                                                                                                                                                            |

### DamageTypeWeaponExtensionData

| Field       | Type     | Description                                                                       |
|-------------|----------|-----------------------------------------------------------------------------------|
| `Id`        | `string` | The unique ID of this entry with the list.                                        |
| `Type`      | `string` | The associated damage type ID from `Mods/WhyBother11.CombatExtended/DamageTypes`. |
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

Example:

```json
{
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Mods/WhyBother11.CombatExtended/WeaponExtensionData",
            "Entries": {
                "{{ModID}}_YourWeapon": {
                    "Abilities": "{{ModID}}_Ability1 {{ModID}}_Ability2",
                    "ActionPoints": "{{ModID}}_ActionPoint1",
                    "DamageTypes": [
                        {
                            "Id": "LightningDamage",
                            "Type": "Lightning",
                            "MinDamage": 5,
                            "MaxDamage": 10
                        }
                    ],
                    "States": {
                        "State1": {
                            "MinDamage": 100,
                            "MaxDamage": 120,
                            "Speed": 6
                        },
                        "State2": {
                            "MinDamage": 50,
                            "MaxDamage": 60,
                            "Speed": 6,
                            "Texture": "<Texture Asset Name>",
                            "SpriteIndex": 0,
                            "DamageTypes": [
                                {
                                    "Id": "LightningDamage",
                                    "Type": "Lightning",
                                    "MinDamage": 50,
                                    "MaxDamage": 60
                                }
                            ],
                        }
                    },
                    "ActionsOnEquip": [
                        "AddBuff <BuffId>"
                    ]
                }
            }
        }
    ]
}
```

### Remarks

Any active state is applied to all instances of corresponding weapon and only while it's in player's inventory. When weapon is removed from
inventory, its state is restored to default (but will be set to any currently active state if added back).

`WeaponStateData` fields will override corresponding fields for affected weapon. If the field is not present here, it will be left intact.

`DamageTypeWeaponExtensionData` can have multiple damage instances with the same `DamageType`. They will count towards the same damage type.

## Weapon trigger actions

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_SetWeaponState`

Format: `WhyBother11.CombatExtended_SetWeaponState <WeaponId> <StateId>`

Used to change current weapon state. `WeaponId` corresponds to weapon whose state should be changed. If `StateId` is `_default`, 
the weapon is reverted to default state, as defined in `Data/Weapons`. Otherwise, if `StateId` is present in this weapon's entry 
in `Mods/WhyBother11.CombatExtended/WeaponExtensionData`, this state is applied to the weapon.

## Weapon game state queries

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_CURRENT_WEAPON`

Format: `WhyBother11.CombatExtended_CURRENT_WEAPON <WeaponId>+`

Used to check player's current weapon. `WeaponId` corresponds to multiple possible weapon IDs to check against.
Query returns `true` if any `WeaponId` is equal to ID of currently held weapon, `false` otherwise.

### `WhyBother11.CombatExtended_CURRENT_WEAPON_TYPE`

Format: `WhyBother11.CombatExtended_CURRENT_WEAPON_TYPE <WeaponType>+`

Used to check player's current weapon type. `WeaponType` corresponds to multiple possible weapon types to check against.
`WeaponType` can be one of `sword`, `dagger` and `club`, each corresponding to the respective weapon type.
Query returns `true` if any `WeaponType` is equal to type of currently held weapon, `false` otherwise.

### `WhyBother11.CombatExtended_WEAPON_STATE`

Format: `WhyBother11.CombatExtended_WEAPON_STATE <WeaponId> <StateId>`

Used to check weapon's current state. `WeaponId` corresponds to a weapon whose state should be checked. `StateId` is ID of state to check, 
including `_default` to check that weapon has no active state. Query returns `true` if given weapon has specified state, `false` otherwise.
