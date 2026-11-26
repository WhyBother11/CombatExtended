using CombatExtended.Framework.Data;
using CombatExtended.Framework.Util;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;

namespace CombatExtended.Framework.Core.Misc.LinkEffect.Impl;

public class MonsterLinkEffect : LinkEffect
{
    
    private Monster Monster { get; set; }
    
    public int DoMonsterEffectEveryTick { get; set; }
    
    public Vector2? Offset { get; set; }
    
    public MonsterLinkEffect(Monster monster, string id, LinkEffectData data) : base(id, data)
    {
        Monster = monster;
        if (data.TriggerEffectData is not null)
        {
            DoMonsterEffectEveryTick = data.TriggerEffectData.DoMonsterEffectEverySec.ToTicks();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var linkPosition = GetLinkPosition();
        Draw(spriteBatch, linkPosition);
    }

    public override IEnumerable<Rectangle> GetHitBox()
    {
        return GetHitBox(GetLinkPosition());
    }

    private Vector2 GetLinkPosition()
    {
        var monsterPosition = Game1.GlobalToLocal(Game1.viewport, Monster.Position);
        var linkPosition = monsterPosition + new Vector2(Monster.GetBoundingBox().Width / 2f, Monster.GetBoundingBox().Height / 2f);
        if (Offset is not null)
            linkPosition += Offset.Value;
        return linkPosition;
    }
}
