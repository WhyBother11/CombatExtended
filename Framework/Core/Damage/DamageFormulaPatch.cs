using System.Reflection.Emit;
using CombatExtended.Framework.Core.Buffs;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Weapons;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Events.Args;
using CombatExtended.Framework.TriggersActionsGSQ.Damage;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.UI.Vanilla;
using CombatExtended.Framework.Util;
using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using StardewValley.Triggers;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.Core.Damage;

[HarmonyPatch]
public static class DamageFormulaPatch
{
    
    private static readonly Dictionary<string, Action<Farmer, MeleeWeapon?, Monster, bool, int>> MonsterTakenDamageListeners = new ();

    private static MeleeWeapon? _weapon;
    private static Dictionary<string, int>? _damageTypeRolls;
    private static Dictionary<string, int>? _resultDamageAmounts;
    private static List<DamageModifierData>? _damageModifiers;
    private static int _prevStageResultAmount;

    private const string DamageRollStage = "damageRoll";
    private const string DamageRollMultStage = "damageRollMultiplier";
    private const string AdditiveModifierStage = "additiveModifier";
    private const string MultiplicativeModifierStage = "multiplicativeModifier";

    #region DamageMonsterPatches
    /// <summary>
    /// Prefix to raise PlayerAttacked trigger if damage to monsters does not come from bomb or projectile and player is holding a weapon.
    /// </summary>
    /// <param name="isBomb">Whether damage comes from bomb.</param>
    /// <param name="isProjectile">Whether damage comes from projectile.</param>
    /// <param name="who">Player instance.</param>
    /// <param name="critChance">The critical strike chance.</param>
    /// <param name="critMultiplier">The critical strike multiplier.</param>
    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.damageMonster), typeof(Rectangle), typeof(int), typeof(int),
        typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer), typeof(bool)
    )]
    [HarmonyPrefix]
    private static void GameLocation_DamageMonster_Prefix(bool isBomb, bool isProjectile, Farmer who, 
        ref float critChance, ref float critMultiplier)
    {
        if (!isBomb && !isProjectile && who.CurrentItem is MeleeWeapon)
        {
            critChance += BuffExtensionEnabler.CalculateTotalFlatCritChance();
            critMultiplier += BuffExtensionEnabler.CalculateTotalFlatCritMultiplier();
            TriggerActionManager.Raise(
                trigger: Triggers.PlayerAttacked,
                location: who.currentLocation,
                player: who
            );
        }
    }
    
    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.damageMonster), typeof(Rectangle), typeof(int), typeof(int),
        typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer), typeof(bool)
    )]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction>? GameLocation_DamageMonster_Transpiler(
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        try
        {
            var matcher = new CodeMatcher(instructions);
            var attackHitAnyMonsterTriggerFired = generator.DeclareLocal(typeof(bool));
            matcher.Start()
                // insert var attackHitAnyMonsterTriggerFired = false;
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldc_I4_0),
                    new CodeInstruction(OpCodes.Stloc_S, attackHitAnyMonsterTriggerFired)
                );
            // match beginning of if (!monster.IsInvisible && !monster.isInvincible() && ...)
            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(NPC), nameof(NPC.IsInvisible))),
                new CodeMatch(OpCodes.Brtrue)
            ).ThrowIfNotMatch("Cannot match beginning of whether monster should be damaged.");
            // match beginning of bool isDagger = !isBomb && ...
            matcher.MatchStartForward(
                new CodeMatch(instr => instr.opcode == OpCodes.Ldarg_S && instr.MatchByIndex(4)),
                new CodeMatch(OpCodes.Brtrue_S),
                new CodeMatch(instr => instr.opcode == OpCodes.Ldarg_S && instr.MatchByIndex(10)),
                new CodeMatch(OpCodes.Brtrue_S)
            ).ThrowIfNotMatch("Cannot match beginning of isDagger assignment.");
            var isDaggerLabels = matcher.Instruction.ExtractLabels();
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloca_S, attackHitAnyMonsterTriggerFired).WithLabels(isDaggerLabels),
                new CodeInstruction(OpCodes.Ldarg_S, 10),
                new CodeInstruction(OpCodes.Ldarg_S, 4),
                new CodeInstruction(OpCodes.Ldarg_S, 11),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamageFormulaPatch), nameof(RaiseAttackHitAnyMonsterTrigger)))
            );
            // match damageAmount = Game1.random.Next(minDamage, maxDamage + 1);
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(Game1), nameof(Game1.random))),
                new CodeMatch(OpCodes.Ldarg_2),
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(
                    typeof(Random), nameof(Random.Next), new []{typeof(int), typeof(int)})
                ),
                new CodeMatch(instr => instr.opcode == OpCodes.Stloc_S && instr.MatchByIndex(9))
            ).ThrowIfInvalid("Cannot find instructions for rolling weapon damage.");
            matcher.Advance(1);
            // insert DamageFormulaPatch.StoreDamageRoll(damageAmount)
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_S, 9),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DamageFormulaPatch), nameof(StoreDamageRoll)))
            );
            // insert damageAmount = DamageFormulaPatch.RollExtraDamageTypes(...)
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_S, 4),
                new CodeInstruction(OpCodes.Ldarg_S, 11),
                new CodeInstruction(OpCodes.Ldloc_S, 4),
                new CodeInstruction(OpCodes.Ldloc_S, 9),
                new CodeInstruction(OpCodes.Ldloc_S, 8),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamageFormulaPatch), nameof(RollExtraDamageTypes))),
                new CodeInstruction(OpCodes.Stloc_S, 9)
            );
            // match damageAmount = (crit ? ((int)((float)damageAmount * critMultiplier)) : damageAmount);
            matcher.MatchStartForward(
                new CodeMatch(instr => instr.opcode == OpCodes.Ldloc_S && instr.MatchByIndex(8)),
                new CodeMatch(OpCodes.Brtrue_S),
                new CodeMatch(instr => instr.opcode == OpCodes.Ldloc_S && instr.MatchByIndex(9)),
                new CodeMatch(OpCodes.Br_S),
                new CodeMatch(instr => instr.opcode == OpCodes.Ldloc_S && instr.MatchByIndex(9)),
                new CodeMatch(OpCodes.Conv_R4),
                new CodeMatch(instr => instr.opcode == OpCodes.Ldarg_S && instr.MatchByIndex(8)),
                new CodeMatch(OpCodes.Mul),
                new CodeMatch(OpCodes.Conv_I4),
                new CodeMatch(instr => instr.opcode == OpCodes.Stloc_S && instr.MatchByIndex(9))
            ).ThrowIfNotMatch("Cannot find instructions for critical damage multiplication.");
            var labels = matcher.Instruction.ExtractLabels();
            // insert damageAmount = DamageFormulaPatch.MultiplyDamageRolls(damageAmount);
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_S, 9).WithLabels(labels),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamageFormulaPatch), nameof(MultiplyDamageRolls))),
                new CodeInstruction(OpCodes.Stloc_S, 9)
            );
            
            // match end of damageAmount = Math.Max(1, damageAmount + ((who != null) ? (who.Attack * 3) : 0));
            matcher.MatchStartForward(
                new CodeMatch(instr => instr.opcode == OpCodes.Ldloc_S && instr.MatchByIndex(9)),
                new CodeMatch(instr => instr.opcode == OpCodes.Ldarg_S && instr.MatchByIndex(10)),
                new CodeMatch(OpCodes.Brtrue_S),
                new CodeMatch(OpCodes.Ldc_I4_0)
            ).ThrowIfNotMatch("Cannot find instructions for beginning of attack adding.");
            // insert damageAmount = DamageFormulaPatch.AddAdditiveModifiers(damageAmount);
            labels = matcher.Instruction.ExtractLabels();
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_S, 9).WithLabels(labels),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DamageFormulaPatch), nameof(AddAdditiveModifiers))),
                new CodeInstruction(OpCodes.Stloc_S, 9));
            // match end of damageAmount = Math.Max(1, damageAmount + ((who != null) ? (who.Attack * 3) : 0));
            matcher.MatchEndForward(
                new CodeMatch(instr => instr.opcode == OpCodes.Ldarg_S && instr.MatchByIndex(10)),
                new CodeMatch(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Attack))),
                new CodeMatch(OpCodes.Ldc_I4_3),
                new CodeMatch(OpCodes.Mul),
                new CodeMatch(OpCodes.Add),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Max), new []{typeof(int), typeof(int)})),
                new CodeMatch(instr => instr.opcode == OpCodes.Stloc_S && instr.MatchByIndex(9)))
                .ThrowIfNotMatch("Cannot find end of instructions for attack adding.")
                .Advance(1);
            // insert damageAmount = DamageFormulaPatch.MultiplyDamage(damageAmount);
            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_S, 9),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamageFormulaPatch), nameof(MultiplyDamage))),
                new CodeInstruction(OpCodes.Stloc_S, 9)
            );
            
            var takeDamageMethod = AccessTools.Method(typeof(Monster), nameof(Monster.takeDamage),
                new[] { typeof(int), typeof(int), typeof(int), typeof(bool), typeof(double), typeof(Farmer) });
            // find monster::takeDamage callvirt in
            // monster.takeDamage(damageAmount, (int)trajectory.X, (int)trajectory.Y, isBomb, (double)addedPrecision / 10.0, who);
            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Callvirt, takeDamageMethod)
            ).ThrowIfInvalid("Cannot find Monster::takeDamage call.");
            // before monster.takeDamage(damageAmount, (int)trajectory.X, (int)trajectory.Y, isBomb, (double)addedPrecision / 10.0, who);
            matcher.MatchStartBackwards(
                new CodeMatch(OpCodes.Ldloc_2)
            );
            var takeDamageLabels = matcher.Instruction.ExtractLabels();
            // insert damageAmount = DamageFormulaPatch.AlterFinalDamage(who, weapon, monster, damage)
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_S, 10).WithLabels(takeDamageLabels),
                new CodeInstruction(OpCodes.Ldloc_S, 4),
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Ldloc_S, 9),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DamageFormulaPatch), nameof(AlterFinalDamage))),
                new CodeInstruction(OpCodes.Stloc_S, 9)
            );
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Callvirt, takeDamageMethod)
            );
            // after damageAmount = DamageFormulaPatch.AlterFinalDamage(who, weapon, monster, damageAmount)
            matcher.Advance(1);
            // insert DamageFormulaPatch.AfterMonsterTakenDamage(who, weapon, monster, isCrit, damageAmount)
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_S, 10),
                new CodeInstruction(OpCodes.Ldloc_S, 4),
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Ldloc_S, 8),
                new CodeInstruction(OpCodes.Ldloc_S, 9),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DamageFormulaPatch), nameof(AfterMonsterTakenDamage)))
            );
            
            // find removeDamageDebris(monster);
            matcher.MatchEndForward(
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldloc_2),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(GameLocation), nameof(GameLocation.removeDamageDebris)))
            ).ThrowIfInvalid("Cannot find instructions for removing damage debris.");
            matcher.Advance(1);
            // insert damageAmount = DamageFormulaPatch.DisplayExtraDamageTypeDebris(this, monster, damageAmount);
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, 2),
                new CodeInstruction(OpCodes.Ldloc_S, 9),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamageFormulaPatch), nameof(DisplayExtraDamageTypeDebris))),
                new CodeInstruction(OpCodes.Stloc_S, 9)
            );
            
            var debrisType = AccessTools.Method(typeof(NetCollection<Debris>), nameof(NetCollection<Debris>.Add));
            // end of debris.Add(new Debris(damageAmount, new Vector2(monsterBox.Center.X + 16, monsterBox.Center.Y), crit ? Color.Yellow : new Color(255, 130, 0), crit ? (1f + (float)damageAmount / 300f) : 1f, monster));
            matcher.MatchStartForward(
                new CodeMatch(OpCodes.Callvirt, debrisType)
            ).ThrowIfInvalid("Cannot find end of instructions for creating damage number debris and adding them.");
            matcher.Advance(1);
            
            // insert damageAmount = DamageFormulaPatch.RestoreDamageVariableAfterDebrisDisplay(damageAmount)
            matcher.Insert(
                new CodeInstruction(OpCodes.Ldloc_S, 9),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamageFormulaPatch), nameof(RestoreDamageVariableAfterDebrisDisplay))),
                new CodeInstruction(OpCodes.Stloc_S, 9)
            );
            matcher.FileIL();
            return matcher.InstructionEnumeration();
        } catch(Exception e)
        {
            Log.Error("Failed injecting patch for damaging monsters.", e);
            return instructions;
        }
    }

    /// <summary>
    /// Raise <see cref="Triggers.AttackHitAnyMonster"/> when player attack hit any monster, once for every such attack.
    /// </summary>
    /// <param name="attackHitAnyMonsterTriggerFired">Flag used to track if the trigger was fired already.</param>
    /// <param name="who">The attacking player.</param>
    /// <param name="isBomb">Whether damage comes from the bomb.</param>
    /// <param name="isProjectile">Whether damage comes from the projectile.</param>
    private static void RaiseAttackHitAnyMonsterTrigger(ref bool attackHitAnyMonsterTriggerFired, Farmer who, 
        bool isBomb, bool isProjectile)
    {
        if (attackHitAnyMonsterTriggerFired || isBomb || isProjectile) return;
        attackHitAnyMonsterTriggerFired = true;
        TriggerActionManager.Raise(
            Triggers.AttackHitAnyMonster,
            location: who.currentLocation,
            player: who
        );
    }

    #region DamageRolls
    /// <summary>
    /// Store initial weapon damage roll as physical damage.
    /// </summary>
    /// <param name="damageAmount">Weapon damage roll.</param>
    private static void StoreDamageRoll(int damageAmount)
    {
        _damageTypeRolls = new Dictionary<string, int> { [DamageType.Physical] = damageAmount };
    }

    /// <summary>
    /// Roll additional damage types for <paramref name="weapon"/>.
    /// </summary>
    /// <param name="isBomb">Whether damage comes from bomb.</param>
    /// <param name="isProjectile">Whether damage comes from projectile.</param>
    /// <param name="weapon">Player's weapon instance.</param>
    /// <param name="damageAmount">Weapon damage roll.</param>
    /// <param name="isCrit">Whether damage is critical.</param>
    /// <param name="loc">Location of monster to be damaged.</param>
    /// <param name="monster">Monster to be damaged.</param>
    /// <returns>Result damage amount.</returns>
    private static int RollExtraDamageTypes(bool isBomb, bool isProjectile, MeleeWeapon? weapon, int damageAmount, 
        bool isCrit, GameLocation loc, Monster monster)
    {
        // do not handle non weapon damage
        if (isBomb || isProjectile || weapon is null)
        {
            _damageTypeRolls = null;
            return damageAmount;
        }
        _weapon = weapon;
        var extraDamageTypes = WeaponStateEnabler.GetExtraDamageTypesWithState(weapon.ItemId);
        var totalDamage = damageAmount;
        foreach (var type in extraDamageTypes)
        {
            var damage = Game1.random.Next(type.MinDamage, type.MaxDamage + 1);
            totalDamage += damage;
            if (_damageTypeRolls!.ContainsKey(type.Type))
                _damageTypeRolls[type.Type] += damage;
            else _damageTypeRolls[type.Type] = damage;
        }
        // set _damageModifiers here to not check conditions in subsequent stages
        _damageModifiers = new List<DamageModifierData>();
        var item = ItemRegistry.Create("(O)168");
        item.PackMonsterInfo(monster);
        item.modData[DamageGameStateQueries.IsCritItemKey] = isCrit.ToString();
        foreach (var (_, modifiers) in DamageModifiersDataAsset.Data)
        {
            if (GameStateQuery.CheckConditions(modifiers.Condition, targetItem: item, location: loc))
                _damageModifiers.AddRange(modifiers.Modifiers);
        }
        // group modifiers by their group
        var grouped = _damageModifiers
            .GroupBy(m => m.Group)
            .SelectMany(g => g)
            .ToList();
        _damageModifiers = grouped;
        _prevStageResultAmount = totalDamage;
        totalDamage = AddDamageForStage(DamageRollStage, totalDamage);
        _prevStageResultAmount = totalDamage;
        return totalDamage;
    }

    private static int MultiplyDamageRolls(int damageAmount)
    {
        if (_damageModifiers is null || _damageTypeRolls is null) return damageAmount;
        var result = MultiplyDamageForStage(DamageRollMultStage, damageAmount);
        return result;
    }

    private static int MultiplyDamage(int damageAmount)
    {
        if (_damageModifiers is null || _damageTypeRolls is null) return damageAmount;
        var result = MultiplyDamageForStage(MultiplicativeModifierStage, damageAmount);
        return result;
    }

    private static int AddAdditiveModifiers(int damageAmount)
    {
        if (_damageModifiers is null || _damageTypeRolls is null) return damageAmount;
        var result = AddDamageForStage(AdditiveModifierStage, damageAmount);
        _prevStageResultAmount = result;
        return result;
    }
    
    #endregion

    #region DamageDebrisDisplay
    /// <summary>
    /// Once damage debris are handled for all types of damage, restore back damage amount variable.
    /// </summary>
    /// <param name="damageAmount"></param>
    /// <returns>Restored total damage amount.</returns>
    private static int RestoreDamageVariableAfterDebrisDisplay(int damageAmount)
    {
        if (_resultDamageAmounts is null) return damageAmount;
        var result = _resultDamageAmounts.Values.Sum();
        _resultDamageAmounts = null;
        _damageTypeRolls = null;
        _weapon = null;
        _damageModifiers = null;
        _prevStageResultAmount = 0;
        return result;
    }

    private static int DisplayExtraDamageTypeDebris(GameLocation location, Monster monster, int damageAmount)
    {
        if (_weapon is null || _resultDamageAmounts is null) return damageAmount;
        
        var offSetX = 32;
        var offSetY = 16;
        foreach (var (type, damage) in _resultDamageAmounts)
        {
            if (type == DamageType.Physical) continue;
            var damageTypeColor = DamageTypeDataAsset.DamageTypeColors[type];
            DamageNumbersDebris.Display(location, monster, damage, damageTypeColor, offSetX, offSetY);
            offSetX += 24;
            offSetY += 24;
        }
        return _resultDamageAmounts[DamageType.Physical];
    }
    #endregion

    private static int MultiplyDamageForStage(string stage, int damage)
    {
        if (_damageModifiers is null || _damageTypeRolls is null) return damage;
        if (damage != _prevStageResultAmount)
        {
            // assume something was added to damage, adjust damage numbers accordingly
            var diff = damage - _prevStageResultAmount;
            _damageTypeRolls[DamageType.Physical] += diff;
        }
        var prevGroup = _damageModifiers.FirstOrDefault()?.Group;
        if (prevGroup is null) 
            return damage;
        var damageTypeGroupMults = new Dictionary<string, float>();
        foreach (var damageModifierData in _damageModifiers)
        {
            if (damageModifierData.Stage != stage) continue;
            if (damageModifierData.DamageType is not "_all" &&
                !DamageTypeDataAsset.Exists(damageModifierData.DamageType))
            {
                Log.Error($"Unknown damage type: {damageModifierData.DamageType}.");
                continue;
            }

            var maxValue = damageModifierData.MaxValue ?? damageModifierData.MinValue;
            // to be added with 1 for a proper group modifier value
            var additiveMult = Utility.InclusiveFloatSemiRandom(damageModifierData.MinValue, maxValue, Game1.random) - 1;
            // group ended, apply group modifier
            if (damageModifierData.Group != prevGroup)
            {
                foreach (var (damageType, additiveGroupMult) in damageTypeGroupMults)
                {
                    _damageTypeRolls[damageType] = (int)(_damageTypeRolls[damageType] * (1 + additiveGroupMult));
                }
                damageTypeGroupMults.Clear();
                prevGroup = damageModifierData.Group;
            }
            
            if (damageModifierData.DamageType != "_all")
            {
                if (!damageTypeGroupMults.TryAdd(damageModifierData.DamageType, additiveMult))
                    damageTypeGroupMults[damageModifierData.DamageType] += additiveMult;
            }
            else
            {
                foreach (var (damageTypeId, _) in _damageTypeRolls)
                {
                    if (!damageTypeGroupMults.TryAdd(damageTypeId, additiveMult)) 
                        damageTypeGroupMults[damageTypeId] += additiveMult;
                }
            }
        }
        
        // handle last group
        foreach (var (damageType, additiveGroupMult) in damageTypeGroupMults)
        {
            _damageTypeRolls[damageType] = (int)(_damageTypeRolls[damageType] * Math.Max(0, 1 + additiveGroupMult));
        }

        var result = _damageTypeRolls.Values.Sum();
        _prevStageResultAmount = result;
        return result;
    }

    private static int AddDamageForStage(string stage, int damage)
    {
        if (_damageModifiers is null || _damageTypeRolls is null) return damage;
        var result = damage;
        if (damage != _prevStageResultAmount)
        {
            // well that's a possibility
            if (_prevStageResultAmount == 0) 
                _damageTypeRolls[DamageType.Physical] = damage;
            else
            {
                // assume something multiplied the damage, adjust damage numbers accordingly
                var mult = (float)damage / _prevStageResultAmount;
                foreach (var (type, roll) in _damageTypeRolls)
                {
                    _damageTypeRolls[type] = (int)(roll * mult);
                }
            }
        }
        foreach (var damageModifierData in _damageModifiers)
        {
            if (damageModifierData.Stage != stage || damageModifierData.DamageType is "_all") continue;
            if (!DamageTypeDataAsset.Exists(damageModifierData.DamageType))
            {
                Log.Error($"Unknown damage type: {damageModifierData.DamageType}.");
                continue;
            }
            var minDamage = (int)damageModifierData.MinValue;
            var maxDamage = (int)(damageModifierData.MaxValue ?? minDamage);
            var randDamage = Game1.random.Next(minDamage, maxDamage + 1);
            result += randDamage;
            if (!_damageTypeRolls.TryAdd(damageModifierData.DamageType, randDamage)) 
                _damageTypeRolls[damageModifierData.DamageType] += randDamage;
        }
        _prevStageResultAmount = result;
        return result;
    }

    /// <summary>
    /// Get resulting damage for each <see cref="DamageType"/> with resistances applied.
    /// </summary>
    /// <param name="weapon">Weapon with extra damage types.</param>
    /// <param name="monster">Monster instance.</param>
    /// <param name="damageAmount">Result damage to monster.</param>
    /// <returns>Dictionary with damage type id keys and corresponding approximate damage</returns>
    private static Dictionary<string, int> GetDamageWithResistancesApplied(MeleeWeapon weapon, Monster monster, int damageAmount)
    {
        var result = new Dictionary<string, int>();
        var total = _damageTypeRolls!.Values.Sum();
        foreach (var (type, damage) in _damageTypeRolls)
        {
            var resistance = monster.GetResistances();
            // assume before there were only multipliers
            result[type] = 
                total == 0 ? 0 :
                (int)((float)damageAmount * damage / total * resistance.GetMult(type));
        }
        return result;
    }

    private static int AlterFinalDamage(Farmer farmer, MeleeWeapon? weapon, Monster monster, int damage)
    {
        if (weapon is null || _weapon is null || _damageTypeRolls is null || _damageModifiers is null) 
            return damage;
        var resultDamages = GetDamageWithResistancesApplied(weapon, monster, damage);
        _resultDamageAmounts = resultDamages;
        return resultDamages.Values.Sum();
    }

    private static void AfterMonsterTakenDamage(Farmer farmer, MeleeWeapon? weapon, Monster monster, bool isCrit, int damage)
    {
        TriggerMonsterAttackedEffects(farmer, weapon, monster, isCrit, damage);
        Event.RaiseMonsterTakenDamage(new MonsterTakenDamageEventArgs(farmer, weapon, monster, damage, isCrit));
    }

    private static void TriggerMonsterAttackedEffects(Farmer farmer, MeleeWeapon? weapon, Monster monster, bool isCrit,
        int damage)
    {
        if (weapon is null) return;
        var item = ItemRegistry.Create("(O)168");
        item.PackMonsterInfo(monster);
        item.modData[DamageGameStateQueries.DamageItemKey] = damage.ToString();
        item.modData[DamageGameStateQueries.IsCritItemKey] = isCrit.ToString();
        object[] triggerArgs = { monster };
        var trigger = damage >= 0 ? Triggers.AttackHitMonster : Triggers.AttackMissedMonster;
        TriggerActionManager.Raise(
            trigger: trigger,
            triggerArgs: triggerArgs,
            location: farmer.currentLocation,
            player: farmer,
            targetItem: item
        );
        Event.RaiseMonsterAttacked(new MonsterAttackedEventArgs(farmer, weapon, monster, damage, isCrit));
    }
    #endregion
}
