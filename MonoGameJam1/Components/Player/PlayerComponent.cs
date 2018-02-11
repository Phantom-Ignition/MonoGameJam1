using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Components.Sprites;
using MonoGameJam1.FSM;
using MonoGameJam1.Managers;
using MonoGameJam1.Scenes;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using System;
using System.Collections.Generic;

namespace MonoGameJam1.Components.Player
{
    public class PlayerComponent : Component, IUpdatable, IBattleEntity
    {
        //--------------------------------------------------
        // Animations

        public enum Animations
        {
            Stand,
            Walking,
            Jumping,
            Shot,
            Dying
        }

        private Dictionary<Animations, string> _animationMap;

        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Platformer Object

        PlatformerObject _platformerObject;
        public PlatformerObject platformerObject => _platformerObject;

        //--------------------------------------------------
        // Collision State

        public TiledMapMover.CollisionState CollisionState => _platformerObject.collisionState;
        public bool ForcedGround { get; set; }

        //--------------------------------------------------
        // Velocity

        public Vector2 Velocity => _platformerObject.velocity;

        //--------------------------------------------------
        // Finite State Machine

        private FiniteStateMachine<PlayerState, PlayerComponent> _fsm;
        public FiniteStateMachine<PlayerState, PlayerComponent> FSM => _fsm;

        //--------------------------------------------------
        // Forced Movement

        private bool _forceMovement;
        private Vector2 _forceMovementVelocity;
        private bool _walljumpForcedMovement;

        //--------------------------------------------------
        // Knockback

        private Vector2 _knockbackVelocity;
        private Vector2 _knockbackTick;

        //--------------------------------------------------
        // Forced position

        public bool forcePosition;
        public float forcedPositionX;
        public float forcedPositionY;

        //--------------------------------------------------
        // Battler

        private BattleComponent _battleComponent;

        //--------------------------------------------------
        // Is on bush

        public bool isOnBush;
        public bool isInsideBush;

        //--------------------------------------------------
        // Can take damage

        public virtual bool canTakeDamage => true;

        //--------------------------------------------------
        // Footstep sound cooldown

        private float _footstepCooldown;

        //--------------------------------------------------
        // Distortion cursor

        private Sprite _distortionCursorSprite;

        //----------------------//------------------------//

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.placeholder);

            _animationMap = new Dictionary<Animations, string>
            {
                {Animations.Stand, "stand"},
                {Animations.Walking, "walking"},
                {Animations.Jumping, "jumping"},
                {Animations.Shot, "shot"},
                {Animations.Dying, "dying"},
            };

            var am = _animationMap;

            sprite = entity.addComponent(new AnimatedSprite(texture, am[Animations.Stand]));
            sprite.CreateAnimation(am[Animations.Stand], 0.2f);
            sprite.AddFrames(am[Animations.Stand], new List<Rectangle>()
            {
                new Rectangle(0, 0, 32, 32),
            });

            sprite.CreateAnimation(am[Animations.Walking], 0.1f);
            sprite.AddFrames(am[Animations.Walking], new List<Rectangle>()
            {
                new Rectangle(32, 0, 32, 32),
            });

            sprite.CreateAnimation(am[Animations.Jumping], 0.1f);
            sprite.AddFrames(am[Animations.Jumping], new List<Rectangle>()
            {
                new Rectangle(0, 32, 32, 32),
            });

            sprite.CreateAnimation(am[Animations.Shot], 0.1f);
            sprite.AddFrames(am[Animations.Shot], new List<Rectangle>()
            {
                new Rectangle(32, 0, 32, 32),
                new Rectangle(32, 0, 32, 32),
            });

            sprite.CreateAnimation(am[Animations.Dying], 0.1f);
            sprite.AddFrames(am[Animations.Dying], new List<Rectangle>()
            {
                new Rectangle(32, 32, 32, 32),
                new Rectangle(32, 32, 32, 32),
            });

