using MonoGameJam1.Components.Sprites;
using Nez;

namespace MonoGameJam1.Components.Map
{
    public class SpriteEffectComponent : Component, IUpdatable
    {
        private AnimatedSprite _sprite;
        private bool _calledDestroy;

        public override void onAddedToEntity()
        {
            _sprite = entity.getComponent<AnimatedSprite>();
        }

        public void update()
        {
            if (_calledDestroy || !_sprite.Looped) return;
            _calledDestroy = true;
            entity.destroy();
        }
    }
}
