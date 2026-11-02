using System.Reflection.Emit;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Weapons;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Damage;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.UI.Vanilla;
using CombatExtended.Framework.Util;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Core.Damage;

[HarmonyPatch]
public static class DamageFormulaPatch
{
    
    private static readonly Dictionary<string, Func<Farmer, MeleeWeapon?, Monster, int, int>> FinalDamageAlteringFuncs = new ();
    
    private static readonly Dictionary<string, Action<Farmer, MeleeWeapon?, Monster, bool, int>> MonsterTakenDamageListeners = new ();

    private static MeleeWeapon? _weapon;
    private static Dictionary<string, int>? _damageTypeRolls;
    private static Dictionary<string, int>? _resultDamageAmounts;

    /// <summary>
    /// Add function to alter damage to monster before it receives it.
    /// </summary>
    /// <remarks>Function arguments are: farmer, their weapon if any, monster that have taken damage, damage to monster.</remarks>
    /// <remarks>Function result is damage that monster should receive.</remarks>
    /// <param name="name">Unique name for the function to use in logging.</param>
    /// <param name="func">Function to alter damage.</param>
    public static void AddFinalDamageAlteringFunction(string name, Func<Farmer, MeleeWeapon?, Monster, int, int> func)
    {
        FinalDamageAlteringFuncs[name] = func;
    }

    /// <summary>
    /// Add listener to run arbitrary logic after monster have taken damage.
    /// </summary>
    /// <remarks>Listener arguments are: farmer, their weapon if any, monster that have taken damage, whether damage was crit and damage to monster.</remarks>
    /// <param name="name">Unique name for the listener to use in logging.</param>
    /// <param name="listener">Listener action.</param>
    public static void AddMonsterTakenDamageListener(string name, Action<Farmer, MeleeWeapon?, Monster, bool, int> listener)
    {
        MonsterTakenDamageListeners[name] = listener;
    }

    #region DamageMonsterPatches
    /// <summary>
    /// Prefix to raise PlayerAttacked trigger if damage to monsters does not come from bomb or projectile and player is holding a weapon.
    /// </summary>
    /// <param name="isBomb">Whether damage comes from bomb.</param>
    /// <param name="isProjectile">Whether damage comes from projectile.</param>
    /// <param name="who">Player instance.</param>
    [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.damageMonster), typeof(Rectangle), typeof(int), typeof(int),
        typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer), typeof(bool)
    )]
    [HarmonyPrefix]
    private static void GameLocation_DamageMonster_Prefix(bool isBomb, bool isProjectile, Farmer who)
    {
        if (!isBomb && !isProjectile && who.CurrentItem is MeleeWeapon)
        {
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
            //find damageAmount = Game1.random.Next(minDamage, maxDamage + 1);
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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DamageFormulaPatch), nameof(RollExtraDamageTypes))),
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
            // insert damageAmount = DamageFormulaPatch.AlterFinalDamage(Farmer farmer, MeleeWeapon? weapon, Monster monster, int damage)
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
            // after damageAmount = DamageFormulaPatch.AlterFinalDamage(Farmer farmer, MeleeWeapon? weapon, Monster monster, int damage)
            matcher.Advance(1);
            // insert AfterMonsterTakenDamage listener after damage
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
            foreach (var codeInstruction in matcher.Instructions())
            {
                File.AppendAllText("/home/dmitry/desktop/yt.txt", codeInstruction + "\n");
            }
            
            return matcher.InstructionEnumeration();
        } catch(Exception e)
        {
            Log.Error("Failed injecting patch for damaging monsters.", e);
            return instructions;
        }
    }

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
    /// <returns>Result damage amount.</returns>
    private static int RollExtraDamageTypes(bool isBomb, bool isProjectile, MeleeWeapon? weapon, int damageAmount)
    {
        // do not handle non weapon damage
        if (isBomb || isProjectile || weapon is null)
        {
            _damageTypeRolls = null;
            return damageAmount;
        }
        _weapon = weapon;
        var extraDamageTypes = WeaponStateEnabler.GetExtraDamageTypesWithState(weapon.ItemId);
        if (!extraDamageTypes.Any()) return damageAmount;
        var totalDamage = damageAmount;
        foreach (var type in extraDamageTypes)
        {
            var damage = Game1.random.Next(type.MinDamage, type.MaxDamage + 1);
            totalDamage += damage;
            _damageTypeRolls![type.Type] = damage;
        }
        return totalDamage;
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
            offSetX += 16;
            offSetY += 16;
        }
        return _resultDamageAmounts[DamageType.Physical];
    }
    #endregion

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
            result[type] = (int)((float)damageAmount * damage / total * resistance.GetMult(type));
        }
        return result;
    }

    private static int AlterFinalDamage(Farmer farmer, MeleeWeapon? weapon, Monster monster, int damage)
    {
        var result = damage;
        if (weapon is not null && _weapon is not null && _resultDamageAmounts is not null)
        {
            var resultDamages = GetDamageWithResistancesApplied(weapon, monster, damage);
            _resultDamageAmounts = resultDamages;
            result = resultDamages.Values.Sum();
        }

        foreach (var (name, func) in FinalDamageAlteringFuncs)
        {
            try
            {
                result = func(farmer, weapon, monster, result);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to execute final damage altering function {name}", e);
            }
        }
        return result;
    }

    private static void AfterMonsterTakenDamage(Farmer farmer, MeleeWeapon? weapon, Monster monster, bool isCrit, int damage)
    {
        TriggerMonsterAttackedEffects(farmer, weapon, monster, isCrit, damage);
        foreach (var (name, listener) in MonsterTakenDamageListeners)
        {
            try
            {
                listener(farmer, weapon, monster, isCrit, damage);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to execute monster taken damage listener {name}", e);
            }
        }
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
    }
    #endregion
}
