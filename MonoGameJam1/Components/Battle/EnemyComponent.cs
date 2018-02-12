using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Colliders;
using MonoGameJam1.Components.Player;
using MonoGameJam1.Components.Sprites;
using MonoGameJam1.Managers;
using MonoGameJam1.Structs;
using Nez;

namespace MonoGameJam1.Components.Battle
{
    public class EnemyComponent : Component, IBattleEntity, IUpdatable
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Player collider reference

        public BoxCollider playerCollider;

        //--------------------------------------------------
        // Forced Movement

        private bool _forceMovement;
        private Vector2 _forceMovementVelocity;

        //--------------------------------------------------
        // Knockback

        private Vector2 _knockbackVelocity;
        private Vector2 _knockbackTick;

        //--------------------------------------------------
        // Platformer Object

        PlatformerObject _platformerObject;
        public PlatformerObject platformerObject => _platformerObject;

        //--------------------------------------------------
        // Battle Component

        protected BattleComponent _battleComponent;

        //--------------------------------------------------
        // Player Component

        protected PlayerComponent _playerComponent;

        //--------------------------------------------------
        // Area of Sight

        public AreaOfSightCollider areaOfSight;

        //--------------------------------------------------
        // Patrol

        public MapPath path;
        public bool patrolStartRight;
        private int _currentPatrolSide;
        private bool _sawThePlayer;

        //--------------------------------------------------
        // Dangerous Stage

        public int dangerousStage;

        //--------------------------------------------------
        // Can take damage

        public virtual bool canTakeDamage => true;

        //----------------------//------------------------//

        public EnemyComponent(bool patrolStartRight)
        {
            this.patrolStartRight = patrolStartRight;
            _currentPatrolSide = patrolStartRight ? 1 : -1;
        }

        public override void initialize()
        {
            dangerousStage = 1;
        }

        public override void onAddedToEntity()
        {
            _platformerObject = entity.getComponent<PlatformerObject>();

            _battleComponent = entity.getComponent<BattleComponent>();
            _battleComponent.setHp(1);
            _battleComponent.battleEntity = this;

            var player = Core.getGlobalManager<SystemManager>().playerEntity;
            _playerComponent = player.getComponent<PlayerComponent>();
        }

        public void increaseDangerousStage()
        {
            dangerousStage++;
        }

        public void forceMovement(Vector2 velocity)
        {
            if (velocity == Vector2.Zero)
            {
                _forceMovement = false;
            }
            else
            {
                _forceMovement = true;
                _forceMovementVelocity = velocity;
            }
        }

        public virtual void onHit(Vector2 knockback)
        {
            var xKnockback = _playerComponent.CurrentStateHorizontalKnockback();
            var yKnockback = _playerComponent.CurrentStateVerticalKnockback();

            var diff = Math.Sign((entity.position - _playerComponent.entity.position).X);
            knockback.X = diff;

            _knockbackTick = new Vector2(0.05f, yKnockback);
            _knockbackVelocity = new Vector2(diff * 2, -1000);

            if (Math.Abs(xKnockback) > 0.01)
            {
                _knockbackTick = new Vector2(xKnockback, yKnockback);
                _knockbackVelocity = new Vector2(diff * 60, -1000);
            }

            if (Math.Abs(yKnockback) < 0.001)
                platformerObject.velocity.Y = platformerObject.collisionState.below ? -100 : - 130;

        } 

        public virtual void onDeath() { }

        public virtual void update()
        {
            if (areaOfSight != null)
            {
                var offsetX = 0.0f;
                if (sprite.spriteEffects == SpriteEffects.FlipHorizontally)
                    offsetX = -2.0f * areaOfSight.X;
                areaOfSight.ApplyOffset(offsetX, 0);
            }

            // apply knockback before movement
            if (applyKnockback())
                return;

            var velocity = _forceMovement ? _forceMovementVelocity.X : 0.0f;
            if (canMove() && (velocity > 0 || velocity < 0))
            {
                var po = _platformerObject;
                var mms = po.maxMoveSpeed;
                var moveSpeed = po.moveSpeed;

                if (velocity != Math.Sign(po.velocity.X))
                {
                    po.velocity.X = 0;
                }
                po.velocity.X = moveSpeed * velocity * Time.deltaTime;
                sprite.spriteEffects = velocity < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }
            else
            {
                _platformerObject.velocity.X = 0;
            }
        }

        private bool applyKnockback()
        {
            if (_knockbackTick.X > 0)
            {
                _knockbackTick.X -= Time.deltaTime;
            }
            if (_knockbackTick.Y > 0)
            {
                _knockbackTick.Y -= Time.deltaTime;
            }

            var mms = _platformerObject.maxMoveSpeed * 5;
            var velx = _platformerObject.velocity.X;
            var vely = _platformerObject.velocity.Y;
            var appliedKb = false;
            if (_knockbackTick.X > 0)
            {
                _platformerObject.velocity.X = MathHelper.Clamp(velx + _platformerObject.moveSpeed * _knockbackVelocity.X * Time.deltaTime, -mms, mms);
                appliedKb = true;
            }
            if (_knockbackTick.Y > 0)
            {
                _platformerObject.velocity.Y = MathHelper.Clamp(vely + _platformerObject.moveSpeed * _knockbackVelocity.Y * Time.deltaTime, -mms, mms);
                appliedKb = true;
            }
            return appliedKb;
        }

        public bool canSeeThePlayer()
        {
            if (!playerCollider.entity.enabled) return false;
            var battler = playerCollider.entity.getComponent<BattleComponent>();
            if (battler.Dying) return false;
            CollisionResult collisionResult;
            return areaOfSight.collidesWith(playerCollider, out collisionResult);
        }

        public float distanceToPlayer()
        {
            return playerCollider.entity.position.X - entity.position.X;
        }

        public int currentPatrolSide()
        {
            return _sawThePlayer ? Math.Sign(distanceToPlayer()) : _currentPatrolSide;
        }

        public void switchPatrolSide()
        {
            _currentPatrolSide *= -1;
        }

        public void turnToPlayer()
        {
            _sawThePlayer = true;
            var side = distanceToPlayer();
            _currentPatrolSide = Math.Sign(side);
            sprite.spriteEffects = side > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }

        public void unseeThePlayer()
        {
            _sawThePlayer = false;
        }

        private bool canMove()
        {
            return !_battleComponent.Dying;
        }
    }
}
