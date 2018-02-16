using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGameJam1.Components.Player;
using MonoGameJam1.FSM;
using MonoGameJam1.Managers;
using Nez;
using Random = Nez.Random;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public class EnemyImpStates : State<EnemyImpStates, EnemyImpComponent>
    {
        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyImpSpawning : EnemyImpStates
    {
        public override void begin()
        {
            entity.sprite.play("spawn");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyImpThinking());
            }
        }
    }

    public class EnemyImpThinking : EnemyImpStates
    {
        private float _thinkingTime;

        public override void begin()
        {
            entity.sprite.play("stand");
            _thinkingTime = 1 + Random.nextFloat(2);
            Console.WriteLine($"Think: {_thinkingTime}");

        }

        public override void update()
        {
            _thinkingTime -= Time.deltaTime;
            if (_thinkingTime <= 0)
            {
                fsm.changeState(new EnemyImpRandomMove());
            }
        }
    }

    public class EnemyImpRandomMove : EnemyImpStates
    {
        private float _moveTime;
        private float _dir;

        public override void begin()
        {
            entity.sprite.play("walking");

            var direction = Random.nextInt(2) == 0 ? -1 : 1;
            var halfArea = entity.MovableArea.width / 2;
            var diffToCenter = diffToMovableAreaCenter();
            var dirToPoint = Math.Sign(diffToCenter);

            if (Math.Abs(diffToCenter) > halfArea / 2 && direction != dirToPoint)
            {
                direction = Random.nextFloat() >= 0.4f ? dirToPoint : direction;
            }

            entity.forceMovement(Vector2.UnitX * direction);
            _moveTime = 0.5f + Random.nextFloat(1);
            _dir = direction;
        }

        public override void update()
        {
            _moveTime -= Time.deltaTime;
            if (!entity.MovableArea.contains(entity.entity.position))
            {
                var diffToCenter = diffToMovableAreaCenter();
                var dirToPoint = Math.Sign(diffToCenter);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_dir != dirToPoint)
                    _dir *= -1;
                entity.forceMovement(Vector2.UnitX * _dir);
            }
            if (_moveTime <= 0)
            {
                fsm.resetStackTo(new EnemyImpThinking());
            }
            if (entity.canSeeThePlayer())
            {
                entity.turnToPlayer();
                fsm.resetStackTo(new EnemyImpAttackPlayer());
            }
        }

        public override void end()
        {
            entity.forceMovement(Vector2.Zero);
        }

        public float diffToMovableAreaCenter()
        {
            var halfArea = entity.MovableArea.width / 2;
            return entity.MovableArea.x + halfArea - entity.entity.position.X;
        }
    }

    public class EnemyImpAttackPlayer : EnemyImpStates
    {
        public override void begin()
        {
            entity.sprite.play("punch");
            entity.turnToPlayer();
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyImpThinking());
            }
        }
    }

    public class EnemyImpJumpAttack : EnemyImpStates
    {
        public override void begin()
        {
            
        }
    }
}
