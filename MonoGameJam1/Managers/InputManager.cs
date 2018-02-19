using Microsoft.Xna.Framework.Input;
using Nez;

namespace MonoGameJam1.Managers
{
    public class InputManager : IUpdatableManager
    {
        private VirtualButton _interactionButton;
        public VirtualButton InteractionButton => _interactionButton;

        private VirtualButton _weaponSelectionButton;
        public VirtualButton WeaponSelectionButton => _weaponSelectionButton;

        private VirtualButton _attackButton;
        public VirtualButton AttackButton => _attackButton;

        private VirtualButton _jumpButton;
        public VirtualButton JumpButton => _jumpButton;

        private VirtualButton _upButton;
        public VirtualButton UpButton => _upButton;

        private VirtualButton _leftButton;
        public VirtualButton LeftButton => _leftButton;

        private VirtualButton _rightButton;
        public VirtualButton RightButton => _rightButton;

        private VirtualButton _downButton;
        public VirtualButton DownButton => _downButton;

        private VirtualIntegerAxis _movementAxis;
        public VirtualIntegerAxis MovementAxis => _movementAxis;

        private VirtualButton _selectButton;
        public VirtualButton SelectButton => _selectButton;

        // Blocks all the interaction stuff
        public bool IsBusy { get; set; }

        // Blocks all the input
        public bool IsLocked { get; set; }

        public InputManager()
        {
            _interactionButton = new VirtualButton();
            _interactionButton
                .addKeyboardKey(Keys.X)
                .addKeyboardKey(Keys.Z)
                .addKeyboardKey(Keys.C)
                .addKeyboardKey(Keys.Space)
                .addKeyboardKey(Keys.Enter)
                .addGamePadButton(0, Buttons.A)
                .addGamePadButton(0, Buttons.X);

            _weaponSelectionButton = new VirtualButton();
            _weaponSelectionButton
                .addKeyboardKey(Keys.X)
                .addGamePadButton(0, Buttons.B);

            _attackButton = new VirtualButton();
            _attackButton
                .addKeyboardKey(Keys.Z)
                .addGamePadButton(0, Buttons.X);

            _jumpButton = new VirtualButton();
            _jumpButton
                .addKeyboardKey(Keys.Up)
                .addGamePadButton(0, Buttons.A);    

            _upButton = new VirtualButton();
            _upButton
                .addKeyboardKey(Keys.Up)
                .addGamePadButton(0, Buttons.DPadUp);

            _leftButton = new VirtualButton();
            _leftButton
                .addKeyboardKey(Keys.Left)
                .addGamePadButton(0, Buttons.DPadLeft);

            _rightButton = new VirtualButton();
            _rightButton
                .addKeyboardKey(Keys.Right)
                .addGamePadButton(0, Buttons.DPadRight);

            _downButton = new VirtualButton();
            _downButton
                .addKeyboardKey(Keys.Down)
                .addGamePadButton(0, Buttons.DPadDown);

            _movementAxis = new VirtualIntegerAxis();
            _movementAxis
                .addKeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right)
                .addGamePadLeftStickX()
                .addGamePadDPadLeftRight();

            _selectButton = new VirtualButton();
            _selectButton
                .addKeyboardKey(Keys.Enter)
                .addGamePadButton(0, Buttons.A)
                .addGamePadButton(0, Buttons.Start);
        }

        public bool isMovementAvailable()
        {
            return !IsBusy && !IsLocked;
        }

        public void update()
        { }
    }
}
