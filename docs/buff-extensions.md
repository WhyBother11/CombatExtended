_## Buff extensions

Buff extension data is located in `Mods/WhyBother11.CombatExtended/BuffExtensionData`. This is a dictionary with key being existing buff id and value being
data model described below. They are used to extend vanilla buffs._

### BuffExtensionData

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

| Field         | Type                      | Description                                                                                                                                                                                                                                   |
|---------------|---------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `ArmourClass` | `int`                     | *(Optional)* The player's armour class. It adds `5 * ArmourClass`% chance to dodge monster attack. When calculating dodge chance, it is clamped to \[0, 19\]. For example, 10 armour class will result in 50% chance to dodge monster attack. |

### Buff triggers

All triggers should be prepended with `WhyBother11.CombatExtended_`. To not clutter documentation, it is omitted here.

### `BuffApplied`

Raised when buff is applied. This trigger passes buff id as target item's id.

### Buff trigger actions

All trigger actions should be prepended with `WhyBother11.CombatExtended_`. To not clutter documentation, it is omitted here.
Required arguments are surrounded with `<>`, while optional with `[]`.

### `ChangeBuffStackLimit`

Format: `ChangeBuffStackLimit <BuffId> <Limit>`

Used to change maximum amount of stacks. `BuffId` corresponds to buff whose maximum amount of stacks should be changed. `Limit` has `int` type and should not be
negative, unless it is `-2`, in which case maximum stack amount will become unlimited.

### Buff game state queries

All game state queries should be prepended with `WhyBother11.CombatExtended_`. To not clutter documentation, it is omitted here.
Required arguments are surrounded with `<>`, while optional with `[]`. If argument has `+` after it, then it can be added multiple times, and if
argument is required, it should be present at least one time.

### `BUFF_STACKS`

Format: `BUFF_STACKS <BuffId> <Min> [Max]`

Used to check current amount of stacks for given buff. `BuffId` corresponds to the buff whose stack amount should be checked. `Min` and `Max` (defaults to `int` max value) have `int` type
and are used to define minimum and maximum possible amount of stacks respectively. Query returns `true` if buff's current stack amount is between `Min` and `Max`

### `BUFF_MAX_STACKS`

Format: `BUFF_MAX_STACKS <BuffId> <Min> [Max]`

Used to check current maximum amount of stacks for given buff. `BuffId` corresponds to the buff whose maximum stack amount should be checked. `Min` and `Max` (defaults to `int` max value) have `int` type
and are used to define minimum and maximum possible amount of max stacks respectively. Query returns `true` if buff's current maximum stack amount is between `Min` and `Max`
