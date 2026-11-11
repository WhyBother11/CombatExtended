# Abilities

Abilities are actions that can have cooldowns, duration, different effects. Ability data is located in `Mods/WhyBother11.CombatExtended/Abilities`.
This is a dictionary with key being ability id and value being data model described below.

For use with weapons, abilities were designed with the intention of being tied to only one weapon. They can be added to multiple
weapons but the stability of doing so is not guaranteed.

## AbilityData

The following table describes shared fields for ability with `Default` type and any other types.

| Field              | Type           | Description                                                                                                                                                                                           |
|--------------------|----------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Type`             | `string`       | The type of this ability. Can be `Default`, `Continuous` or `Throw`.                                                                                                                                  |
| `DisplayName`      | `string`       | The text for in-game display name of this ability.                                                                                                                                                    |
| `Description`      | `string`       | The text for in-game description name of this ability.                                                                                                                                                |
| `Cooldown`         | `float`        | The time (in seconds) for next ability trigger to be available.                                                                                                                                       |
| `Texture`          | `string`       | The asset name containing icon for this ability. The icon should be 16x16.                                                                                                                            |
| `SpriteIndex`      | `float`        | The index within `Texture` of icon for this ability.                                                                                                                                                  |
| `HighlightRangeId` | `string`       | *(Optional)* The ID of range from `Mods/WhyBother11.CombatExtended/Range` that should be highlighted when ability is selected and highlight buttons are pressed. If omitted, no range is highlighted. |
| `ContextTags`      | `List<string>` | *(Optional)* The list of ability's context tags, similar to item's context tags.                                                                                                                      |

The following table describes fields for ability with `Continuous` type. These abilities can have duration, during which 
they count as active. After `Duration` ends, `Cooldown` starts to tick.

| Field              | Type    | Description                                                                                                                    |
|--------------------|---------|--------------------------------------------------------------------------------------------------------------------------------|
| `Duration`         | `float` | The text for in-game display name of this ability.                                                                             |
| `AttachedToWeapon` | `bool`  | *(Optional)* Whether ability's effect should continue to trigger if associated weapon is unequipped. Defaults to `false`.      |
| `DoEffectEverySec` | `float` | *(Optional)* The time (in seconds) for ability effect to be triggered. If set to `0`, no effect is triggered. Defaults to `0`. |

The following table describes fields for ability with `Throw` type. These abilities take item from player's inventory and launch
a projectile with this item's appearance towards specified direction.

| Field          | Type     | Description                                                                                                                                                                                                                                                                                                                       |
|----------------|----------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `ProjectileId` | `string` | ID of projectile from `Mods/WhyBother11.CombatExtended/Projectiles` that corresponds to thrown item projectile. This projectile data *has* to include `ItemId` field, that is also used to take item from inventory.                                                                                                              |
| `Returnable`   | `float`  | *(Optional)* Whether the thrown item should return to player's inventory when associated projectile is destroyed. If set to `false`, the thrown item is dropped as debris once projectiel is gone. Defaults to `false`.                                                                                                           |
| `ReturnOnWarp` | `bool`   | *(Optional)* Whether the thrown item should return to player's inventory when the associated projectile is flying or it is destroyed and the item debris are spawned. This is primarily intended for the items launched in the Mines and prevents losing them. Takes no effect if `Returnable` set to `true`. Defaults to `true`. |
| `Angle`        | `float`  | *(Optional)*  Specifies angle (in degrees), relative to the vector pointed up (e.g. `0` will launch projectile up, while `90` in the right direction). If omitted projectile will be fired towards cursor.                                                                                                                        |

Example for ability with `Continuous` type:

```json
{
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Mods/WhyBother11.CombatExtended/Abilities",
            "Entries": {
                "{{ModID}}_YourAbility": {
                    "Type": "Continuous",
                    "DisplayName": "Example",
                    "Description": "ExampleAbility",
                    "Cooldown": 30,
                    "Texture": "<Ability Texture Asset Name>",
                    "SpriteIndex": 1,
                    "HighlightRangeId": "{{ModID}}_YourRange",
                    "ContextTags": [
                        "cool_ability"
                    ],
                    "Duration": 30,
                    "DoEffectEverySec": 5
                }
            }
        }
    ]
}
```

## Ability triggers

### `WhyBother11.CombatExtended_AbilityTriggered`

Raised when ability is triggered. Provides `Ability context`.

### `WhyBother11.CombatExtended_AbilityCooldownEnded`

Raised when ability's cooldown ends and it is available for trigger. Provides `Ability context`.

### `WhyBother11.CombatExtended_AbilityEnded`

Raised when `Continuous` ability's duration ends. Provides `Ability context`.

## Ability trigger actions

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_AddWeaponAbility`

