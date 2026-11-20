# Monster buffs

Monster buffs allow to alter monster's stats, do actions on apply or have continuous effect, and display temporary animated 
sprites for visualising effects. Monster buff data is located in `Mods/WhyBother11.CombatExtended/MonsterBuffs`. This is a 
dictionary with key being monster buff ID and value being data model described below.

## MonsterBuffData

| Field                               | Type                        | Description                                                                                                                                                                                    |
|-------------------------------------|-----------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Type`                              | `string`                    | The type of this monster buff. Can be `Default` or `Stackable`.                                                                                                                                |
| `DisplayName`                       | `string`                    | The text for in-game display name of this monster buff.                                                                                                                                        |
| `Description`                       | `string`                    | The text for in-game description name of this monster buff.                                                                                                                                    |
| `Duration`                          | `float`                     | The time (in seconds) for monster buff to be considered active and do its effect.                                                                                                              |
| `IsDebuff`                          | `bool`                      | Whether this monster buff should be considered as debuff.                                                                                                                                      |
| `DoEffectEverySec`                  | `float`                     | *(Optional)* The time (in seconds) for monster buff effect to be triggered. If set to `0` or not set, no effect is triggered.                                                                  |
| `DoAnimationEverySec`               | `float`                     | *(Optional)* The time (in seconds) for monster buff animation sprite to be displayed. If set to `0` or not set, no animation sprites are shown.                                                |
| `TemporaryAnimatedSprites`          | `List<string>`              | *(Optional)* The list of temporary animated sprites, as defined in `Mods/WhyBother11.CombatExtended/CombatTemporaryAnimatedSprites`, to be displayed when monster buff *is applied*.           |
| `AnimationTemporaryAnimatedSprites` | `List<string>`              | *(Optional)* The list of temporary animated sprites, as defined in `Mods/WhyBother11.CombatExtended/CombatTemporaryAnimatedSprites`, to be displayed *accordingly with* `DoAnimationEverySec`. |
| `ApplyCondition`                    | `string`                    | *(Optional)* The GSQ to check if this monster buff should be applied to the monster. This GSQ receives `Monster context`, so you can use any related queries.                                  |
| `Attributes`                        | `MonsterBuffAttributesData` | *(Optional)* Describes what monster stats and how much should be altered when monster buff *is applied*. The data model is described below.                                                    |
| `Effects`                           | `MonsterBuffAttributesData` | *(Optional)* Describes what monster stats and how much should be altered *accordingly with* `DoEffectEverySec`. The data model is described below.                                             |
| `StackableData`                     | `MonsterBuffStackableData`  | Describes additional data related for `Stackable` monster buffs and required for them, otherwise ignored.                                                                                      |

## MonsterBuffStackableData

| Field                | Type                        | Description                                                                                                                                                                                      |
|----------------------|-----------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `MaxStacks`          | `int`                       | The maximum amount of stacks. If set to `-2` buff has no stack limit.                                                                                                                            |
| `IndependentStacks`  | `bool`                      | *(Optional)* Whether stacks should be considered as independent, in which case every added stack will have its own timer (by default any added stack renews buff duration). Defaults to `false`. |
| `AttributesPerStack` | `MonsterBuffAttributesData` | *(Optional)* Used to calculate total added stats with stacks when buff is applied. Applies to extra stacks. Any value used here is multiplied by `current stack amount - 1`.                     |
| `EffectsPerStack`    | `MonsterBuffAttributesData` | *(Optional)* Used to calculate total added stats with stacks when effect is triggered. Applies to extra stacks. Any value used here is multiplied by `current stack amount - 1`.                 |

## MonsterBuffAttributesData

| Field            | Type                             | Description                                                                                                                                  |
|------------------|----------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| `Resilience`     | `float`                          | *(Optional)* How much should monster's resilience (decreases damage received by monster) be increased.                                       |
| `Health`         | `float`                          | *(Optional)* How many health points should be added to the monster. Applied after `MaxHealth`.                                               |
| `MaxHealth`      | `float`                          | *(Optional)* How much should monster's maximum health be increased.                                                                          |
| `MissChance`     | `float`                          | *(Optional)* How much should monster's miss chance (how often can weapon attack miss when hitting this monster) be increased.                |
| `ArmourClass`    | `float`                          | *(Optional)* How much should monster's armour class be increased.                                                                            |
| `Weight`         | `float`                          | *(Optional)* How much should monster's weight be increased.                                                                                  |
| `StunTime`       | `float`                          | *(Optional)* How long (in milliseconds) should the monster be stunned. Can be set to `-2` for indefinite time until monster buff is removed. |
| `Speed`          | `float`                          | *(Optional)* How much should monster's speed be increased.                                                                                   |
| `DamageData`     | `List<DamageAttributesData>`     | *(Optional)* Describes the damage that the monster should receive.                                                                           |
| `ResistanceData` | `List<ResistanceAttributesData>` | *(Optional)* Describes what and how much monster's resistances should be altered.                                                            |

## DamageAttributesData

| Field        | Type     | Description                                                                                                                    |
|--------------|----------|--------------------------------------------------------------------------------------------------------------------------------|
| `Id`         | `string` | The unique ID of this entry with the list.                                                                                     |
| `DamageType` | `string` | The damage type from `Mods/WhyBother11.CombatExtended/DamageTypes` for damage this monster buff should inflict on the monster. |
| `Damage`     | `float`  | The amount of damage to deal to the monster.                                                                                   |
| `MaxDamage`  | `float`  | *(Optional)* If set, damage is rolled between `Damage` and `MaxDamage` inclusively.                                            |

## ResistanceAttributesData

| Field        | Type     | Description                                                                                                                                                                                                                                                                              |
|--------------|----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Id`         | `string` | The unique ID of this entry with the list.                                                                                                                                                                                                                                               |
| `DamageType` | `string` | The damage type from `Mods/WhyBother11.CombatExtended/DamageTypes` for which monster's resistance should be altered.                                                                                                                                                                     |
| `Delta`      | `float`  | How much monster's resistance should be increased. From the lowest to highest, it is: `Vulnerable`, `Sensitive`, `Normal`, `Enduring`, `Resistant`, `Immune`, meaning that delta `-2` will change resistance from `Resistant` to `Normal`. Altering `Immune` resistance takes no effect. |

