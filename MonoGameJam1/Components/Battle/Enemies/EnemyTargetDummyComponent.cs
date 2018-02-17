using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Colliders;
using MonoGameJam1.Components.Sprites;
using Nez;
using System.Collections.Generic;

namespace MonoGameJam1.Components.Battle.Enemies
{
    class EnemyTargetDummyComponent : EnemyComponent
    {
        public EnemyTargetDummyComponent(bool patrolStartRight) : base(patrolStartRight)
        {
        }

        public override void initialize()
        {
            base.initialize();

            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.placeholder);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.25f);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });
            sprite.CreateAnimation("dying", 0.25f);
            sprite.AddFrames("dying", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            // View range
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-24, -12, 92, 32));
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();

            // Change HP
            entity.getComponent<BattleComponent>().setHp(5);
        }

    }
}
