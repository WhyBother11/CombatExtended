using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using HarmonyLib;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Buffs;

namespace CombatExtended.Framework.Core.Buffs;

[HarmonyPatch]
public static class StackableBuffs
{
    
    [HarmonyPatch(typeof(BuffManager), nameof(BuffManager.Apply))]
    [HarmonyPostfix]
    private static void BuffManager_Apply_Postfix(Buff buff)
    {
        if (!BuffExtensionDataAsset.Data.TryGetValue(buff.id, out var data) || data.StackableData is null) return;
        buff.RegisterAddedStack();
        if (data.StackableData.AttributesPerStack is null) return;
        var effectiveStacks = buff.GetStacks() - 1;
        if (effectiveStacks <= 0) return;
        var attributes = data.StackableData.AttributesPerStack;
        if (attributes is not null)
        {
            var resultAttributes = new BuffAttributesData
            {
                Attack = attributes.Attack * effectiveStacks,
                AttackMultiplier = attributes.AttackMultiplier * effectiveStacks,
                CombatLevel = attributes.CombatLevel * effectiveStacks,
                CriticalChanceMultiplier = attributes.CriticalChanceMultiplier * effectiveStacks,
                CriticalPowerMultiplier = attributes.CriticalPowerMultiplier * effectiveStacks,
                Defense = attributes.Defense * effectiveStacks,
                FarmingLevel = attributes.FarmingLevel * effectiveStacks,
                FishingLevel = attributes.FishingLevel * effectiveStacks,
                ForagingLevel = attributes.ForagingLevel * effectiveStacks,
                Immunity = attributes.Immunity * effectiveStacks,
                KnockbackMultiplier = attributes.KnockbackMultiplier * effectiveStacks,
                LuckLevel = attributes.LuckLevel * effectiveStacks,
                MagneticRadius = attributes.MagneticRadius * effectiveStacks,
                MaxStamina = attributes.MaxStamina * effectiveStacks,
                MiningLevel = attributes.MiningLevel * effectiveStacks,
                Speed = attributes.Speed * effectiveStacks,
                WeaponPrecisionMultiplier = attributes.WeaponPrecisionMultiplier * effectiveStacks,
                WeaponSpeedMultiplier = attributes.WeaponSpeedMultiplier * effectiveStacks
            };
            buff.effects.Add(resultAttributes);
        }

        var extraAttributes = data.StackableData.ExtraAttributesPerStack;
        if (extraAttributes is not null)
        {
            var resultExtraAttributes = new ExtraBuffAttributesData
            {
                ArmourClass = extraAttributes.ArmourClass * effectiveStacks,
            };
            buff.AddExtraBuffEffects(resultExtraAttributes);
        }
    }

    [HarmonyPatch(typeof(Buff), nameof(Buff.update))]
    [HarmonyPostfix]
    private static void Buff_update_ClearPostfix(Buff __instance, bool __result)
    {
        if (!__result || !BuffExtensionDataAsset.Data.TryGetValue(__instance.id, out _)) return;
        __instance.OnStackableBuffExpire();
    }
}
