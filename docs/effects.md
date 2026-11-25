# Effects (temporary animated sprites and other cool stuff)

This doc contains info on different type of effects that can be used in conjunction with other systems.

# Link effects

Links are effects to connect the player and some other entity (currently projectile). Link effect data is located in
`Mods/WhyBother11.CombatExtended/LinkEffects`. This is a dictionary with key being link effect ID and value being data
model described below.

## LinkEffectData

| Field               | Type                          | Description                                                                                                                                                                                                                                                                      |
|---------------------|-------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Texture`           | `string`                      | The texture used for link drawn between target and the player. It is then repeated enough times to create a link, and the construction method is specified by `Type`.                                                                                                            |
| `Type`              | `string`                      | Used to specify how link should be constructed. Can be one of: `Static` (only the first frame in `Frames` will be drawn), `Sequence` (frames will be drawn as specified by `FrameOrder`, looping back if necessary) and `Random` (frames will be chosen randomly on every draw). |
| `Frames`            | `List<int>`                   | The list of indexes that specify positions for link frames within `Texture`.                                                                                                                                                                                                     |
| `Size`              | `Rectangle`                   | *(Optional)* Used to specify the size in pixels of single link segment with `Width` and `Height` (both defalut to `16`).                                                                                                                                                         |
| `Scale`             | `float`                       | *(Optional)* Used to scale link's `Texture`. Defaults to `4`.                                                                                                                                                                                                                    |
| `Duration`          | `float`                       | *(Optional)* The time (in seconds) for how much time should the link effect be drawn and have effect. If set to `-2` will be drawn until manually removed or attached entity is removed. Defaults to `-2`.                                                                       |
| `TriggerEffectData` | `LinkEffectTriggerEffectData` | *(Optional)* Used to specify additional data for trigger effects of this link. The data model is described below.                                                                                                                                                                |

### LinkEffectTriggerEffectData

| Field                          | Type    | Description                                                                                                                                                                                                        |
|--------------------------------|---------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `DoMonsterEffectEverySec`      | `float` | *(Optional)* The time (in seconds) for effect on linked monster to be triggered. As such, only applicable in conjuction with `LinkMonster` trigger action. If set to `0`, no effect is triggered. Defaults to `0`. |
| `DoMonsterTouchEffectEverySec` | `float` | *(Optional)* The time (in seconds) for cooldown on single monster to be affected with link's effects. Applicable to any link effect. If set to `0`, no effect is triggered. Defaults to `0`.                       |

# Triggers, trigger actions, GSQs stuff

## Triggers

### `WhyBother.CombatExtended_LinkEffectTriggered`

Raised when link effect is triggered on linked monster (as per `DoMonsterEffectEverySec` in its data). Provides `Monster context`.

### `WhyBother.CombatExtended_MonsterTouchedLink`

Raised when monster touches some existing link (no less than per `DoMonsterTouchEffectEverySec` seconds). Provides 
`Monster context`.

## Trigger actions

Required arguments are surrounded with `<>`, while optional with `[]`. If optional argument has `*` after it, it is optional
only under certain conditions. If argument has `+` after it, then it can be added multiple times, and if argument is required,
it should be present at least one time. `int` type arguments are prefixed with `#`, `float` with `%`, `bool` with `?`.

### `WhyBother11.CombatExtended_LightningStrike`

Format: `WhyBother11.CombatExtended_LightningStrike [RangeId]* [Target]*`

Used to draw a lightning bolt. `RangeId` corresponds to [range](https://github.com/WhyBother11/CombatExtended/blob/master/docs/ranges.md)
which contains characters on top of which lightning strike should be drawn. `Target` specifies type of those characters:
if it is `all` (default value), lightning strike is drawn for every character, if `player`, for every player, and if `monster`,
for every monster.

If this action is used in `Monster context`, `RangeId` and `Target` may be omitted, in which case the lightning bolt will 
be drawn on top of context monster.

### `WhyBother11.CombatExtended_LinkMonster`

Format: `WhyBother11.CombatExtended_LinkMonster <LinkEffectId> [RangeId]*`

Can be used only in `Monster context`. Used to create a link effect between player and monster. `LinkEffectId` corresponds
to link effect to apply between player and monster. If set to `_none`, the link will be detached from monster. `RangeId`
corresponds to [range](https://github.com/WhyBother11/CombatExtended/blob/master/docs/ranges.md) which contains monsters 
with whom the link effect should be created.

If used in `Monster context`, `RangeId` can be omitted, in which case only context monster will be linked.
