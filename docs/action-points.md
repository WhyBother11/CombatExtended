# Action points

Action points represent any arbitrary points that can be spent to do actions, replenished manually or overtime. Action point data 
is located in `Mods/WhyBother11.CombatExtended/ActionPoints`. This is a dictionary with key being action point id and value being
data model described below.

## ActionPointData

| Field           | Type     | Description                                                                                                                                  |
|-----------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------|
| `MaxAmount`     | `int`    | The maximum amount of this action point.                                                                                                     |
| `DisplayName`   | `string` | The text for in-game display name of this action point.                                                                                      |
| `Description`   | `string` | The text for in-game description name of this action point.                                                                                  |
| `Texture`       | `string` | The asset name containing icon for this action point.                                                                                        |
| `SpriteIndex`   | `int`    | The index within `Texture` of icon for this action point.                                                                                    |
| `ReplenishTime` | `float`  | The time (in seconds) for one action point to be replenished. If set to `-1`, action points are not replenished with time. Defaults to `-1`. |

Example:

```json
{
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Mods/WhyBother11.CombatExtended/ActionPoints",
            "Entries": {
                "{{ModID}}_MjolnirActionPoint": {
                    "DisplayName": "<Display name of action point>",
                    "Description": "<Description of action point>",
                    "MaxAmount": 100,
                    "ReplenishTime": 30,
                    "Texture": "Mods/{{ModID}}/ActionPoints",
                    "SpriteIndex": 0
                }
            }
        }
    ]
}
```

## Action points trigger actions

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_AddActionPoints`

Format: `WhyBother11.CombatExtended_AddActionPoints <ActionPointId> <#Amount>`

Used to manually add action points. `ActionPointId` corresponds to action point to add to. `Amount` is amount of action points that
should be added, capping at `MaxAmount`. If negative number, action point amount will be subtracted, otherwise added.

### `WhyBother11.CombatExtended_EnableActionPoint`

Format: `WhyBother11.CombatExtended_EnableActionPoint <ActionPointId>`

Used to manually enable action point. `ActionPointId` corresponds to action point to enable. Enabled action points start being shown on UI
and being interacted with.

### `WhyBother11.CombatExtended_DisableActionPoint`

Format: `WhyBother11.CombatExtended_DisableActionPoint <ActionPointId>`

Used to manually disable action point. `ActionPointId` corresponds to action point to disable. Disabled action points stop being shown on UI
and being interacted with.

### `WhyBother11.CombatExtended_SetReplenishTime <ActionPointId> <%ReplenishTime>`

Used to override replenish time of action point. `ActionPointId` corresponds to action point whose replenish time should be overriden.
`ReplenishTime` is overriding amount of seconds for replenish time.

### `WhyBother11.CombatExtended_RestoreReplenishTime <ActionPointId>`

Used to restore replenish time of action point back to the defined `ReplenishTime` in `Mods/WhyBother11.CombatExtended/ActionPoints`. `ActionPointId` corresponds to action point whose replenish time should be restored.

### `WhyBother11.CombatExtended_SetMaxAmount <ActionPointId> <#MaxAmount>`

Used to override maximum amount of action point. `ActionPointId` corresponds to action point whose maximum amount should be overriden.
`MaxAmount` is overriding max amount for replenish time.

### `WhyBother11.CombatExtended_RestoreMaxAmount <ActionPointId>`

Used to restore maximum amount of action point back to the defined `MaxAmount` in `Mods/WhyBother11.CombatExtended/ActionPoints`. 
`ActionPointId` corresponds to action point whose maximum amount should be restored.

## Action points game state queries

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_ACTION_POINTS <ActionPointId> <#Min> [#Max]`

Used to check current amount of action points. `ActionPointId` corresponds to action point whose amount should be checked.
`Min` and `Max` (defaults to `int` max value) are used to define minimum and maximum possible amount of action points, respectively.
Query returns `true` if action point amount is between `Min` and `Max` inclusively, `false` otherwise.

### `WhyBother11.CombatExtended_MAX_ACTION_POINTS <ActionPointId> <#Min> [#Max]`

Used to check current maximum amount of action points. `ActionPointId` corresponds to action point whose maximum amount should be checked.
`Min` and `Max` (defaults to `int` max value) are used to define minimum and maximum possible amount of maximum amount of action points, respectively.
Query returns `true` if maximum action point amount is between `Min` and `Max` inclusively, `false` otherwise.
