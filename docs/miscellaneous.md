# Miscellaneous stuff

This doc contains all the miscellaneous stuff that is does not warrant a separate doc.

## Trigger actions

### `WhyBother11.CombatExtended_LightningStrike`

Format: `WhyBother11.CombatExtended_LightningStrike [RangeId] [Target]`

Used to draw a lightning bolt. If `RangeId` is present, it corresponds to a range from `Mods/WhyBother11.CombatExtended/Ranges`.
The `Target` specifies on top of what type of entity in the range should lightning bolt be drawn. If it is `all` (default value), it is drawn
for every character, if `player`, for every player, and if `monster` for every monster.

If this action is used in `Monster context`, `RangeId` and `Target` may be omitted, in which case the lightning bolt will be drawn on top
of context monster.

### `WhyBother11.CombatExtended_CooldownAction`

Format: `WhyBother11.CombatExtended_CooldownAction <Id> <Time> <Action>`

Used to make actions have cooldown without requiring use of abilities. `Id` is unique ID to associate with given action. `Time` has `float`
type and specifies cooldown time in seconds. `Action` is trigger action that should be run if cooldown expires. Triggering `CooldownAction`
while cooldown is active will not execute any `Action`.

## Game state queries

### `WhyBother11.CombatExtended_MINE_LEVEL`

Format: `WhyBother11.CombatExtended_MINE_LEVEL <Min> [Max]`

Used to check current mine level the current player is on. `Min` and `Max` (defaults to `int` max value) have `int` type and are used to define
minimum and maximum possible level of current mine level. Can be used in skull cavern, with 121st level be its 1st level.
Query returns `true` if current mine level is between `Min` and `Max`, `false` otherwise or if player is not in regular mines or skull cavern.

### `WhyBother11.CombatExtended_VOLCANO_DUNGEON_LEVEL`

Format: `WhyBother11.CombatExtended_VOLCANO_DUNGEON_LEVEL <Min> [Max]`

Used to check current volcano dungeon level the current player is on. `Min` and `Max` (defaults to `int` max value) have `int` type and are used to define
minimum and maximum possible level of current volcano dungeon level. Query returns `true` if current mine level is between `Min` and `Max`, 
`false` otherwise or if player is not in volcano dungeon.
