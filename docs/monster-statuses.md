# Monster statuses

Monster statuses are similar to monster buffs. They, too, allow to alter monster's stats, do actions on apply or have 
continuous effect, and display temporary animated sprites for visualising effects. The main difference is that monster
statuses are applied passively and do not have duration. They are applied based on conditions or by some of the mod systems,
but you cannot apply them directly. Monster status data is located in `Mods/WhyBother11.CombatExtended/MonsterStatuses`. 
This is a dictionary with key being monster status ID and value being data model described below.

## MonsterStatusData

| Field                               | Type                          | Description                                                                                                                                                                                                   |
|-------------------------------------|-------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `DisplayName`                       | `string`                      | The text for in-game display name of this monster status.                                                                                                                                                     |
| `Description`                       | `string`                      | The text for in-game description name of this monster status.                                                                                                                                                 |
| `DoEffectEverySec`                  | `float`                       | *(Optional)* The time (in seconds) for monster status effect to be triggered. If set to `0` or not set, no effect is triggered.                                                                               |
| `DoAnimationEverySec`               | `float`                       | *(Optional)* The time (in seconds) for monster status animation sprite to be displayed. If set to `0` or not set, no animation sprites are shown.                                                             |
| `TemporaryAnimatedSprites`          | `List<string>`                | *(Optional)* The list of temporary animated sprites, as defined in `Mods/WhyBother11.CombatExtended/CombatTemporaryAnimatedSprites`, to be displayed when monster status *is applied*.                        |
| `AnimationTemporaryAnimatedSprites` | `List<string>`                | *(Optional)* The list of temporary animated sprites, as defined in `Mods/WhyBother11.CombatExtended/CombatTemporaryAnimatedSprites`, to be displayed *accordingly with* `DoAnimationEverySec`.                |
| `Condition`                         | `string`                      | *(Optional)* The GSQ to check if this monster status should be applied to the monster. This GSQ receives `Monster context`, so you can use any related queries. It is checked every 30 ticks (half a minute). |
| `Attributes`                        | `MonsterStatusAttributesData` | *(Optional)* Describes what monster stats and how much should be altered when monster buff *is applied*. The data model is described below.                                                                   |
| `Effects`                           | `MonsterStatusAttributesData` | *(Optional)* Describes what monster stats and how much should be altered *accordingly with* `DoEffectEverySec`. The data model is described below.                                                            |

## MonsterStatusAttributesData

| Field            | Type                             | Description                                                                                                                                    |
|------------------|----------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| `Resilience`     | `float`                          | *(Optional)* How much should monster's resilience (decreases damage received by monster) be increased.                                         |
| `Health`         | `float`                          | *(Optional)* How many health points should be added to the monster. Applied after `MaxHealth`.                                                 |
| `MaxHealth`      | `float`                          | *(Optional)* How much should monster's maximum health be increased.                                                                            |
| `MissChance`     | `float`                          | *(Optional)* How much should monster's miss chance (how often can weapon attack miss when hitting this monster) be increased.                  |
| `ArmourClass`    | `float`                          | *(Optional)* How much should monster's armour class be increased.                                                                              |
| `Weight`         | `float`                          | *(Optional)* How much should monster's weight be increased.                                                                                    |
| `StunTime`       | `float`                          | *(Optional)* How long (in milliseconds) should the monster be stunned. Can be set to `-2` for indefinite time until monster status is removed. |
| `Speed`          | `float`                          | *(Optional)* How much should monster's speed be increased.                                                                                     |
| `DamageData`     | `List<DamageAttributesData>`     | *(Optional)* Describes the damage that the monster should receive.                                                                             |
| `ResistanceData` | `List<ResistanceAttributesData>` | *(Optional)* Describes what and how much monster's resistances should be altered.                                                              |

## DamageAttributesData

| Field        | Type     | Description                                                                                                                      |
|--------------|----------|----------------------------------------------------------------------------------------------------------------------------------|
| `Id`         | `string` | The unique ID of this entry with the list.                                                                                       |
| `DamageType` | `string` | The damage type from `Mods/WhyBother11.CombatExtended/DamageTypes` for damage this monster status should inflict on the monster. |
| `Damage`     | `float`  | The amount of damage to deal to the monster.                                                                                     |
| `MaxDamage`  | `float`  | *(Optional)* If set, damage is rolled between `Damage` and `MaxDamage` inclusively.                                              |

## ResistanceAttributesData

| Field        | Type     | Description                                                                                                                                                                                                                                                                              |
|--------------|----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Id`         | `string` | The unique ID of this entry with the list.                                                                                                                                                                                                                                               |
| `DamageType` | `string` | The damage type from `Mods/WhyBother11.CombatExtended/DamageTypes` for which monster's resistance should be altered.                                                                                                                                                                     |
| `Delta`      | `float`  | How much monster's resistance should be increased. From the lowest to highest, it is: `Vulnerable`, `Sensitive`, `Normal`, `Enduring`, `Resistant`, `Immune`, meaning that delta `-2` will change resistance from `Resistant` to `Normal`. Altering `Immune` resistance takes no effect. |
