using CombatExtended.Framework.Core.Util;
using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Monsters;

namespace CombatExtended.Framework.TriggersActionsGSQ.Misc;

[TriggerActionGSQ_Holder]
public static class EffectTriggerActions
{

    [TriggerAction]
    private static bool LightningStrike(string[] args, TriggerActionContext ctx, out string error)
    {
        if (ArgUtility.TryGet(args, 1, out var rangeId, out error))
        {
            if (!RangeDataAsset.Exists(rangeId))
            {
                error = $"Invalid range id: {rangeId}";
                return false;
            }
            var entity = "all";
            if (ArgUtility.TryGet(args, 2, out var target, out error))
                entity = target;
            var player = Game1.player;
            RangeUtil.GetAffectedByRange(rangeId, player, null, out var affectedFarmers, out var affectedMonsters);
            var characters = new List<Character>();
            switch (entity)
            {
                case "all":
                    characters.AddRange(affectedMonsters);
                    characters.AddRange(affectedMonsters);
                    break;
                case "player":
                    characters.AddRange(affectedFarmers); 
                    break;
                case "monster":
                    characters.AddRange(affectedMonsters); 
                    break;
            }
            characters.ForEach(c => SUtility.drawLightningBolt(new Vector2(
                c.Position.X, c.Position.Y - c.Sprite.SpriteHeight / 2f), c.currentLocation));
            return true;
        }
        if (ctx.TriggerArgs.Length == 1 && ctx.TriggerArgs[0] is Monster monster)
        {
            SUtility.drawLightningBolt(new Vector2(monster.Position.X, monster.Position.Y - monster.Sprite.SpriteHeight / 2f), 
                monster.currentLocation);
        }

        error = "No range was used in a context that does not have a single monster.";
        return false;
    }
}
