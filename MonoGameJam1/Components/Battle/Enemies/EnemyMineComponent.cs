using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Sprites;
using Nez;
using System.Collections.Generic;
using MonoGameJam1.Components.Colliders;
using MonoGameJam1.FSM;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public class EnemyMineComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        public FiniteStateMachine<EnemyMineStates, EnemyMineComponent> FSM { get; private set; }

        //----------------------//------------------------//

        public EnemyMineComponent(bool patrolStartRight) : base(patrolStartRight)
        {
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            platformerObject.maxMoveSpeed = 0;
            platformerObject.moveSpeed = 0;
        }

        public override void initialize()
        {
            base.initialize();

            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.trap);
            sprite = entity.addComponent(new AnimatedSprite(texture, "idle"));
            sprite.CreateAnimation("idle", 0.1f);
            sprite.AddFrames("idle", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            sprite.CreateAnimation("ticking", 0.1f);
            sprite.AddFrames("ticking", new List<Rectangle>
            {
                new Rectangle(32, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
            });

            sprite.CreateAnimation("explode", 0.1f);
            sprite.AddFrames("explode", new List<Rectangle>
            {
                new Rectangle(64, 0, 32, 32),
            });
            sprite.AddAttackCollider("explode", new List<List<Rectangle>>
            {
                new List<Rectangle> {new Rectangle(-16, -16, 32, 32)}
            });
            sprite.AddFramesToAttack("explode", 0);

            // FSM
            FSM = new FiniteStateMachine<EnemyMineStates, EnemyMineComponent>(this, new EnemyMineIdle());

            // View range
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-16, -12, 32, 32));
        }

        public override void update()
        {
            FSM.update();
            base.update();
        }
    }
}
