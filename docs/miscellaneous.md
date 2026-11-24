# Miscellaneous stuff

This doc contains all the miscellaneous stuff that is does not warrant a separate doc.

## Sequences

Some of the trigger actions added by the mod can have certain effect for some time. You may want to do some trigger actions
sequentially, but after said effects end. For example, for `WhyBother11.CombatExtended_DragMonster` trigger action you may
want to execute other trigger actions once pulling is over.

Sequences are subsequent lists of trigger actions that can be executed sequentially. The trigger action that has asynchronous
effect which may end after some time may be marked as `Sequence aware`. When such action is triggered inside the sequence,
the next trigger action in sequence will be executed once the `Sequence aware` action's effect is ended. Non-aware trigger
actions are still handled sequentially, but they will be executed on the same game tick that the previous one.

# Triggers, trigger actions, GSQs stuff

## Trigger actions

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_Sequence <Action1> ## [Action2] ## ... ## [ActionN]`

Used to start a sequence. `Action1`, `Action2`, all the way to `ActionN` (that is, arbitrary `N` amount of trigger actions) 
describe what trigger actions should be executed sequentially. For more information on sequences refer to the above paragraph.

### `WhyBother11.CombatExtended_CooldownAction`

Format: `WhyBother11.CombatExtended_CooldownAction <Id> <%Time> <Action>`

Used to make actions have cooldown without requiring use of abilities. `Id` is unique ID to associate with given action.
`Time` specifies cooldown time in seconds. `Action` is trigger action that should be run if cooldown is not active. Triggering
`CooldownAction` while cooldown is active will not execute any `Action`.

### `WhyBother11.CombatExtended_DelayedAction`

Format: `WhyBother11.CombatExtended_DelayedAction <%Delay> <Action>`

Used to execute action after some delay. `Delay` specifies delay time in seconds. `Action` is trigger action that should
be run after `Delay` seconds passed. This action passes its context to `Action` (that is, if used in `Monster context` any
`Action` will receive `Monster context` as well).

### `WhyBother11.CombatExtended_DragMonster`

Format: `WhyBother11.CombatExtended_DragMonster <Direction> <%Power> [Distance] [?IgnoreWeight] [RangeId]*`

Can be used only in `Monster` context. `Sequence aware` trigger action. Used to drag a monster in some direction. `Direction` is the direction in which monster
should be dragged. It can be one of the following:

- If set to `_toPlayer`, monster will be dragged towards the player.
- If set to `_fromPlayer`, monster will be dragged from the player.
- Otherwise, it should have `float` type and be an angle in degrees according to which monster should be displaced.

`Power` is how fast monster should be dragged. `Distance` specifies how many tiles should monster be displaced. If `Direction` 
is `_toPlayer`, it can be set to `_total`, in which case the distance will be approximate distance between player and the 
monster. Otherwise, `Distance` should have `float` type and specify distance in tiles (defaults to `5`). `IgnoreWeight` 
(defaults to `true`) specifies if monster's weight should be used to adjust `Power` (that is, the provided `Power` is used 
as a base and is increased or decreased based on monster's weight).

### `WhyBother11.CombatExtended_SetMoving`

Format: `WhyBother11.CombatExtended_SetMoving <?CanMove>`

Used to toggle whether player can move. If `CanMove` is `true`, set player to be able to move, otherwise prevent it.

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
