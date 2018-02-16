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

            Sword1,
            Sword2,
            Sword3,

            Quarterstaff1,
            Quarterstaff2,
            Quarterstaff3,
            Quarterstaff4,

            Pistol1,
            Pistol2,
            Pistol3,
            Pistol4,
            
            Hit,
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
        private Direction? _lockDirection;

        //--------------------------------------------------
        // Velocity multiplier

        public float velocityMultiplier;

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

        //--------------------------------------------------
        // Damage scaling

        private float _damageScalingStreak;

        //--------------------------------------------------
        // Linkable frames

        public Dictionary<Animations, int[]> LinkableFrames { get; private set; }

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

                {Animations.Sword1, "sword1"},
                {Animations.Sword2, "sword2"},
                {Animations.Sword3, "sword3"},

                {Animations.Quarterstaff1, "quarterstaff1"},
                {Animations.Quarterstaff2, "quarterstaff2"},
                {Animations.Quarterstaff3, "quarterstaff3"},
                {Animations.Quarterstaff4, "quarterstaff4"},

                {Animations.Pistol1, "pistol1"},
                {Animations.Pistol2, "pistol2"},
                {Animations.Pistol3, "pistol3"},
                {Animations.Pistol4, "pistol4"},

                {Animations.Hit, "hit"},
                {Animations.Dying, "dying"},
            };

            LinkableFrames = new Dictionary<Animations, int[]>();

            var am = _animationMap;

            sprite = entity.addComponent(new AnimatedSprite(texture, am[Animations.Stand]));
            sprite.CreateAnimation(am[Animations.Stand], 0.1f);
            sprite.AddFrames(am[Animations.Stand], new List<Rectangle>()
            {
                new Rectangle(0, 0, 100, 100),
                new Rectangle(100, 0, 100, 100),
                new Rectangle(200, 0, 100, 100),
                new Rectangle(300, 0, 100, 100),
                new Rectangle(400, 0, 100, 100),
                new Rectangle(500, 0, 100, 100),
                new Rectangle(600, 0, 100, 100),
                new Rectangle(700, 0, 100, 100),
            });

            sprite.CreateAnimation(am[Animations.Walking], 0.09f);
            sprite.AddFrames(am[Animations.Walking], new List<Rectangle>()
            {
                new Rectangle(0, 900, 100, 100),
                new Rectangle(100, 900, 100, 100),
                new Rectangle(200, 900, 100, 100),
                new Rectangle(300, 900, 100, 100),
                new Rectangle(400, 900, 100, 100),
                new Rectangle(500, 900, 100, 100),
                new Rectangle(600, 900, 100, 100),
                new Rectangle(700, 900, 100, 100),
            });

            sprite.CreateAnimation(am[Animations.Jumping], 0.1f);
            sprite.AddFrames(am[Animations.Jumping], new List<Rectangle>()
            {
                new Rectangle(400, 700, 100, 100),
            });

            // == FIRST ATTACKS ==

            #region Fist Animations
            
            sprite.CreateAnimation(am[Animations.Fist1], 0.09f, false);
            sprite.AddFrames(am[Animations.Fist1], new List<Rectangle>()
            {
                new Rectangle(100, 100, 100, 100),
                new Rectangle(200, 100, 100, 100),
                new Rectangle(300, 100, 100, 100),
                new Rectangle(400, 100, 100, 100),
                new Rectangle(500, 100, 100, 100),
            });
            sprite.AddAttackCollider(am[Animations.Fist1], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Fist1], 1);
            LinkableFrames[Animations.Fist1] = new[] {2, 3};

            sprite.CreateAnimation(am[Animations.Fist2], 0.09f, false);
            sprite.AddFrames(am[Animations.Fist2], new List<Rectangle>()
            {
                new Rectangle(700, 100, 100, 100),
                new Rectangle(0, 200, 100, 100),
                new Rectangle(100, 200, 100, 100),
                new Rectangle(200, 200, 100, 100),
                new Rectangle(300, 200, 100, 100),
                new Rectangle(400, 200, 100, 100),
                new Rectangle(500, 200, 100, 100),
            });
            sprite.AddAttackCollider(am[Animations.Fist2], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Fist2], 1);
            LinkableFrames[Animations.Fist2] = new[] {3, 4};

            sprite.CreateAnimation(am[Animations.Fist3], 0.09f, false);
            sprite.AddFrames(am[Animations.Fist3], new List<Rectangle>()
            {
                new Rectangle(700, 200, 100, 100),
                new Rectangle(0, 300, 100, 100),
                new Rectangle(100, 300, 100, 100),
                new Rectangle(200, 300, 100, 100),
                new Rectangle(300, 300, 100, 100),
                new Rectangle(400, 300, 100, 100),
                new Rectangle(500, 300, 100, 100),
            });
            sprite.AddAttackCollider(am[Animations.Fist3], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Fist3], 1);

            #endregion

            // == SWORD ATTACKS ==

            #region Sword Animations

            sprite.CreateAnimation(am[Animations.Sword1], 0.09f, false);
            sprite.AddFrames(am[Animations.Sword1], new List<Rectangle>()
            {
                new Rectangle(700, 300, 100, 100),
                new Rectangle(0, 400, 100, 100),
                new Rectangle(100, 400, 100, 100),
                new Rectangle(200, 400, 100, 100),
                new Rectangle(300, 400, 100, 100),
            });
            sprite.AddAttackCollider(am[Animations.Sword1], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Sword1], 1);
            LinkableFrames[Animations.Sword1] = new[] { 2, 3 };

            sprite.CreateAnimation(am[Animations.Sword2], 0.09f, false);
            sprite.AddFrames(am[Animations.Sword2], new List<Rectangle>()
            {
                new Rectangle(500, 400, 100, 100),
                new Rectangle(600, 400, 100, 100),
                new Rectangle(700, 400, 100, 100),
                new Rectangle(0, 500, 100, 100),
                new Rectangle(100, 500, 100, 100),
                new Rectangle(200, 500, 100, 100),
            });
            sprite.AddAttackCollider(am[Animations.Sword2], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 34, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Sword2], 1);
            LinkableFrames[Animations.Sword2] = new[] { 2, 3 };

            sprite.CreateAnimation(am[Animations.Sword3], 0.09f, false);
            sprite.AddFrames(am[Animations.Sword3], new List<Rectangle>()
            {
                new Rectangle(400, 500, 100, 100),
                new Rectangle(500, 500, 100, 100),
                new Rectangle(600, 500, 100, 100),
                new Rectangle(700, 500, 100, 100),
                new Rectangle(0, 600, 100, 100),
                new Rectangle(100, 600, 100, 100),
                new Rectangle(200, 600, 100, 100),
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

            sprite.CreateAnimation(am[Animations.Quarterstaff1], 0.1f, false);
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

            sprite.CreateAnimation(am[Animations.Quarterstaff2], 0.1f, false);
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

            sprite.CreateAnimation(am[Animations.Quarterstaff3], 0.1f, false);
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

            sprite.CreateAnimation(am[Animations.Quarterstaff4], 0.1f, false);
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
            
            // == PISTOL ATTACKS ==

            #region Pistol Animations

            sprite.CreateAnimation(am[Animations.Pistol1], 0.07f, false);
            sprite.AddFrames(am[Animations.Pistol1], new List<Rectangle>()
            {
                new Rectangle(500, 600, 100, 100),
                new Rectangle(600, 600, 100, 100),
                new Rectangle(700, 600, 100, 100),
                new Rectangle(0, 700, 100, 100),
                new Rectangle(100, 700, 100, 100),
                new Rectangle(200, 700, 100, 100),
            });
            sprite.AddAttackCollider(am[Animations.Pistol1], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 128, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Pistol1], 1);
            LinkableFrames[Animations.Pistol1] = new[] { 3, 4 };

            sprite.CreateAnimation(am[Animations.Pistol2], 0.07f, false);
            sprite.AddFrames(am[Animations.Pistol2], new List<Rectangle>()
            {
                new Rectangle(500, 600, 100, 100),
                new Rectangle(600, 600, 100, 100),
                new Rectangle(700, 600, 100, 100),
                new Rectangle(0, 700, 100, 100),
                new Rectangle(100, 700, 100, 100),
                new Rectangle(200, 700, 100, 100),
            });
            sprite.AddAttackCollider(am[Animations.Pistol2], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 128, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Pistol2], 1);
            LinkableFrames[Animations.Pistol2] = new[] { 3, 4 };

            sprite.CreateAnimation(am[Animations.Pistol3], 0.07f, false);
            sprite.AddFrames(am[Animations.Pistol3], new List<Rectangle>()
            {
                new Rectangle(500, 600, 100, 100),
                new Rectangle(600, 600, 100, 100),
                new Rectangle(700, 600, 100, 100),
                new Rectangle(0, 700, 100, 100),
                new Rectangle(100, 700, 100, 100),
                new Rectangle(200, 700, 100, 100),
            });
            sprite.AddAttackCollider(am[Animations.Pistol3], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 128, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Pistol3], 1);
            LinkableFrames[Animations.Pistol3] = new[] { 3, 4 };

            sprite.CreateAnimation(am[Animations.Pistol4], 0.1f, false);
            sprite.AddFrames(am[Animations.Pistol4], new List<Rectangle>()
            {
                new Rectangle(96, 128, 32, 32),
                new Rectangle(96, 128, 32, 32),
            });
            sprite.AddAttackCollider(am[Animations.Pistol4], new List<List<Rectangle>>
            {
                new List<Rectangle>(),
                new List<Rectangle> { new Rectangle(0, -10, 128, 29) },
            });
            sprite.AddFramesToAttack(am[Animations.Pistol4], 1);

            #endregion

            // == MISC ANIMATIONS ==

            sprite.CreateAnimation(am[Animations.Hit], 0.09f, false);
            sprite.AddFrames(am[Animations.Hit], new List<Rectangle>()
            {
                new Rectangle(500, 700, 100, 100),
                new Rectangle(600, 700, 100, 100),
            });

            sprite.CreateAnimation(am[Animations.Dying], 0.09f, false);
            sprite.AddFrames(am[Animations.Dying], new List<Rectangle>()
            {
                new Rectangle(0, 800, 100, 100),
                new Rectangle(100, 800, 100, 100),
                new Rectangle(200, 800, 100, 100),
                new Rectangle(300, 800, 100, 100),
                new Rectangle(400, 800, 100, 100),
                new Rectangle(500, 800, 100, 100),
                new Rectangle(600, 800, 100, 100),
                new Rectangle(700, 800, 100, 100),
            });

            // init fsm
            _fsm = new FiniteStateMachine<PlayerState, PlayerComponent>(this, new StandState());
            
            // misc
            _damageScalingStreak = 10;
            velocityMultiplier = 1;
        }

        public override void onAddedToEntity()
        {
            _platformerObject = entity.getComponent<PlatformerObject>();
            _platformerObject.setGetDeltaTimeFunc(GetDeltaTimeFunc);

            _battleComponent = entity.getComponent<BattleComponent>();
            _battleComponent.battleEntity = this;
            _battleComponent.ImmunityDuration = 0.5f;
            _battleComponent.destroyEntityAction = destroyEntity;
            _battleComponent.setHp(10);

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
            FSM.changeState(new HitState());
        }

        public void onDeath()
        {
            FSM.changeState(new DyingState());
        }

        public void forceMovement(Vector2 velocity, Direction? lockDirection = null, bool walljumpForcedMovement = false)
        {
            if (velocity == Vector2.Zero)
            {
                _forceMovement = false;
                _lockDirection = null;
            }
            else
            {
                _forceMovement = true;
                _forceMovementVelocity = velocity;
            }
            if (lockDirection.HasValue)
            {
                _lockDirection = lockDirection.Value;
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
                po.velocity.X = (int)MathHelper.Clamp(po.velocity.X + moveSpeed * velocity * Time.unscaledDeltaTime, -mms, mms) * velocityMultiplier;

                if (platformerObject.grabbingWall)
                {
                    po.velocity.X = po.grabbingWallSide * mms;
                    sprite.spriteEffects = po.grabbingWallSide == -1
                        ? SpriteEffects.FlipHorizontally
                        : SpriteEffects.None;
                }
                else
                {
                    if (_lockDirection.HasValue)
                    {
                        sprite.spriteEffects = _lockDirection.Value == Direction.Left
                            ? SpriteEffects.FlipHorizontally
                            : SpriteEffects.None;
                    }
                    else
                    {
                        sprite.spriteEffects = velocity < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    }
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
            _damageScalingStreak = 10;
        }

        public void ReduceDamageScale()
        {
            _damageScalingStreak -= 0.5f;
            if (_damageScalingStreak < 3)
                _damageScalingStreak = 3;
        }

        #region Helpers

        public bool CanLinkCombo(Animations anim)
        {
            return LinkableFrames.ContainsKey(anim) && LinkableFrames[anim].contains(sprite.CurrentFrame);
        }

        public float CurrentDamageScale()
        {
            // Max: 1   (100%)
            // Min: 0.3 (30%)
            return Math.Max(Math.Max(0, Math.Min(10, _damageScalingStreak)), 3) / 10;
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

        public int GetIntDirection()
        {
            return sprite.spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1;
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
