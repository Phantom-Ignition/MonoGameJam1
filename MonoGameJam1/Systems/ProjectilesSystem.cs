using System;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Scenes;
using Nez;
using Nez.ECS.Components.Physics.Colliders;

namespace MonoGameJam1.Systems
{
    public class ProjectilesSystem : EntityProcessingSystem
    {
        private readonly BattleComponent _playerBattler;
        private readonly BoxCollider _playerCollider;

        public ProjectilesSystem(Entity player) : base(new Matcher().one(typeof(ProjectileComponent)))
        {
            _playerBattler = player.getComponent<BattleComponent>();
            _playerCollider = player.getComponent<BoxCollider>();
        }

        public override void process(Entity entity)
        {
            var projectileComponent = entity.getComponent<ProjectileComponent>();
            
            var lastPosition = entity.position;
            projectileComponent.update();
            var newPosition = entity.position;
            
            var linecast = Physics.linecast(lastPosition, newPosition, 1 << SceneMap.PLAYER_LAYER);
            if (linecast.collider != null)
            {
                if (_playerBattler.onHit(linecast.normal * -1))
                {
                    entity.destroy();
                }
                return;
            }
            
            CollisionResult collisionResult;
            var collider = entity.getComponent<Collider>();

            // shots vs map
            if (collider.collidesWithAnyOfType<MapBoxCollider>(out collisionResult))
            {
                Console.WriteLine("hey");
                entity.destroy();
            }

            // shots vs player
            if (_playerBattler.isOnImmunity() || _playerCollider == null) return;
            if (collider.collidesWith(_playerCollider, out collisionResult))
            {
                if (_playerBattler.onHit(collisionResult))
                    entity.destroy();
            }
        }
    }
}
