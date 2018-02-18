using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Managers;
using Nez;

namespace MonoGameJam1.Components.Player
{
    public class WeaponSelectionComponent : RenderableComponent, IUpdatable
    {
        public override float width => _texture?.Width ?? 1;
        public override float height => _texture?.Height ?? 1;

        private Texture2D _texture;
        public bool IsOpen { get; private set; }
        private float _closeTick;
        private float _openInterval;

        private PlayerComponent _playerComponent;
        private BoxCollider _collider;
        private InputManager _inputManager;
        
        public override void onAddedToEntity()
        {
            _texture = entity.scene.content.Load<Texture2D>(Content.Misc.weaponCircle);
            _collider = entity.getComponent<BoxCollider>();
            _playerComponent = entity.getComponent<PlayerComponent>();

            _inputManager = Core.getGlobalManager<InputManager>();
        }

        public void Open()
        {
            if (_openInterval > 0) return;

            _openInterval = 0.1f;
            IsOpen = true;
            _inputManager.IsBusy = true;
            _closeTick = 0;
            Time.timeScale = 0f;
        }

        public void Close()
        {
            IsOpen = false;
            _inputManager.IsBusy = false;
            Time.timeScale = 1f;
        }

        private void PickWeapon(Weapon weapon)
        {
            _playerComponent.ChangeWeapon(weapon);
            Close();
        }

        public override void render(Graphics graphics, Camera camera)
        {
            if (!IsOpen) return;

            var x = _collider.bounds.x - (width - _collider.width) / 2;
            var y = _collider.bounds.y - (height - _collider.height) / 2;

            graphics.batcher.draw(_texture, new Vector2(x, y));
        }

        public void update()
        {
            if (_openInterval > 0)
                _openInterval -= Time.deltaTime;

            if (IsOpen)
            {
                _closeTick += Time.unscaledDeltaTime;
                if (_closeTick >= 0.1 && 
                    (_inputManager.WeaponSelectionButton.isPressed||_inputManager.AttackButton.isPressed))
                    Close();
                if (_inputManager.LeftButton.isPressed)
                    PickWeapon(Weapon.Fist);
                if (_inputManager.UpButton.isPressed)
                {
                    PickWeapon(Weapon.Sword);
                }
                if (_inputManager.RightButton.isPressed)
                    PickWeapon(Weapon.Quarterstaff);
                if (_inputManager.DownButton.isPressed)
                    PickWeapon(Weapon.Pistol);
            }
        }
    }
}
