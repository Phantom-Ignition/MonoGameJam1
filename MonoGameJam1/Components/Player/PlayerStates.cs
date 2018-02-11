using MonoGameJam1.FSM;
using MonoGameJam1.Managers;
using Nez;

namespace MonoGameJam1.Components.Player
{
    public class PlayerState : State<PlayerState, PlayerComponent>
    {
        protected InputManager _input => Core.getGlobalManager<InputManager>();

        public override void begin() { }

        public override void end() { }

        public void handleInput()
        {
            if (isMovementAvailable())
            {
                if (entity.isOnGround() && isMovementAvailable() && _input.JumpButton.isPressed)
                {
                    fsm.resetStackTo(new JumpingState(true));
                }
                if (_input.AttackButton.isPressed)
                {
                    switch (entity.CurrentWeapon)
                    {
                        case Weapon.Fist:
                            fsm.pushState(new FistAttack1());
                            break;
                        case Weapon.Sword:
                            fsm.pushState(new SwordAttack1());
                            break;
                    }
                }
                if (_input.WeaponSelectionButton.isPressed)
                {
                    entity.OpenWeaponSelection();
                }
            }
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
            }
        }

        public override void update()
        {
            base.update();

            if (entity.isOnGround())
            {
                fsm.resetStackTo(new StandState());
            }
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

        public PlayerComponent.Animations Animation;
        public PlayerState NextState;
        public bool IsFinal;
        public float VerticalKnockback;

        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(Animation);
        }

        public override void update()
        {
            base.update();
            if (entity.sprite.isOnCombableFrame() && _input.AttackButton.isPressed)
            {
                _changeToAttack = true;
            }
            if (entity.sprite.Looped)
            {
                if (!IsFinal && _changeToAttack)
                {
                    fsm.changeState(NextState);
                }
                else
                {
                    fsm.resetStackTo(new StandState());
                }
            }
        }

        public override void end()
        {
            _input.IsLocked = false;
        }
    }

    #region Fist States

    public class FistAttack1 : BaseAttackComboState
    {
        public FistAttack1()
        {
            Animation = PlayerComponent.Animations.Fist1;
            NextState = new FistAttack2();
        }
    }

    public class FistAttack2 : BaseAttackComboState
    {
        public FistAttack2()
        {
            Animation = PlayerComponent.Animations.Fist2;
            NextState = new FistAttack3();
        }
    }

    public class FistAttack3 : BaseAttackComboState
    {
        public FistAttack3()
        {
            Animation = PlayerComponent.Animations.Fist3;
            NextState = new FistAttack4();
        }
    }

    public class FistAttack4 : BaseAttackComboState
    {
        public FistAttack4()
        {
            Animation = PlayerComponent.Animations.Fist4;
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
            VerticalKnockback = 0.03f;
            NextState = new SwordAttack2();
        }
    }

    public class SwordAttack2 : BaseAttackComboState
    {
        public SwordAttack2()
        {
            Animation = PlayerComponent.Animations.Sword2;
            NextState = new SwordAttack3();
        }
    }

    public class SwordAttack3 : BaseAttackComboState
    {
        public SwordAttack3()
        {
            Animation = PlayerComponent.Animations.Sword3;
            NextState = new SwordAttack4();
        }
    }

    public class SwordAttack4 : BaseAttackComboState
    {
        public SwordAttack4()
        {
            Animation = PlayerComponent.Animations.Sword4;
            VerticalKnockback = 0.03f;
            IsFinal = true;
        }
    }

    #endregion

    public class DyingState : PlayerState
    {
        public override void begin()
        {
            _input.IsLocked = true;
            entity.SetAnimation(PlayerComponent.Animations.Dying);
        }
    }
}
