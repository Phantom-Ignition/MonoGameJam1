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
    //--------------------------------------------------
    // Weapons

    public enum Weapon
    {
        Fist,
        Sword,
        Quarterstaff,
        Pistol
    }

    public class PlayerComponent : Component, IUpdatable, IBattleEntity
    {
        //--------------------------------------------------
        // Animations

        public enum Animations
        {
            Stand,
            Walking,
            Jumping,
            
            Fist1,
            Fist2,
            Fist3,
            Fist4,

            Sword1,
            Sword2,
            Sword3,

            Quarterstaff1,
            Quarterstaff2,
            Quarterstaff3,
            Quarterstaff4,

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
        // Can take damage

        public virtual bool canTakeDamage => true;

        //--------------------------------------------------
        // Footstep sound cooldown

        private float _footstepCooldown;

        //--------------------------------------------------
        // Current Weapon

        public Weapon CurrentWeapon;

        //--------------------------------------------------
        // Weapon selection

        private WeaponSelectionComponent _weaponSelectionComponent;

        //----------------------//------------------------//

        public override void initialize()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.player);

            _animationMap = new Dictionary<Animations, string>
            {
                {Animations.Stand, "stand"},
                {Animations.Walking, "walking"},
                {Animations.Jumping, "jumping"},

                {Animations.Fist1, "fist1"},
                {Animations.Fist2, "fist2"},
                {Animations.Fist3, "fist3"},
                {Animations.Fist4, "fist4"},

                {Animations.Sword1, "sword1"},
                {Animations.Sword2, "sword2"},
                {Animations.Sword3, "sword3"},

                {Animations.Quarterstaff1, "quarterstaff1"},
                {Animations.Quarterstaff2, "quarterstaff2"},
                {Animations.Quarterstaff3, "quarterstaff3"},
                {Animations.Quarterstaff4, "quarterstaff4"},

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
                new Rectangle(32, 0, 32, 32),
            });

            // == FIRST ATTACKS ==

            #region Fist Animations
            
            sprite.CreateAnimation(am[Animations.Fist1], 0.1f);
            sprite.AddFrames(am[Animations.Fist1], new List<Rectangle>()
            {
                new Rectangle(0, 32, 32, 32),
                new Rectangle(0, 32, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Fist1], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Fist1], 1);

            sprite.CreateAnimation(am[Animations.Fist2], 0.1f);
            sprite.AddFrames(am[Animations.Fist2], new List<Rectangle>()
            {
                new Rectangle(32, 32, 32, 32),
                new Rectangle(32, 32, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Fist2], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Fist2], 1);

            sprite.CreateAnimation(am[Animations.Fist3], 0.1f);
            sprite.AddFrames(am[Animations.Fist3], new List<Rectangle>()
            {
                new Rectangle(64, 32, 32, 32),
                new Rectangle(64, 32, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Fist3], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Fist3], 1);

            sprite.CreateAnimation(am[Animations.Fist4], 0.1f);
            sprite.AddFrames(am[Animations.Fist4], new List<Rectangle>()
            {
                new Rectangle(96, 32, 32, 32),
                new Rectangle(96, 32, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Fist4], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Fist4], 1);

            #endregion

            // == SWORD ATTACKS ==

            #region Sword Animations

            sprite.CreateAnimation(am[Animations.Sword1], 0.1f);
            sprite.AddFrames(am[Animations.Sword1], new List<Rectangle>()
            {
                new Rectangle(0, 64, 32, 32),
                new Rectangle(0, 64, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Sword1], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Sword1], 1);

            sprite.CreateAnimation(am[Animations.Sword2], 0.1f);
            sprite.AddFrames(am[Animations.Sword2], new List<Rectangle>()
            {
                new Rectangle(32, 64, 32, 32),
                new Rectangle(32, 64, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Sword2], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Sword2], 1);

            sprite.CreateAnimation(am[Animations.Sword3], 0.1f);
            sprite.AddFrames(am[Animations.Sword3], new List<Rectangle>()
            {
                new Rectangle(64, 64, 32, 32),
                new Rectangle(64, 64, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Sword3], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Sword3], 1);

            #endregion

            // == QUARTERSTAFF ATTACKS ==

            #region Quarterstaff Animations

            sprite.CreateAnimation(am[Animations.Quarterstaff1], 0.1f);
            sprite.AddFrames(am[Animations.Quarterstaff1], new List<Rectangle>()
            {
                new Rectangle(0, 96, 32, 32),
                new Rectangle(0, 96, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Quarterstaff1], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 60, 18) },
            });
            sprite.AddFramesToAttack(am[Animations.Quarterstaff1], 1);

            sprite.CreateAnimation(am[Animations.Quarterstaff2], 0.1f);
            sprite.AddFrames(am[Animations.Quarterstaff2], new List<Rectangle>()
            {
                new Rectangle(32, 96, 32, 32),
                new Rectangle(32, 96, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Quarterstaff2], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 60, 18) },
            });
            sprite.AddFramesToAttack(am[Animations.Quarterstaff2], 1);

            sprite.CreateAnimation(am[Animations.Quarterstaff3], 0.1f);
            sprite.AddFrames(am[Animations.Quarterstaff3], new List<Rectangle>()
            {
                new Rectangle(64, 96, 32, 32),
                new Rectangle(64, 96, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Quarterstaff3], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 60, 18) },
            });
            sprite.AddFramesToAttack(am[Animations.Quarterstaff3], 1);

            sprite.CreateAnimation(am[Animations.Quarterstaff4], 0.1f);
            sprite.AddFrames(am[Animations.Quarterstaff4], new List<Rectangle>()
            {
                new Rectangle(96, 96, 32, 32),
                new Rectangle(96, 96, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Quarterstaff4], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 60, 18) },
            });
            sprite.AddFramesToAttack(am[Animations.Quarterstaff4], 1);

            #endregion

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

            _weaponSelectionComponent = entity.addComponent<WeaponSelectionComponent>();
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

        public void SetGravity(float gravity)
        {
            platformerObject.gravity = gravity;
        }

        public void OpenWeaponSelection()
        {
            _weaponSelectionComponent.Open();
        }

        public void ChangeWeapon(Weapon newWeapon)
        {
            CurrentWeapon = newWeapon;
            _fsm.resetStackTo(new StandState());
        }

        #region Helpers

        public Entity createEntityOnMap()
        {
            return entity.scene.createEntity();
        }

        public float CurrentStateVerticalKnockback()
        {
            var comboState = _fsm.CurrentState as BaseAttackComboState;
            return comboState?.VerticalKnockback ?? 0.0f;
        }

        public float CurrentStateHorizontalKnockback()
        {
            var comboState = _fsm.CurrentState as BaseAttackComboState;
            return comboState?.HorizontalKnockback ?? 0.0f;
        }

        public void Jump()
        {
            _platformerObject.jump();
        }

        public float GetDeltaTimeFunc()
        {
            return Time.deltaTime;
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

        #endregion
    }
}
