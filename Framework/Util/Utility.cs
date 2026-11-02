using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using CombatExtended.Framework.Core.Buffs;
using CombatExtended.Framework.Core.Monsters;
using CombatExtended.Framework.Core.Monsters.Buffs;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.TriggersActionsGSQ.Util;
using CombatExtended.Framework.UI.Vanilla;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using StardewValley.Triggers;

namespace CombatExtended.Framework.Util;

public static class Utility
{
    /// <summary>
    /// Damage <paramref name="monster"/> directly and display <paramref name="damage"/> with custom <paramref name="damageColor"/>.
    /// </summary>
    /// <param name="location">Location with <paramref name="monster"/> to be damaged.</param>
    /// <param name="monster">The monster to be damaged.</param>
    /// <param name="damage">The damage to <paramref name="monster"/>.</param>
    /// <param name="player">The current player.</param>
    /// <param name="damageColor">The display colour of damage.</param>
    public static void DamageMonsterDirectly(
        this GameLocation location,
        Monster monster,
        int damage,
        Farmer player,
        Color damageColor
    )
    {
        var actual = monster.takeDamage(damage, 0, 0, false, 0.0, player);
        DamageNumbersDebris.Display(location, monster, actual, damageColor);
    }
    
    /// <summary>
    /// Find the closest monster to given <paramref name="tileFrom"/> in given <paramref name="radius"/> in tiles, ignoring <paramref name="from"/> if provided.
    /// </summary>
    /// <param name="loc">The monster location.</param>
    /// <param name="tileFrom">The tile to search relative to.</param>
    /// <param name="radius">The radius of search.</param>
    /// <param name="closest">The closest monster, if any.</param>
    /// <param name="from">The monster from whom search should be initiated.</param>
    /// <returns>Whether any <paramref name="closest"/> was found in <paramref name="radius"/>.</returns>
    public static bool TryFindClosestMonster(GameLocation loc, Vector2 tileFrom, float radius, 
        [MaybeNullWhen(false)] out Monster closest, Monster? from = null)
    {
        var monsters = loc.characters.OfType<Monster>();
        closest = null;
        var distance = float.MaxValue;
        foreach (var monster in monsters)
        {
            if (monster == from) continue;
            var monsterTile = monster.Tile;
            var distanceSquared = (monsterTile.X - tileFrom.X) * (monsterTile.X - tileFrom.X) + 
                                         (monsterTile.Y - tileFrom.Y) * (monsterTile.Y - tileFrom.Y);
            if (distanceSquared < distance && distanceSquared < radius * radius)
            {
                closest = monster;
                distance = distanceSquared;
            }
        }

        return closest is not null;
    }

    /// <summary>
    /// Convert given <paramref name="sec"/> seconds to milliseconds.
    /// </summary>
    /// <param name="sec">Seconds amount.</param>
    /// <returns>Milliseconds amount.</returns>
    public static int ToMilliseconds(this float sec)
    {
        return (int) (sec * 1000);
    }
    
    /// <summary>
    /// Convert given <paramref name="sec"/> seconds to milliseconds.
    /// </summary>
    /// <param name="sec">Seconds amount.</param>
    /// <returns>Milliseconds amount.</returns>
    public static int ToMilliseconds(this int sec)
    {
        return sec * 1000;
    }

    public static int ToTicks(this int sec)
    {
        return sec * 60;
    }
    
    /// <summary>
    /// Convert given <paramref name="sec"/> seconds to ticks equivalent.
    /// </summary>
    /// <param name="sec">Seconds amount.</param>
    /// <returns>Ticks amount.</returns>
    public static int ToTicks(this float sec)
    {
        return (int) (sec * 60);
    }

    public static int MillisToTicks(this int millis)
    {
        return (millis / 1000f).ToTicks();
    }

