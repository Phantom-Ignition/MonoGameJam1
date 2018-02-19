using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Colliders;
using MonoGameJam1.Components.Sprites;
using MonoGameJam1.FSM;
using Nez;
using System.Collections.Generic;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public class EnemyTrapComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        public FiniteStateMachine<EnemyTrapState, EnemyTrapComponent> FSM { get; private set; }

        //--------------------------------------------------
        // Cooldown

        public float Cooldown { get; set; }

        //----------------------//------------------------//

        public EnemyTrapComponent(bool patrolStartRight) : base(patrolStartRight)
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
                new Rectangle(0, 0, 16, 16),
            }, new[] {0}, new[] {15});

            sprite.CreateAnimation("active", 0.09f, false);
            sprite.AddFrames("active", new List<Rectangle>
            {
                new Rectangle(0, 0, 16, 16),
                new Rectangle(16, 0, 16, 16),
                new Rectangle(16, 0, 16, 16),
                new Rectangle(16, 0, 16, 16),
                new Rectangle(32, 0, 16, 16),
                new Rectangle(48, 0, 16, 16),
                new Rectangle(64, 0, 16, 16),
            }, new[] { 0, 0, 0, 0, 0, 0, 0 }, new[] { 15, 15, 15, 15, 15, 15, 15 });
            sprite.AddAttackCollider("active", new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle>(),
                new List<Rectangle>(),
                new List<Rectangle>(),
                new List<Rectangle>(),
                new List<Rectangle>(),
                new List<Rectangle> {new Rectangle(-7, 6, 14, 16)}
            });
            sprite.AddFramesToAttack("active", 6);

            // FSM
            FSM = new FiniteStateMachine<EnemyTrapState, EnemyTrapComponent>(this, new EnemyTrapIdle());

            // View range
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-16, -8, 32, 32));
        }

        public override void update()
        {
            base.update();
            FSM.update();
            if (Cooldown > 0.0f)
                Cooldown -= Time.deltaTime;
        }
    }
}
