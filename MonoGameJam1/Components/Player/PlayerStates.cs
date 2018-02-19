using Microsoft.Xna.Framework;
using MonoGameJam1.Extensions;
using MonoGameJam1.FSM;
using MonoGameJam1.Managers;
using Nez;

namespace MonoGameJam1.Components.Player
{
    public class PlayerState : State<PlayerState, PlayerComponent>
    {
        protected InputManager _input => Core.getGlobalManager<InputManager>();
        private float _dashTick;
        private int _dashSide;

        public override void begin() { }

        public override void end() { }

        public void handleInput()
        {
            if (isMovementAvailable())
            {
                if (entity.isOnGround() && _input.JumpButton.isPressed)
                {
                    fsm.resetStackTo(new JumpingState(true));
                }
                else if (entity.isOnGround() && isDashRequested())
                {
                    fsm.pushState(new DashState());
                }
                else if (_input.AttackButton.isPressed && !entity.SkipAttackState)
                {
                    switch (entity.CurrentWeapon)
                    {
                        case Weapon.Fist:
                            fsm.pushState(new FistAttack1());
                            break;
                        case Weapon.Sword:
                            fsm.pushState(new SwordAttack1());
                            break;
                        case Weapon.Quarterstaff:
                            fsm.pushState(new QuarterstaffAttack1());
                            break;
                        case Weapon.Pistol:
                            fsm.pushState(new PistolAttack1());
                            break;
                    }
                }
                else if (entity.SkipAttackState)
                {
                    entity.SkipAttackState = false;
                }
            }
            if (_input.WeaponSelectionButton.isPressed)
            {
                entity.OpenWeaponSelection();
            }
            if (_dashTick > 0)
                _dashTick -= Time.deltaTime;
        }

        private bool isDashRequested()
        {
            var dashSide = _input.LeftButton.isPressed ? -1 : _input.RightButton.isPressed ? 1 : 0;
            if (dashSide == 0) return false;
            if (_dashSide == dashSide && _dashTick > 0.0f) return true;
            _dashSide = dashSide;
            _dashTick = 5f;
            return false;
        }

        protected bool isMovementAvailable()
        {
            return Core.getGlobalManager<InputManager>().isMovementAvailable();
        }

        public override void update()
        {
            handleInput();
        }
    }

