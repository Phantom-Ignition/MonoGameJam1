using Microsoft.Xna.Framework;
using MonoGameJam1.FSM;
using Nez;
using System;
using MonoGameJam1.Components.Player;
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
        private readonly bool _lessTime;
        private float _thinkingTime;

        public EnemyImpThinking(bool lessTime = false)
        {
            _lessTime = lessTime;
        }

        public override void begin()
        {
            entity.sprite.play("stand");
            _thinkingTime = _lessTime ? Random.nextFloat(1) : 1 + Random.nextFloat(2);
        }

        public override void update()
        {
            _thinkingTime -= Time.deltaTime;
            if (_thinkingTime <= 0 || entity.canSeeThePlayer())
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
                var distance = entity.distanceToPlayer();
                if (Math.Abs(distance) < 60)
                {
                    var rand = Random.nextFloat(1);
                    if (rand < 0.7)
                    {
                        fsm.resetStackTo(new EnemyImpAttackPlayer());
                    }
                    else
                    {
                        fsm.resetStackTo(new EnemyImpJumpAttack());
                    }
                }
                else
                {
                    fsm.resetStackTo(new EnemyImpJumpAttack());
                }
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
                fsm.resetStackTo(new EnemyImpThinking(true));
            }
        }
    }

    public class EnemyImpJumpAttack : EnemyImpStates
    {
        private bool _jumped;

        public override void begin()
        {
            entity.sprite.play("jumpAttack");
        }

        private void jump()
        {
            var dir = Math.Sign(entity.distanceToPlayer());
            entity.platformerObject.velocity = new Vector2(0, -300);
            entity.changeSpeed(ImpVelocity.Fast);
            entity.forceMovement(Vector2.UnitX * dir);
        }

        public override void update()
        {
            if (!_jumped && entity.sprite.CurrentAnimation == "jumpAttack" && entity.sprite.CurrentFrame >= 4)
            {
                _jumped = true;
                jump();
            }
            if (_jumped && entity.sprite.Looped && entity.isOnGround())
            {
                fsm.resetStackTo(new EnemyImpThinking(true));
            }
        }

        public override void end()
        {
            entity.changeSpeed(ImpVelocity.Normal);
            entity.forceMovement(Vector2.Zero);
        }
    }

    public class EnemyImpHit : EnemyImpStates
    {
        public override void begin()
        {
            entity.sprite.play("hit");
        }

        public override void update()
        {
            if (entity.isOnGround() && entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyImpThinking(true));
            }
        }
    }

    public class EnemyImpDying : EnemyImpStates
    {
        public override void begin()
        {
            entity.sprite.play("dying");
        }
    }
}