    /// <summary>
    /// Check if any button combination in <paramref name="keybinds"/> equals given <paramref name="buttons"/>, ignoring order of pressed buttons.
    /// </summary>
    /// <param name="keybinds">The keybinds.</param>
    /// <param name="buttons">The pressed buttons.</param>
    /// <returns>Whether <paramref name="buttons"/> are included in <paramref name="keybinds"/>.</returns>
    public static bool AnyEquals(this KeybindList keybinds, List<SButton> buttons)
    {
        foreach (var keybind in keybinds.Keybinds)
        {
            if (keybind.Buttons.ToHashSet().SequenceEqual(buttons))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Execute <paramref name="action"/>, catching <typeparam name="T"></typeparam> exceptions and printing exception message into <paramref name="log"/>, using WARN level.
    /// </summary>
    /// <param name="log"><see cref="ModLog"/> instance.</param>
    /// <param name="action">Action to execute.</param>
    /// <typeparam name="T">Exception type that should be caught.</typeparam>
    public static void TryWithWarning<T>(ModLog log, Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T e)
        {
            log.Warn(e.Message);
        }
    }
    
    /// <summary>
    /// Execute <paramref name="action"/>, catching <typeparam name="T"></typeparam> exceptions and printing <paramref name="warning"/> into <paramref name="log"/>, using WARN level.
    /// </summary>
    /// <param name="log"><see cref="ModLog"/> instance.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="warning">Warning to be logged.</param>
    /// <typeparam name="T">Exception type that should be caught.</typeparam>
    public static void TryWithWarning<T>(ModLog log, Action action, string warning) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            log.Warn(warning);
        }
    }
    
    public static void RaiseMonsterBuffTrigger(string trigger, string buffId, Monster monster, GameLocation loc)
    {
        var item = ItemRegistry.Create("(O)168");
        var buff = MonsterBuff.Create(buffId);
        item.PackMonsterInfo(monster);
        item.PackMonsterBuffInfo(buff);
        TriggerActionManager.Raise(
            trigger: trigger,
            triggerArgs: new object[] { monster },
            location: loc,
            player: Game1.player,
            targetItem: item
        );
    } 
    
    /// <summary>
    /// Try to find monster with given <paramref name="monsterId"/> on <paramref name="loc"/>.
    /// </summary>
    /// <param name="monsterId">The monster to match with ones from modData.</param>
    /// <param name="loc">The location where monster should be searched.</param>
    /// <param name="monster">The found monster, if any.</param>
    /// <returns>Whether monster with given id was found.</returns>
    public static bool TryFindMonster(string monsterId, string loc, [MaybeNullWhen(false)] out Monster monster)
    {
        var location = Game1.getLocationFromName(loc);
        var result = location.characters.OfType<Monster>().FirstOrDefault(m => 
            m.modData.TryGetValue("Id".PrependId(), out var id) && id == monsterId);
        if (result is not null)
        {
            monster = result;
            return true;
        }

        monster = null;
        return false;
    }

    public static string GetCharacterId(this Character character)
    {
        return character switch
        {
            Monster monster => monster.modData.GetValueOrDefault("Id".PrependId()) ??
                                          $"{Game1.player.UniqueMultiplayerID}_{MonsterExtensionEnabler.GetNextMonsterId()}",
            Farmer farmer => farmer.UniqueMultiplayerID.ToString(),
            _ => throw new ArgumentException($"Character has unsupported type {character.GetType()}")
        };
    }
    
    public static string GetCharacterType(this Character character)
    {
        return character switch
        {
            Monster => "monster",
            Farmer => "player",
            _ => throw new ArgumentException($"Character has unsupported type {character.GetType()}")
        };
    }

    public static bool MatchByIndex(this CodeInstruction codeInstruction, int index)
    {
        if (codeInstruction.operand is int idx) return idx == index;
        if (codeInstruction.operand is byte bIdx) return bIdx == (byte) index;
        if (codeInstruction.operand is LocalBuilder lb) return lb.LocalIndex == index;
        throw new ArgumentException($"Operand {codeInstruction.operand.GetType()} is not supported");
    }

    /// <summary>
    /// Get player's current weapon, if any
    /// </summary>
    /// <returns>Player's weapon if any, <c>null</c> otherwise.</returns>
    public static MeleeWeapon? CurrentWeapon()
    {
        return Game1.player.ActiveItem is MeleeWeapon weapon ? weapon : null;
    }

    /// <summary>
    /// Prepend mod id and <c>_</c> delimiter to given <paramref name="str"/>.
    /// </summary>
    /// <param name="str">String to prepend id to.</param>
    /// <returns><paramref name="str"/> with prepended mod id.</returns>
    public static string PrependId(this string str) => str.Prepend($"{Id}_");
    
    /// <summary>
    /// Prepend given <paramref name="prefix"/> to given <paramref name="str"/>.
    /// </summary>
    /// <param name="str">String to prepend prefix to.</param>
    /// <param name="prefix">Prefix to prepend.</param>
    /// <returns><paramref name="str"/> with prepended prefix.</returns>
    public static string Prepend(this string str, string prefix) => $"{prefix}{str}";

    #region Buffs
    public static void ApplyBuff(this Monster monster, GameLocation location, string buffId, string? applier,
        string? applierId)
    {
        MonsterBuffManager.ApplyBuff(monster, location, buffId, applier, applierId);
    }

    public static void RegisterAddedStack(this Buff buff)
    {
        StackableBuffManager.RegisterAddedStack(buff.id);
    }
    
    public static int GetStacks(this Buff buff)
    {
        return StackableBuffManager.GetStacks(buff.id);
    }

    public static void OnStackableBuffExpire(this Buff buff)
    {
        StackableBuffManager.RegisterBuffExpiration(buff.id);
    }

    public static void AddExtraBuffEffects(this Buff buff, ExtraBuffAttributesData data)
    {
        BuffExtensionEnabler.AddArmourClass(buff, (int) data.ArmourClass);
    }
    #endregion
}
