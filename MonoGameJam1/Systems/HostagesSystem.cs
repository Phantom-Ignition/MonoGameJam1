using MonoGameJam1.Components;
using MonoGameJam1.Components.Map;
using MonoGameJam1.Components.Player;
using MonoGameJam1.Managers;
using Nez;

namespace MonoGameJam1.Systems
{
    public class HostagesSystem : EntityProcessingSystem
    {
        private readonly PlayerComponent _player;
        private readonly InputManager _inputManager;

        public HostagesSystem(PlayerComponent player) : base(new Matcher().one(typeof(HostageComponent)))
        {
            _player = player;
            _inputManager = Core.getGlobalManager<InputManager>();
        }

        public override void process(Entity entity)
        {
            if (_inputManager.AttackButton.isPressed)
            {
                CollisionResult collisionResult;
                if (entity.getComponent<Collider>().collidesWith(_player.getComponent<InteractionCollider>(), out collisionResult))
                {
                    var hostage = entity.getComponent<HostageComponent>();
                    if (!hostage.Rescued)
                    {
                        _player.rescueHostage();
                        entity.getComponent<HostageComponent>().getRescued();
                    }
                }
            }
        }
    }
}
