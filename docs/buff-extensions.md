# Buff extensions

Buff extension data is located in `Mods/WhyBother11.CombatExtended/BuffExtensionData`. This is a dictionary with key being existing buff id and value being
data model described below. They are used to extend vanilla buffs.

## BuffExtensionData

| Field             | Type                         | Description                                                                                                                                                                                                                              |
|-------------------|------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `StackableData`   | `BuffExtensionStackableData` | *(Optional)* If present, buff can be stackable. Only applies to extra stacks (i.e. any extra stack after the first one, you still need to define `Effects` in `Data/Buffs` and it will be used to add the stats for first stack of buff. |
| `ExtraAttributes` | `ExtraBuffAttributesData`    | *(Optional)* Analoguous to vanilla `BuffAttributesData` (`Effects` in `Data/Buffs`), used to add player stats added by this mod (e.g. `ArmourClass`).                                                                                    |

### BuffExtensionStackableData

| Field                     | Type                      | Description                                                                                                                                                                                      |
|---------------------------|---------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `MaxStacks`               | `int`                     | The maximum amount of stacks. If set to `-2` buff has no stack limit.                                                                                                                            |
| `IndependentStacks`       | `bool`                    | *(Optional)* Whether stacks should be considered as independent, in which case every added stack will have its own timer (by default any added stack renews buff duration). Defaults to `false`. |
| `AttributesPerStack`      | `BuffAttributesData`      | *(Optional)* Used to calculate total added vanilla buff stats with stacks. Applies to extra stacks. Any value used here is multiplied by `current stack amount - 1`.                             |
| `ExtraAttributesPerStack` | `ExtraBuffAttributesData` | *(Optional)* Used to calculate total added mod buff stats (e.g. `ArmourClass`) with stacks. Applies to extra stacks. Any value used here is multiplied by `current stack amount - 1`.            |

### ExtraBuffAttributesData

| Field                    | Type    | Description                                                                                                                                                                                                                                   |
|--------------------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `ArmourClass`            | `int`   | *(Optional)* The player's armour class. It adds `5 * ArmourClass`% chance to dodge monster attack. When calculating dodge chance, it is clamped to \[0, 19\]. For example, 10 armour class will result in 50% chance to dodge monster attack. |
| `FlatCriticalChance`     | `float` | *(Optional)* The extra flat critical strike chance. Applied after `CriticalChanceMultiplier` takes effect but before vanilla Scout profession.                                                                                                |
| `FlatCriticalMultiplier` | `float` | *(Optional)* The extra flat critical strike multiplier. Applied after `CriticalChanceMultiplier` takes effect but before vanilla Scout profession.                                                                                            |

Example:

```json
{
    "Changes": [
        // don't forget to add {{ModID}}_YourCustomBuff buff entry into Data/Buffs
        {
            "Action": "EditData",
            "Target": "Mods/WhyBother11.CombatExtended/BuffExtensionData",
            "Entries": {
                "{{ModID}}_YourCustomBuff": {
                    "ExtraAttributes": {
                        "ArmourClass": 2,
                        "FlatCriticalMultiplier": 2.5
                    },
                    "StackableData": {
                        "MaxStacks": 10,
                        "IndependentStacks": true,
                        "AttributesPerStack": {
                            "Speed": 0.2,
                            "FarmingLevel": 0.5
                        },
                        "ExtraAttributesPerStack": {
                            "ArmourClass": 2,
                            "FlatCriticalMultiplier": 2.5
                        }
                    }
                }
            }
        }
    ]
}
```

### Remarks

Added by mod stats can also be used for food (or edible objects in general) buffs. For this you should target `CustomFields` of entry in `Buffs` list of 
corresponding food's data in `Data/Objects` by including the following keys: `WhyBother11.CombatExtended_ArmourClass`, 
`WhyBother11.CombatExtended_FlatCriticalChance` or `FlatCriticalMultiplier`, each for corresponding extra buff attribute. 
The key (`float` type) is the buff extension stat value. `ArmourClass` buff stat has a custom icon and will be displayed alongside
vanilla buffs that are shown when hovering over buff icon (for example, `Speed`).

Example:

```json
{
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/Objects",
            "Entries": {
                "{{ModID}}_YourItem": {
                    "Name": "{{ModID}}_YourItem",
                    "Edibility": 123,
                    ...
                    "CustomFields": {
                        "WhyBother11.CombatExtended_ArmourClass": "2",
                        "WhyBother11.CombatExtended_FlatCriticalChance": "0.5"
                    }        
                }
            }
        }
    ]
}
```


## Buff triggers

### `WhyBother11.CombatExtended_BuffApplied`

Raised when buff is applied. This trigger passes buff id as target item's id.

## Buff trigger actions

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_ChangeBuffStackLimit`

Format: `WhyBother11.CombatExtended_ChangeBuffStackLimit <BuffId> <#Limit>`

Used to change maximum amount of stacks. `BuffId` corresponds to buff whose maximum amount of stacks should be changed. 
`Limit` should not be negative, unless it is `-2`, in which case maximum stack amount will become unlimited.

### `WhyBother11.CombatExtended_AddArmourClass`

Format: `WhyBother11.CombatExtended_AddArmourClass <#Amount>`

Used to add armour class to current player directly. Any added armour class will be cleared upon day ending. `Amount` specifies
how much armour class should be added.

## Buff game state queries

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_BUFF_STACKS`

Format: `WhyBother11.CombatExtended_BUFF_STACKS <BuffId> <#Min> [#Max]`

Used to check current amount of stacks for given buff. `BuffId` corresponds to the buff whose stack amount should be checked. `Min` and `Max` (defaults to `int` max value) 
are used to define minimum and maximum possible amount of stacks respectively. Query returns `true` if buff's current stack amount 
is between `Min` and `Max` inclusively, `false` otherwise.

### `WhyBother11.CombatExtended_BUFF_MAX_STACKS`

Format: `WhyBother11.CombatExtended_BUFF_MAX_STACKS <BuffId> <#Min> [#Max]`

Used to check current maximum amount of stacks for given buff. `BuffId` corresponds to the buff whose maximum stack amount should be checked. 
`Min` and `Max` (defaults to `int` max value) are used to define minimum and maximum possible amount of max stacks respectively. 
Query returns `true` if buff's current maximum stack amount is between `Min` and `Max` inclusively, `false` otherwise.
