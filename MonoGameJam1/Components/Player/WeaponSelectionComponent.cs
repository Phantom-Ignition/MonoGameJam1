using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Managers;
using Nez;
using Nez.Textures;

namespace MonoGameJam1.Components.Player
{
    class WeaponSelectionComponent : RenderableComponent, IUpdatable
    {
        public override float width => _texture?.Width ?? 1;
        public override float height => _texture?.Height ?? 1;

        private Texture2D _texture;
        private bool _isOpen;

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
            _isOpen = true;
            _inputManager.IsBusy = true;
            Time.timeScale = 0f;
        }

        public void Close()
        {
            _isOpen = false;
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
            if (!_isOpen) return;

            var x = _collider.bounds.x - (width - _collider.width) / 2;
            var y = _collider.bounds.y - (height - _collider.height) / 2;

            graphics.batcher.draw(_texture, new Vector2(x, y));
        }

        public void update()
        {
            if (_isOpen)
            {
                if (_inputManager.LeftButton.isPressed)
                    PickWeapon(Weapon.Fist);
                if (_inputManager.UpButton.isPressed)
                    PickWeapon(Weapon.Sword);
                if (_inputManager.RightButton.isPressed)
                    PickWeapon(Weapon.Quarterstaff);
                if (_inputManager.DownButton.isPressed)
                    PickWeapon(Weapon.Pistol);
            }
        }
    }
}
