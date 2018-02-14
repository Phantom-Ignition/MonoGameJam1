using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGameJam1.Components.Map;
using MonoGameJam1.Components.Player;
using MonoGameJam1.Scenes;
using Nez;
using Microsoft.Xna.Framework;

namespace MonoGameJam1.Systems
{
    class BattleAreasSystem : EntityProcessingSystem
    {
        private Entity _blockEntity;
        private Entity _playerEntity;
        private bool _battleHappening;

        public BattleAreasSystem(Entity playerEntity) : base(new Matcher().one(typeof(BattleAreaComponent)))
        {
            _playerEntity = playerEntity;

            _blockEntity = playerEntity.scene.createEntity();
            _blockEntity.addComponent(new BoxCollider(0, 0, 16, Scene.virtualSize.Y));
            _blockEntity.enabled = false;
        }

        public override void process(Entity entity)
        {
            if (_battleHappening) return;

            var collider = entity.getComponent<BoxCollider>();

            var collisionRect = Physics.overlapRectangle(collider.bounds, 1 << SceneMap.PLAYER_LAYER);
            if (collisionRect != null)
            {
                _battleHappening = true;
                _blockEntity.enabled = true;
                _blockEntity.setPosition(collider.localOffset + collider.width * Vector2.UnitX);

                Console.WriteLine("gotcha!");
            }
        }

        protected override void lateProcess(List<Entity> entities)
        {
            base.lateProcess(entities);

            // Check if the player is colliding with the block entity
            if (!_battleHappening) return;

            CollisionResult collisionResult;
            if (_blockEntity.getComponent<BoxCollider>()
                .collidesWith(_playerEntity.getComponent<BoxCollider>(), out collisionResult))
            {
                _playerEntity.transform.position += collisionResult.minimumTranslationVector;
            }
        }
    }
}