Example for monster buff with `Stackable` type:

```json
{
    "Changes": [
        {
            "Entries": {
                "{{ModID}}_YourMonsterBuff": {
                    "Type": "Stackable",
                    "DisplayName": "Example",
                    "Description": "ExampleMonsterBuff",
                    "Duration": 100,
                    "IsDebuff": "true",
                    "TemporaryAnimatedSprites": [
                        "{{ModID}}_TAS"
                    ],
                    "Attributes": {
                        "Resilience": -5,
                        "Speed": -1
                    },
                    "Effects": {
                        "DamageData": [
                            {
                                "Id": "PoisonDamage",
                                "DamageType": "Poison",
                                "Damage": 5,
                                "MaxDamage": 10
                            }
                        ]
                    },
                    "StackableData": {
                        "MaxStacks": 5,
                        "AttributesPerStack": {
                            "ArmourClass": -0.2
                        }
                    },
                    "ContextTags": [
                        "cool_debuff"
                    ]
                }
            }
        }
    ]
}
```

# Triggers, trigger actions, GSQs stuff

## Monster buff triggers

### `WhyBother11.CombatExtended_MonsterBuffApplied`

Raised when monster buff is applied on monster. Provides `Monster context` and `MonsterBuffContext`.

### `WhyBother11.CombatExtended_MonsterBuffExpired`

Raised when monster buff on monster expires. Provides `Monster context` and `MonsterBuffContext`.

### `WhyBother11.CombatExtended_MonsterBuffRemoved`

Raised when monster buff is removed from monster by any other means except from natural expiration. Provides `Monster context` 
and `MonsterBuffContext`. When buff is removed `MonsterBuffExpired` is also raised afterwards.

### `WhyBother11.CombatExtended_MonsterBuffContinuousEffect`

Raised when monster buff triggers its effects. Provides `Monster context` and `MonsterBuffContext`.

## Monster buff trigger actions

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_ApplyMonsterBuff`

Format: `WhyBother11.CombatExtended_ApplyMonsterBuff <MonsterBuffId> [RangeId]* [%Chance]`

Used to apply monster buff on monster(s).