            // init fsm
            _fsm = new FiniteStateMachine<PlayerState, PlayerComponent>(this, new StandState());
            
        }

        public override void onAddedToEntity()
        {
            _platformerObject = entity.getComponent<PlatformerObject>();
            _platformerObject.setGetDeltaTimeFunc(GetDeltaTimeFunc);

            _battleComponent = entity.getComponent<BattleComponent>();
            _battleComponent.battleEntity = this;
            _battleComponent.ImmunityDuration = 0.5f;
            _battleComponent.destroyEntityAction = destroyEntity;
        }

        public void destroyEntity()
        {
            entity.setEnabled(false);
            Core.startSceneTransition(new WindTransition(() => new SceneMap()));
        }

        public void onHit(Vector2 knockback)
        {
            _knockbackTick = new Vector2(0.06f, 0.04f);
            _knockbackVelocity = new Vector2(knockback.X * 60, -5);
        }

        public void onDeath()
        {
            FSM.changeState(new DyingState());
        }

        public void forceMovement(Vector2 velocity, bool walljumpForcedMovement = false)
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
            _walljumpForcedMovement = walljumpForcedMovement;
        }

        public void update()
        {
            // Update FSM
            _fsm.update();

            // apply knockback before movement
            if (applyKnockback())
                return;

            if (forcePosition)
            {
                var pos = new Vector2(forcedPositionX, forcedPositionY);
                entity.setPosition(pos);
            }

            var axis = Core.getGlobalManager<InputManager>().MovementAxis.value;
            var velocity = _forceMovement ? _forceMovementVelocity.X : axis;
            if (canMove() && (velocity > 0 || velocity < 0))
            {
                if (isOnGround() && axis != 0 && _footstepCooldown <= 0.0f)
                {
                    _footstepCooldown = 0.25f;
                    // footstep sound
                    // AudioManager.footstep.Play(0.7f);
                }
                else
                {
                    _footstepCooldown -= Time.deltaTime;
                }

                var po = _platformerObject;
                var mms = po.maxMoveSpeed;
                var moveSpeed = _walljumpForcedMovement ? po.gravity * mms : po.moveSpeed;

                if (velocity != Math.Sign(po.velocity.X))
                {
                    po.velocity.X = 0;
                }
                po.velocity.X = (int)MathHelper.Clamp(po.velocity.X + moveSpeed * velocity * Time.unscaledDeltaTime, -mms, mms);

                if (platformerObject.grabbingWall)
                {
                    po.velocity.X = po.grabbingWallSide * mms;
                    sprite.spriteEffects = po.grabbingWallSide == -1
                        ? SpriteEffects.FlipHorizontally
                        : SpriteEffects.None;
                }
                else
                {
                    sprite.spriteEffects = velocity < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                }
            }
            else
            {
                _platformerObject.velocity.X = 0;
            }

            ForcedGround = false;
        }

        private bool applyKnockback()
        {
            if (_knockbackTick.X > 0)
            {
                _knockbackTick.X -= Time.unscaledDeltaTime;
            }
            if (_knockbackTick.Y > 0)
            {
                _knockbackTick.Y -= Time.unscaledDeltaTime;
            }

            var mms = _platformerObject.maxMoveSpeed;
            var velx = _platformerObject.velocity.X;
            var vely = _platformerObject.velocity.Y;
            var appliedKb = false;
            if (_knockbackTick.X > 0)
            {
                _platformerObject.velocity.X = MathHelper.Clamp(velx + _platformerObject.moveSpeed * _knockbackVelocity.X * Time.unscaledDeltaTime, -mms, mms);
                appliedKb = true;
            }
            if (_knockbackTick.Y > 0)
            {
                _platformerObject.velocity.Y = MathHelper.Clamp(vely + _platformerObject.moveSpeed * _knockbackVelocity.Y * Time.unscaledDeltaTime, -mms, mms);
                appliedKb = true;
            }
            return appliedKb;
        }

        public void SetAnimation(Animations animation)
        {
            var animationStr = _animationMap[animation];
            if (sprite.CurrentAnimation != animationStr)
            {
                sprite.play(animationStr);
            }
        }

        public Entity createEntityOnMap()
        {
            return entity.scene.createEntity();
        }

        public void Jump()
        {
            _platformerObject.jump();
        }

        public float GetDeltaTimeFunc()
        {
            return Time.unscaledDeltaTime;
        }

        private bool canMove()
        {
            return (Core.getGlobalManager<InputManager>().isMovementAvailable() || _forceMovement) &&
                   !_battleComponent.Dying;
        }

        public bool isOnGround()
        {
            return ForcedGround || _platformerObject.collisionState.below;
        }
    }
}
