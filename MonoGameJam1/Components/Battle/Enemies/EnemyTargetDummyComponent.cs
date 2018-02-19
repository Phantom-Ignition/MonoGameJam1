using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Colliders;
using MonoGameJam1.Components.Sprites;
using Nez;
using System.Collections.Generic;
using MonoGameJam1.FSM;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public class EnemyTargetDummyComponent : EnemyComponent
    {
        //--------------------------------------------------
        // Finite State Machine

        public FiniteStateMachine<EnemyTargetDummyStates, EnemyTargetDummyComponent> FSM { get; private set; }
        
        //----------------------//------------------------//

        public override void initialize()
        {
            base.initialize();

            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.dummy);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.1f, false);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 24, 34),
            }, new []{ 0 }, new []{ 6 });

            sprite.CreateAnimation("hit", 0.1f, false);
            sprite.AddFrames("hit", new List<Rectangle>
            {
                new Rectangle(24, 0, 24, 34),
                new Rectangle(48, 0, 24, 34),
            }, new[] { 0, 0 }, new[] { 6, 6 });

            sprite.CreateAnimation("dying", 0.1f, false);
            sprite.AddFrames("dying", new List<Rectangle>
            {
                new Rectangle(0, 0, 24, 34),
            }, new[] { 0 }, new[] { 6 });

            // collisor init
            entity.addComponent(new HurtCollider(-11, -10, 18, 34));

            // fsm
            FSM = new FiniteStateMachine<EnemyTargetDummyStates, EnemyTargetDummyComponent>(this, new EnemyTargetDumbIdle());
        }

        public override void onHit(Vector2 knockback)
        {
            FSM.resetStackTo(new EnemyTargetDumbHit());
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();

            // Change HP
            entity.getComponent<BattleComponent>().setMaxHp(99999, true);
        }

        public override void update()
        {
            FSM.update();
        }
    }
}
