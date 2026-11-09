# Damage modifiers

Damage modifiers data allow to modify the game's damage formula. The data is located in `Mods/WhyBother11.CombatExtended/Modifiers`.
This is a dictionary with key being modifier ID and value being data model described below.

## DamageModifiersData

| Field       | Type                       | Description                                                                                                                                               |
|-------------|----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Condition` | `string`                   | The GSQ to check if this modifier should be applied. This GSQ receives `Monster context`, so you can use any related queries, as well as `IS_CRIT` query. |
| `Modifiers` | `List<DamageModifierData>` | The list of data models describing when and how should damage be modified. The data model is desctibed below.                                             |

## DamageModifierData

| Field        | Type     | Description                                                                                                                                                                                                                                                                                                                                                                                                    |
|--------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Id`         | `string` | The unique ID of this entry withing the list.                                                                                                                                                                                                                                                                                                                                                                  |
| `Stage`      | `string` | The damage calculation stage at which damage modifier should be applied. Supported values and changes to damage calculation are described below in the `Damage calculation` section                                                                                                                                                                                                                            |
| `MinValue`   | `float`  | The minimum value that should be added to the damage or by which damage should be multiplied.                                                                                                                                                                                                                                                                                                                  |
| `MaxValue`   | `float`  | *(Optional)* The maximum value that should be added to the damage or by which damage should be multiplied. If not present, defaults to `MinValue`.                                                                                                                                                                                                                                                             |
| `DamageType` | `string` | *(Optional)* The damage type from `Mods/WhyBother11.CombatExtended/DamageTypes` for damage type that should be targeted by this modifier. For *multiplicative* stages can be set to `_all` which would multiply all damage types that were *previously* added.                                                                                                                                                 |
| `Group`      | `string` | *(Optional)* The group ID that allows for *multiplicative* modifiers withing the same group to add together (i.e if 1.1 and 1.5 modifiers are in the same group, the result multiplier is 1.1 + 1.5 = 1.6 instead of 1.1 * 1.5 = 1.65). As such, ignored on `damageRoll` and `additiveModifier` stages. The default group is `_default`, which is a default value if `Group` is not present in the data model. |

## Damage calculation

The vanilla formula for damage when attacking monsters with weapon is: `damage = (random(minDamage, maxDamage) * <damageRollModifiers> * critMult (if critical strike) + Attack (buff stat) * 3) * <resultModifiers> - MonsterDefense`,
where `minDamage` and maxDamage are taken from the weapon's, `damageRollModifiers` are modifiers from ruby forging, as well as ruby and iridium rings (as two separate modifiers),
critMult is taken from weapon's native critical power multiplier and `CriticalPowerMultiplier` stat, `resiltModifiers` are taken from various sources,
for example enchantment effects and `Fighter` profession, and `MonsterDefense` is corresponding monster stat (also known as `resilience`).

The wiki has more thorough explanation [here](https://stardewvalleywiki.com/Combat/Mechanics#Formula_overview)

The mod allows to modify this formula:
`damage = (((random[minDamage, maxDamage] * <attackModifiers>) + <stage_damageRoll>) * <stageGrouped_damageRollMultiplier> * critMult (if critical strike) + Attack (buff stat) * 3 + <stage_additiveModifier>) * <stageGrouped_multiplicativeModifier> * <resultModifiers> - MonsterDefense`

The new added expression components all start with stage, and value after `_` delimiter is the stage name (`Stage`) used in `DamageModifierData`,
so supported values are `damageRoll`, `damageRollMultiplier`, `additiveModifier` and `multiplicativeModifier`. The grouped in new components
refers to grouped multiplicative modifiers, meaning that multiplicative modifiers that have the same group will add up to the group modifier by which
the value will be multiplied. For example, given groups `(1.1, 1.4)`, `(1.2)`, `(1.05, 1.2, 1.5)`, the result multiplier equals
`(1 + 0.1 + 0.4) * (1 + 0.2) * (1 + 0.05 + 0.2 + 0.5)`.
