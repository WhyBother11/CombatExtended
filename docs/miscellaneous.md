# Miscellaneous stuff

This doc contains all the miscellaneous stuff that is does not warrant a separate doc.

## Trigger actions

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_LightningStrike`

Format: `WhyBother11.CombatExtended_LightningStrike [RangeId]* [Target]*`

Used to draw a lightning bolt. `RangeId` corresponds to a range from `Mods/WhyBother11.CombatExtended/Ranges`.
The `Target` specifies on top of what type of entity in the range should lightning bolt be drawn. If it is `all` (default value), it is drawn
for every character, if `player`, for every player, and if `monster` for every monster.

If this action is used in `Monster context`, `RangeId` and `Target` may be omitted, in which case the lightning bolt will be drawn on top
of context monster.

### `WhyBother11.CombatExtended_CooldownAction`

Format: `WhyBother11.CombatExtended_CooldownAction <Id> <%Time> <Action>`

Used to make actions have cooldown without requiring use of abilities. `Id` is unique ID to associate with given action.
`Time` specifies cooldown time in seconds. `Action` is trigger action that should be run if cooldown is not active. Triggering
`CooldownAction` while cooldown is active will not execute any `Action`.

### `WhyBother11.CombatExtended_DragMonster`

Format: `WhyBother11.CombatExtended_DragMonster <Direction> <%Power> [Distance] [?IgnoreWeight]`

Can be used only in `Monster` context. Used to drag a monster in some direction. `Direction` is the direction in which monster
should be dragged. If set to `_toPlayer` monster will be dragged towards the player, if set to `_fromPlayer` — from the 
player, otherwise it should have `float` type and be an angle in degrees according to which monster should be displaced. 
`Power` is how fast monster should be dragged. `Distance` specifies how many tiles should monster be displaced. If `Direction` 
is `_toPlayer`, it can be set to `_total`, in which case the distance will be approximate distance between player and the 
monster. Otherwise, `Distance` should have `float` type and specify distance in tiles (defaults to `5`). `IgnoreWeight` 
(defaults to `false`) specifies if monster's weight should be used to adjust `Power` (that is, the provided `Power` is used 
as a base and is increased or decreased based on monster's weight).

## Game state queries

### `WhyBother11.CombatExtended_MINE_LEVEL`

Format: `WhyBother11.CombatExtended_MINE_LEVEL <#Min> [#Max]`

Used to check current mine level the current player is on. `Min` and `Max` (defaults to `int` max value) are used to define
minimum and maximum possible level of current mine level. Can be used in skull cavern, with 121st level being its 1st level.
Query returns `true` if current mine level is between `Min` and `Max`, `false` otherwise or if player is not in regular 
mines or skull cavern.

### `WhyBother11.CombatExtended_VOLCANO_DUNGEON_LEVEL`

Format: `WhyBother11.CombatExtended_VOLCANO_DUNGEON_LEVEL <#Min> [#Max]`

Used to check current volcano dungeon level the current player is on. `Min` and `Max` (defaults to `int` max value) are used 
to define minimum and maximum possible level of current volcano dungeon level. Query returns `true` if current mine level 
is between `Min` and `Max`, `false` otherwise or if player is not in volcano dungeon.
