# Monster extensions

Monster extension data is located in `Mods/WhyBother11.CombatExtended/MonsterExtensionData`. This is a dictionary with key being existing monster name (as defined in `Data/Monsters`).
The value is data model described below. They are used to extend vanilla monsters. `assets/monster_extensions.json` already includes entries for
all vanilla monsters plus entry for `Haunted Skull` that is not present in vanilla.

Custom monsters can be added as well. For new monster classes added by C# the key should be monster's `Name`. For monsters added by FarmTypeManager or SpaceCore
the key should be monster's texture name (`Sprite` for FTM and `MonsterTextureOverride` for SpaceCore).

## MonsterExtensionData

| Field              | Type     | Description                                                                                                                                                                                                    |
|--------------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Tags`             | `string` | *(Optional)* The space delimited string of tag IDs that should be applied to this monster.                                                                                                                     |
| `ExtraArmourClass` | `int`    | *(Optional)* The extra armour class for this monster (the default for every monster is `2`). Can be negative. Every armour point corresponds to 5% miss chance that stacks with monster's native `missChance`. |

## Monster triggers

### `WhyBother11.CombatExtended_MonsterDied`

Raised when monster takes any damage and its health drops below 0. Provies `Monster context`.

## Monster game state queries

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_MONSTER_HEALTH`

Format: `WhyBother11.CombatExtended_MONSTER_HEALTH <#Min> [#Max]`

Can be used only in `Monster context`. Used to check monster's current health. `Min` and `Max` (defaults to `int` max value) represent 
minimum and maximum possible amount of monster's current health. In case `Min` is less than or equal to `0`, it is treated as `int` min value
to check if monster health dropped below `0` and not account for possible negative values. Query returns `true` if monster's health is
between `Min` and `Max` inclusively, `false` otherwise.

### `WhyBother11.CombatExtended_MONSTER_HAS_TAG`

Format: `WhyBother11.CombatExtended_MONSTER_HAS_TAG <TagId>+`

Can be used only in `Monster context`. Used to check monster's tags. `TagId` corresponds to multiple possible tag IDs to check against.
Query returns `true` if any `TagId` is equal to ID of one of context monster's tags, `false` otherwise.

### `WhyBother11.CombatExtended_MONSTER_NAME`

Format: `WhyBother11.CombatExtended_MONSTER_NAME <MonsterName>+`

Can be used only in `Monster context`. Used to check monster's name, as defined in `Data/Monsters`. `MonsterName` corresponds to multiple
possible monster names to check against. Query returns `true` if any `MonsterName` is equal to monster name of context monster, `false` otherwise.

### `WhyBother11.CombatExtended_MONSTER_HAS_BUFF`

Format: `WhyBother11.CombatExtended_MONSTER_HAS_BUFF <MonsterBuffId>+`

Can be used only in `Monster context`. Used to check monster's buffs. `MonsterBuffId` corresponds to multiple possible monster buff IDs to
check against. Query returns `true` if any `MonsterBuffId` is equal to one of context monster's currently applied monster buffs, `false` otherwise.

### `WhyBother11.CombatExtended_MONSTER_BUFF_STACKS`

Format: `WhyBother11.CombatExtended_MONSTER_BUFF_STACKS <MonsterBuffId> <#Min> [#Max]`

Can be used only in `Monster context`. Used to check stack amount of given monster buff inflicted upon context monster. `MonsterBuffId` corresponds to monster buff whose
stack amount should be checked. `Min` and `Max` (defaults to `int` max value) represent minimum and maximum possible amount of stack for given monster buff.
Query returns `true` if `MonsterBuffId` is stackable and its stack amount on context monster is between `Min` and `Max` inclusively, `false` otherwise.

### `WhyBother11.CombatExtended_MONSTER_STUN_TIME`

Format: `WhyBother11.CombatExtended_MONSTER_STUN_TIME [%Min] [%Max]`

Can be used only in `Monster context`. Used to check if context monster is currently stunned. `Min` (defaults to `int` min value) specifies
minimum amount of stun time in seconds, `Max` (defaults to `int` max value) specifies maximum amount. To have `Max` argument you should 
also specify `Min`. If no `Min` and `Max` are provided, query returns `true` if monster is currently stunned, `false` otherwise. If they are present,
query returns `true` if monster's stun time is between `Min` and `Max` inclusively, `false` otherwise.

# Monster tags

Monster tags are similar to item context tags. They can be used to broadly categorise monsters, define resistances to different damage types
and apply actions based on tags.

Monster tag data is located in `Mods/WhyBother11.CombatExtended/Tags`. This is a dictionary with key being tag id and value being
data model described below. `assets/tags.json` already includes a lot of built-in tags to use.

## TagData

| Field         | Type                   | Description                                                                                                                                                       |
|---------------|------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Id`          | `string`               | The ID of this tag.                                                                                                                                               |
| `Priority`    | `int`                  | Used to define order in which tags are applied. Entries with lower `Priority` will be applied first, letting entries with higher priority override their changes. |
| `Condition`   | `string`               | *(Optional)* The GSQ to check if this tag should be applied. This should generally be used for the broadest tags, for example tags depending on location.         |
| `Resistances` | `List<ResistanceData>` | *(Optional)* The list of data models to define resistances. The data model is described below.                                                                    |

### ResistanceData

| Field   | Type     | Description                                                                                                                                                                                                                                    |
|---------|----------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Type`  | `string` | The ID of damage type, as defined in `Mods/WhyBother11.CombatExtended/DamageTypes` to which resistance should apply.                                                                                                                           |
| `Value` | `string` | The resistance value for `Type` damage type. Can be one of: `Immune` (takes no damage), `Resistant` (50% of damage), `Enduring` (75% of damage), `Normal` (no damage changes), `Sensitive` (150% of damage) and `Vulnerable` (200% of damage). |