    public class StandState : PlayerState
    {
        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Stand);
        }

        public override void update()
        {
            base.update();

            if (!entity.isOnGround())
            {
                fsm.changeState(new JumpingState(false));
                return;
            }

            if (entity.isOnGround())
            {
                if (entity.Velocity.X > 0 || entity.Velocity.X < 0)
                {
                    entity.SetAnimation(PlayerComponent.Animations.Walking);
                }
                else
                {
                    entity.SetAnimation(PlayerComponent.Animations.Stand);
                }
            }
        }
    }

    public class JumpingState : PlayerState
    {
        private bool _needJump;

        public JumpingState(bool needJump)
        {
            _needJump = needJump;
        }

        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Jumping);
            if (_needJump)
            {
                _needJump = false;
                entity.Jump();
                AudioManager.jump.Play(0.6f);
                if (entity.isOnGround())
                    entity.createJumpEffect("jump");
            }
        }

        public override void update()
        {
            base.update();

            if (entity.isOnGround())
            {
                fsm.resetStackTo(new StandState());
                entity.createJumpEffect("land");
            }
        }
    }

    public class DashState : PlayerState
    {
        private ITimer _timer;
        public float AttackPushDuration = 0.15f;
        public float VelocityMultiplier = 2.6f;

        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Jumping);
            entity.SetDashTrail(true);

            _input.IsLocked = true;
            entity.forceMovement(entity.GetIntDirection() * Vector2.UnitX);
            _timer = Core.schedule(AttackPushDuration, entity, t =>
            {
                entity.forceMovement(Vector2.Zero);
                fsm.resetStackTo(new StandState());
            });
            entity.velocityMultiplier = VelocityMultiplier;
        }
        
        public override void end()
        {
            entity.SetDashTrail(false);
            entity.velocityMultiplier = 1.0f;
            _timer?.stop();
            entity.forceMovement(Vector2.Zero);
            _input.IsLocked = false;
            entity.ReduceDamageScale();
        }
    }

    public class RescueHostageState : PlayerState
    {
        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(PlayerComponent.Animations.Sword1);
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new StandState());
            }
        }

        public override void end()
        {
            _input.IsLocked = false;
        }
    }

    public class BaseLoopedState : PlayerState
    {
        public PlayerComponent.Animations Animation;

        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(Animation);
        }

        public override void update()
        {
            base.update();
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new StandState());
            }
        }

        public override void end()
        {
            _input.IsLocked = false;
        }
    }

    public class BaseAttackComboState : PlayerState
    {
        private bool _changeToAttack;
        private ITimer _timer;
        private bool _playedSe;

        protected int FrameToPlaySe = -1;

        public PlayerComponent.Animations Animation;
        public PlayerState NextComboState;
        public bool IsFinal;
        public float VerticalKnockback;
        public float HorizontalKnockback;
        public float AttackPushDuration = 0.05f;
        public float AttackPushMultiplier = 1.0f;
        public bool AttackPushLockDirection;
        public float VelocityMultiplier = 1.0f;
        public float ScreenShakeMagnitude;
        public float FreezeScreenDuration = 0.01f;
        public float FreezeScreenScale = 0.2f;

        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(Animation);
            entity.platformerObject.lockVerticalMovement = true;

            var lockDirection = AttackPushLockDirection
                ? (Direction?)(entity.GetIntDirection() == 1 ? Direction.Right : Direction.Left)
                : null;
            entity.forceMovement(entity.GetIntDirection() * AttackPushMultiplier * Vector2.UnitX, lockDirection);
            _timer = Core.schedule(AttackPushDuration, entity, t =>
            {
                entity.forceMovement(Vector2.Zero);
            });
            entity.velocityMultiplier = VelocityMultiplier;
        }

        public override void update()
        {
            base.update();
            if (entity.IsChoosingWeapon()) return;

            if (FrameToPlaySe >= 0 && !_playedSe && entity.sprite.CurrentFrame == FrameToPlaySe)
            {
                _playedSe = true;
                AudioManager.attackSounds.play();
            }

            if (entity.CanLinkCombo(Animation) && _input.UpButton.isPressed)
            {
                fsm.resetStackTo(new JumpingState(true));
                return;
            }
            if (!IsFinal && entity.CanLinkCombo(Animation) && _input.AttackButton.isPressed)
            {
                _changeToAttack = true;
                fsm.changeState(NextComboState);
                return;
            }
            if (!_changeToAttack && entity.sprite.Looped)
            {
                fsm.resetStackTo(new StandState());
            }
        }

        public override void end()
        {
            entity.velocityMultiplier = 1.0f;
            _timer?.stop();
            entity.forceMovement(Vector2.Zero);
            _input.IsLocked = false;
            entity.platformerObject.lockVerticalMovement = false;
            entity.ReduceDamageScale();
        }
    }

    #region Fist States

    public class FistAttack1 : BaseAttackComboState
    {
        public FistAttack1()
        {
            Animation = PlayerComponent.Animations.Fist1;
            FrameToPlaySe = 1;
            NextComboState = new FistAttack2();
        }
    }

    public class FistAttack2 : BaseAttackComboState
    {
        public FistAttack2()
        {
            Animation = PlayerComponent.Animations.Fist2;
            FrameToPlaySe = 2;
            NextComboState = new FistAttack3();
        }
    }

    public class FistAttack3 : BaseAttackComboState
    {
        public FistAttack3()
        {
            Animation = PlayerComponent.Animations.Fist3;
            FrameToPlaySe = 2;
            VelocityMultiplier = 2;
            HorizontalKnockback = 0.04f;
            AttackPushDuration = 0.1f;
            ScreenShakeMagnitude = 1.5f;
            FreezeScreenDuration = 0.008f;
            FreezeScreenScale = 0.19f;
            IsFinal = true;
        }
    }
    
    #endregion

    #region Sword States

    public class SwordAttack1 : BaseAttackComboState
    {
        public SwordAttack1()
        {
            Animation = PlayerComponent.Animations.Sword1;
            FrameToPlaySe = 1;
            NextComboState = new SwordAttack2();
        }
    }

    public class SwordAttack2 : BaseAttackComboState
    {
        public SwordAttack2()
        {
            Animation = PlayerComponent.Animations.Sword2;
            FrameToPlaySe = 1;
            NextComboState = new SwordAttack3();
        }
    }

    public class SwordAttack3 : BaseAttackComboState
    {
        public SwordAttack3()
        {
            Animation = PlayerComponent.Animations.Sword3;
            FrameToPlaySe = 3;
            VerticalKnockback = 0.03f;
            ScreenShakeMagnitude = 1.5f;
            FreezeScreenDuration = 0.008f;
            FreezeScreenScale = 0.13f;
            IsFinal = true;
        }
    }

    #endregion

    #region Quarterstaff States

    public class QuarterstaffAttack1 : BaseAttackComboState
    {
        public QuarterstaffAttack1()
        {
            Animation = PlayerComponent.Animations.Quarterstaff1;
            FrameToPlaySe = 1;
            NextComboState = new QuarterstaffAttack2();
        }
    }

    public class QuarterstaffAttack2 : BaseAttackComboState
    {
        public QuarterstaffAttack2()
        {
            Animation = PlayerComponent.Animations.Quarterstaff2;
            FrameToPlaySe = 1;
            NextComboState = new QuarterstaffAttack3();
        }
    }

    public class QuarterstaffAttack3 : BaseAttackComboState
    {
        public QuarterstaffAttack3()
        {
            Animation = PlayerComponent.Animations.Quarterstaff3;
            FrameToPlaySe = 3;
            HorizontalKnockback = 0.09f;
            ScreenShakeMagnitude = 1.5f;
            FreezeScreenDuration = 0.008f;
            FreezeScreenScale = 0.13f;
            IsFinal = true;
        }
    }

    #endregion

    #region Pistol States

    public class PistolAttack1 : BaseAttackComboState
    {
        private bool _playedSe;

        public PistolAttack1()
        {
            Animation = PlayerComponent.Animations.Pistol1;
            NextComboState = new PistolAttack2();
            AttackPushMultiplier = -1.0f;
            AttackPushLockDirection = true;
        }

        public override void update()
        {
            base.update();
            if (!_playedSe && entity.sprite.CurrentFrame == 1)
            {
                _playedSe = true;
                AudioManager.gun.Play(0.7f);
            }
        }
    }

    public class PistolAttack2 : BaseAttackComboState
    {
        private bool _playedSe;

        public PistolAttack2()
        {
            Animation = PlayerComponent.Animations.Pistol2;
            NextComboState = new PistolAttack3();
            AttackPushMultiplier = -1.0f;
            AttackPushLockDirection = true;
        }

        public override void update()
        {
            base.update();
            if (!_playedSe && entity.sprite.CurrentFrame == 1)
            {
                _playedSe = true;
                AudioManager.gun.Play(0.7f);
            }
        }
    }

    public class PistolAttack3 : BaseAttackComboState
    {
        private bool _playedSe;

        public PistolAttack3()
        {
            Animation = PlayerComponent.Animations.Pistol3;
            AttackPushMultiplier = -1.0f;
            AttackPushLockDirection = true;
            IsFinal = true;
        }

        public override void update()
        {
            base.update();
            if (!_playedSe && entity.sprite.CurrentFrame == 1)
            {
                _playedSe = true;
                AudioManager.gun.Play(0.7f);
            }
        }
    }

    #endregion

    public class HitState : PlayerState
    {
        public override void begin()
        {
            entity.SetAnimation(PlayerComponent.Animations.Hit);
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new StandState());
            }
        }
    }

    public class DyingState : PlayerState
    {
        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(PlayerComponent.Animations.Dying);
        }
    }
}
