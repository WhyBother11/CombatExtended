# Changelog

## 0.6.1

### Changed

- `WhyBother11.CombatExtended_DamageMonster` trigger action's arguments.
- Extracted launching projectile towards closest monster from trigger action `WhyBother11.CombatExtended_LaunchProjectile`
into new trigger action `WhyBother11.CombatExtended_LaunchProjectileMonsterClosest`.
- `WhyBother11.CombatExtended_DAMAGE_IS_CRIT` and `WhyBother11.CombatExtended_DAMAGE_RESULT` GSQ's arguments.

### Removed

- `WhyBother.CombatExtended_AlterResistance` trigger action.

## 0.6.0

### Added

- `DamageModifiers` data asset to define damage modifiers used on various stages of damage calculation.
- Removed `Id` field from `ProjectileData`, added `Id` field to `DamageTypeWeaponExtensionData`, `RangeTileData`, `RangeTileData_Rectangle`,
`RangeTileData_Line` and `RangeTileData_CursorLine` so they may be targetable by CP.
- Changed default colours for damage types.

## 0.5.0

### Added

- `Abilities` data asset for ability data, defining its type (`Default`, `Continuous` for abilities with duration and `Throw`
for abilities to throw items), cooldown and more, as well as various related triggers, trigger actions and game state queries.
- `AbilityHotbar` UI element to display abilities and interact with them.
- `ActionPoints` data asset for action point data, as well as various related trigger actions and game state queries.
- `ActionPointIcons` UI element to display action points.
- `BuffExtensionDataAsset` data asset to extend buffs with added buff stats (`ArmourClass` for player dodge chance, `FlatCriticalChance`
for critical chance added on top of used in calculations, `FlatCriticalMultiplier` for critical multiplier added on top of
used in calculations) and being stackable. As well as various related triggers, trigger actions and game state queries.
- `CombatTemporaryAnimatedSprites` data asset for using temporary animated sprites with monster buffs.
- `DamageTypes` data asset for different damage types, also containing default ones.
- `MonsterBuffs` data asset for monster buff data, defining its type (`Default`, `Stackable` for buffs with stacks), stats added 
to the monster while buff is active, including added stats. As well as various related triggers, trigger actions and game state queries.
- `MonsterExtensionData` data asset to extend monsters with tags and added monster stats (`ArmourClass` for monster miss chance), 
as well as various related triggers and game state queries.
- `Projectiles` data asset for custom monster hitting projectiles, as well as various related triggers, trigger actions and game state queries.
- `Ranges` data asset for defining ranges used primarily with added actions that specify its effect range.
- `Tags` data asset for assigning monsters different tags, also containing default ones.
- `WeaponExtensionData` data asset to extend weapons with added systems (tied to weapon abilities and action points) and different states,
as well as various related trigger actions and game state queries.
- Various triggers, trigger actions and game state queries related to dealing damage to monsters.
- Various events for use with C#, related to weapon changing and damage dealing.
- Possible other thing I forgot to mention.