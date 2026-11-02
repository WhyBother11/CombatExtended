using CombatExtended.Framework.Core.Abilities;
using CombatExtended.Framework.Core.Misc.ActionPoints;
using CombatExtended.Framework.Data;
using StardewValley;
using StardewValley.Tools;
using Utility = CombatExtended.Framework.Util.Utility;

namespace CombatExtended.Framework.Core.Weapons;

public static class Extensions
{
    
    public static List<Ability> GetAbilities(this MeleeWeapon weapon)
    {
        var result = new List<Ability>();
        if (WeaponExtensionDataAsset.Abilities.TryGetValue(weapon.ItemId, out var abilitiesStr))
        {
            var abilities = ArgUtility.SplitBySpace(abilitiesStr);
            foreach (var ability in abilities)
            {
                Utility.TryWithWarning<ArgumentException>(Log, () => result.Add(Ability.Create(ability)));
            }
        }

        return result;
    }

    public static Dictionary<string, Ability> GetAbilityDictionary(this MeleeWeapon weapon)
    {
        var result = new Dictionary<string, Ability>();
        if (WeaponExtensionDataAsset.Abilities.TryGetValue(weapon.ItemId, out var abilitiesStr))
        {
            var abilities = ArgUtility.SplitBySpace(abilitiesStr);
            foreach (var ability in abilities)
            {
                Utility.TryWithWarning<ArgumentException>(Log, () => result.Add(ability, Ability.Create(ability)));
            }
        }

        return result;
    }
    
    public static List<ActionPoint> GetActionPoints(this MeleeWeapon weapon)
    {
        var result = new List<ActionPoint>();
        if (WeaponExtensionDataAsset.ActionPoints.TryGetValue(weapon.ItemId, out var actionPointsStr))
        {
            var actions = ArgUtility.SplitBySpace(actionPointsStr);
            foreach (var action in actions)
            {
                Utility.TryWithWarning<ArgumentException>(Log, () => result.Add(ActionPoint.Create(action)));
            }
        }
        return result;
    }
    
    public static Dictionary<string, ActionPoint> GetActionPointDictionary(this MeleeWeapon weapon)
    {
        var result = new Dictionary<string, ActionPoint>();
        if (WeaponExtensionDataAsset.ActionPoints.TryGetValue(weapon.ItemId, out var actionPointsStr))
        {
            var actions = ArgUtility.SplitBySpace(actionPointsStr);
            foreach (var action in actions)
            {
                Utility.TryWithWarning<ArgumentException>(Log, () => result.Add(action, ActionPoint.Create(action)));
            }
        }

        return result;
    }
}
