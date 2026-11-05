## Damage types

Damage type data is located in `Mods/WhyBother11.CombatExtended/DamageTypes`. This is a dictionary with key being damage type id and value being
data model described below. By default, has `Physical` (applies to any weapon attack damage), `Ice`, `Fire`, `Lightning`, `Radiant` and `Poison` entries 
(as defined in `assets/damage_types.json`).

### DamageTypeData

| Field         | Type     | Description                                                                                                                                                                                                                                                                            |
|---------------|----------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `DisplayName` | `string` | The name for this damage type to display.                                                                                                                                                                                                                                              |
| `Color`       | `string` | The colour that should be used to display damage numbers when dealing damage with this damage type. If it starts with `-1`, the vanilla damage color is used. Otherwise it should contain three different numbers in RGB format (for example, `255 0 0` will correspond to red colour. |