Format: `WhyBother11.CombatExtended_AddWeaponAbility <AbilityId> <WeaponId>`

Used to manually add ability tied to a weapon. `AbilityId` corresponds to ability that should be added. `WeaponId` corresponds
to weapon to which this ability should be tied. Added ability will behave as if it was added in `Mods/WhyBother11.CombatExtended/WeaponExtensionData`.

### `WhyBother11.CombatExtended_RemoveWeaponAbility`

Format: `WhyBother11.CombatExtended_RemoveWeaponAbility <AbilityId> <WeaponId>`

Used to manually remove ability tied to a weapon. Stability of removing ability defined in `Mods/WhyBother11.CombatExtended/WeaponExtensionData`
is not guaranteed. `AbilityId` corresponds to ability that should be removed. `WeaponId` corresponds to weapon from 
which this ability will be removed.

### `WhyBother11.CombatExtended_SetAbilityCooldown`

Format: `WhyBother11.CombatExtended_SetAbilityCooldown <AbilityId> <%Cooldown>`

Used to override cooldown of ability. `AbilityId` corresponds to ability whose cooldown should be overriden. `Cooldown` is
overriding amount of seconds for cooldown. Cooldown cannot be non-negative.

### `WhyBother11.CombatExtended_RestoreAbilityCooldown`

Format: `WhyBother11.CombatExtended_RestoreAbilityCooldown <AbilityId>`

Used to restore cooldown of ability back to the defined `Cooldown` in `Mods/WhyBother11.CombatExtended/Abilities`.
`AbilityId` corresponds to ability whose cooldown should be restored.

### `WhyBother11.CombatExtended_SetAbilityDuration`

Format: `WhyBother11.CombatExtended_SetAbilityDuration <AbilityId> <%Duration>`

Used to override duration of `Continuous` ability. `AbilityId` corresponds to ability whose duration should be overriden. `Duration` is
overriding amount of seconds for duration. Duration cannot be non-negative.

### `WhyBother11.CombatExtended_RestoreAbilityDuration`

Format: `WhyBother11.CombatExtended_RestoreAbilityDuration <AbilityId>`

Used to restore duration of `Continuous` ability back to the defined `Duration` in `Mods/WhyBother11.CombatExtended/Abilities`.
`AbilityId` corresponds to ability whose duration should be restored.

## Ability game state queries

### `WhyBother11.CombatExtended_ABILITY_ID`

Format: `WhyBother11.CombatExtended_ABILITY_ID <AbilityId>+`

Can be used only in `Ability context`. Used to check ID of ability in context. `AbilityId` corresponds to multiple possible 
ability IDs to check against. Query returns `true` if any `AbilityId` is equal to ID of ability in context, `false` otherwise.

### `WhyBother11.CombatExtended_CONTEXT_TAG`

Format: `WhyBother11.CombatExtended_CONTEXT_TAG <ContextTag>+`

Can be used only in `Ability context`. Used to check context tags of ability in context. `ContextTag` corresponds to 
multiple possible context tags to check against. Query returns `true` if any `ContextTag` is equal to one of context tag 
of ability in context, `false` otherwise.

### `WhyBother11.CombatExtended_IS_ACTIVE`

Format: `WhyBother11.CombatExtended_IS_ACTIVE <AbilityId>+`

Used to check currently active `Continuous` abilities. `AbilityId` corresponds to multiple possible ability IDs to check against.
Ability is checked across all abilities, which includes current weapon ability, previous weapons' abilities if they are not tied
to them, and player abilities. Query returns `true` if any ability with ID equal to `AbilityId` is currently active, `false` otherwise.
