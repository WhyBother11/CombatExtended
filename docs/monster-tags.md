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
