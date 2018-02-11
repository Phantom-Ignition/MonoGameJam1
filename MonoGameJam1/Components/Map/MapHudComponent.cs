using Microsoft.Xna.Framework;
using MonoGameJam1.Components.Player;
using Nez;

namespace MonoGameJam1.Components.Map
{
    class MapHudComponent : RenderableComponent
    {
        public override float width => Scene.virtualSize.X;
        public override float height => Scene.virtualSize.Y; 

        private PlayerComponent _playerComponent;

        public override void onAddedToEntity()
        {
            var playerEntity = entity.scene.findEntity("player");
            _playerComponent = playerEntity.getComponent<PlayerComponent>();
        }

        public override void render(Graphics graphics, Camera camera)
        {
            var damageScalePosition = entity.position + new Vector2(20, 30);
            var damageScale = $"{_playerComponent.CurrentDamageScale() * 100}%";
            graphics.batcher.drawString(GameMain.bigBitmapFont, damageScale, damageScalePosition, Color.White);
        }
    }
}
