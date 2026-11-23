using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Misc.MonsterDisplacement;

public class MonsterDisplacementInfo
{
    public Vector2 Velocity { get; init; }
    
    public float Distance { get; init; }

    public int TimeBeforeDistanceCheck { get; set; }
    
    public Vector2 TotalDisplacementDistance { get; set; }

    public Action OnEnd { get; init; } = () => { };
}

[HarmonyPatch]
public static class MonsterDisplacementEnabler
{
    private static readonly ConditionalWeakTable<Monster, MonsterDisplacementInfo> Displacements = new();

    public static void StartDisplacement(Monster monster, Vector2 velocity, float distance, Action? onEnd = null)
    {
        var pullInfo = new MonsterDisplacementInfo
        {
            Velocity = velocity,
            Distance = distance,
            TotalDisplacementDistance = Vector2.Zero,
            OnEnd = () =>
            {
                Displacements.Remove(monster);
                onEnd?.Invoke();
            }
        };
        Displacements.Add(monster, pullInfo);
    }

    [HarmonyPatch(typeof(Monster), nameof(Monster.update))]
    [HarmonyPostfix]
    private static void Monster_update_Postfix(Monster __instance, GameTime time)
    {
        if (!Displacements.TryGetValue(__instance, out var info)) return;
        
        var boundingBox = __instance.GetBoundingBox();
        var value = new Rectangle(
            boundingBox.X - 2 * (int)Math.Floor(info.Velocity.X), 
            boundingBox.Y - 2 * (int)Math.Floor(info.Velocity.Y), 
            boundingBox.Width + 8, boundingBox.Height + 8);
        var value2 = new Rectangle(
            boundingBox.X + 2 * (int)Math.Ceiling(info.Velocity.X), 
            boundingBox.Y + 2 * (int)Math.Ceiling(info.Velocity.Y), 
            boundingBox.Width + 8, boundingBox.Height + 8);
        var rectangle = Rectangle.Union(value, value2);
        if (__instance.currentLocation.isCollidingPosition(rectangle, Game1.viewport, false, -1,
                __instance.isGlider.Value, __instance))
        {
            info.OnEnd.Invoke();
            return;
        }
        __instance.position.X += info.Velocity.X;
        __instance.position.Y += info.Velocity.Y;
        info.TotalDisplacementDistance += info.Velocity;
        if (info.TimeBeforeDistanceCheck > 0)
        {
            info.TimeBeforeDistanceCheck -= time.ElapsedGameTime.Milliseconds;
        }
        else
        {
            var distance = info.TotalDisplacementDistance.Length();
            if (distance >= info.Distance)
                info.OnEnd.Invoke();
            info.TimeBeforeDistanceCheck = 10;
        }
    }

    [HarmonyPatch(typeof(Monster), nameof(Monster.updateMovement))]
    [HarmonyPrefix]
    private static bool Monster_updateMovement_Prefix(Monster __instance)
    {
        return !Displacements.TryGetValue(__instance, out _);
    }
    
    [HarmonyPatch(typeof(Monster), nameof(Monster.MovePosition))]
    [HarmonyPrefix]
    private static bool Monster_MovePosition_Prefix(Monster __instance)
    {
        return !Displacements.TryGetValue(__instance, out _);
    }
}
