# Effects (temporary animated sprites and other cool stuff)

This doc contains info on different type of effects that can be used in conjunction with other systems.

# Link effects

Links are effects to connect the player and some other entity (currently projectile). Link effect data is located in
`Mods/WhyBother11.CombatExtended/LinkEffects`. This is a dictionary with key being link effect ID and value being data
model described below.

## LinkEffectData

| Field     | Type        | Description                                                                                                                                                                                                                                                                      |
|-----------|-------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Texture` | `string`    | The texture used for link drawn between target and the player. It is then repeated enough times to create a link, and the construction method is specified by `Type`.                                                                                                            |
| `Type`    | `string`    | Used to specify how link should be constructed. Can be one of: `Static` (only the first frame in `Frames` will be drawn), `Sequence` (frames will be drawn as specified by `FrameOrder`, looping back if necessary) and `Random` (frames will be chosen randomly on every draw). |
| `Frames`  | `List<int>` | The list of indexes that specify positions for link frames within `Texture`.                                                                                                                                                                                                     |
| `Size`    | `Rectangle` | *(Optional)* Used to specify the size in pixels of single link segment with `Width` and `Height` (both defalut to `16`).                                                                                                                                                         |
| `Scale`   | `float`     | *(Optional)* Used to scale link's `Texture`. Defaults to `4`.                                                                                                                                                                                                                    |

# Triggers, trigger actions, GSQs stuff

### `WhyBother11.CombatExtended_LightningStrike`

Format: `WhyBother11.CombatExtended_LightningStrike [RangeId]* [Target]*`

Used to draw a lightning bolt. `RangeId` corresponds to a range from `Mods/WhyBother11.CombatExtended/Ranges`.
The `Target` specifies on top of what type of entity in the range should lightning bolt be drawn. If it is `all` (default value), 
it is drawn for every character, if `player`, for every player, and if `monster` for every monster.

If this action is used in `Monster context`, `RangeId` and `Target` may be omitted, in which case the lightning bolt will 
be drawn on top of context monster.

### `WhyBother11.CombatExtended_LinkMonster <LinkEffectId>`
