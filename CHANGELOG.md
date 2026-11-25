# Changelog

## 0.9.0

### Added

- Links effects can now have effect on linked monsters and when they touch the link. Specified by `TriggerEffectData` field
in `LinkEffectData`.
- `HitRate` extra stat for monsters.
- `RangeTileData_Circle` data model to specify building circles for range builder.

### Changed

- Adjusted range circle calculation to not include stray extremum points and be like in vanilla circle ranges (e.g.,
scarecrow ranges).
- `Radius` field in `RangeTileData` to `Circles`, specifying list of range circles.
- `StunTime` field in `MonsterBuffAttributesData` to be time in seconds.
- Renamed following triggers: `MonsterBuffContinuousEffect` to `MonsterBuffEffectTriggered`, `ProjectileMonsterHit` to 
`ProjectileHitMonster`.

## 0.8.0

### Added

- Sequences of trigger actions that are executed one after another and that take into account the continuous actions (that
is, animations and other type that can not be just executed on one tick). As well as `WhyBother11.CombatExtended_Sequence` 
trigger action to launch such sequences.
- `WhyBother11.CombatExtended_DelayedAction` for delayed trigger actions that allow them to receive the same context.
- `WhyBother11.CombatExtended_SetMoving` to toggle whether player should move.
- `LinkEffects` data asset for creating link effects between players and different targets.
- `LinkEffect` and `DestroyOnWarp` fields into `ProjectileData`.

### Fixed

- Monster's armour class not correctly applied.

### Changed

- `WhyBother11.CombatExtended_LaunchProjectile` arguments to include rotation.

### Removed

- `Rotation` field from `ProjectileData`.

## 0.7.0

### Added

- Mouse wheel support for ability hotbar, as well as next ability keybinds to select next ability.
- `WhyBother11.CombatExtended_AbilityEffectTriggered` trigger for `Continuous` ability effects.
- `ExtraWeight` stat for monsters.
- `WhyBother11.CombatExtended_PullMonster` trigger action to pull monsters in some direction.
- `MonsterStatuses` data asset to define monster statuses applied passively based on conditions and by other mod systems.
- `DoEffectEverySec` and `ActionsOnEffect` fields for `BuffExtensionData` for player buffs to have timed effects akin to
monster buffs.
- Trigger related fields to `AbilityData` as alternative to ability related triggers for less text-heavy ability effects 
and related trigger actions.

### Changed

- `MonsterBuffData` and `CombatTemporaryAnimatedSpriteData` fields.
- `Max` argument in trigger actions `AddPlayerHealth` and `DamageMonster` is now optional.

## 0.6.1

### Added

- `Angle` field into `AbilityData` for `Throw` type.

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
- `Id` field to `DamageTypeWeaponExtensionData`, `RangeTileData`, `RangeTileData_Rectangle`, `RangeTileData_Line` and 
`RangeTileData_CursorLine` so they may be targetable by CP.

### Changed
- Default colours for damage types.

### Removed

- `Id` field from `ProjectileData`.

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