using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Components.Player;
using Nez;

namespace MonoGameJam1.Components.Map
{
    public class MapHudComponent : RenderableComponent
    {
        public override float width => Scene.virtualSize.X;
        public override float height => Scene.virtualSize.Y; 

        private PlayerComponent _playerComponent;

        private Rectangle _hpBounds;
        private int _hpFillOffset;
        private Texture2D _hpBackTexture;
        private Texture2D _hpFillTexture;

        public override void onAddedToEntity()
        {
            var playerEntity = entity.scene.findEntity("player");
            _playerComponent = playerEntity.getComponent<PlayerComponent>();

            _hpBackTexture = entity.scene.content.Load<Texture2D>(Content.Misc.hphud);
            _hpFillTexture = entity.scene.content.Load<Texture2D>(Content.Misc.hpbar);
            _hpBounds = new Rectangle(20, 30, _hpFillTexture.Width, _hpFillTexture.Height);
            _hpFillOffset = 14;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            var damageScalePosition = entity.position + new Vector2(20, 60);
            var damageScale = $"{_playerComponent.CurrentDamageScale() * 100}%";
            graphics.batcher.drawString(GameMain.bigBitmapFont, damageScale, damageScalePosition, Color.White);

            var hudPosition = entity.position + _hpBounds.Location.ToVector2();
            graphics.batcher.draw(_hpBackTexture, hudPosition);
            graphics.batcher.draw(_hpFillTexture,
                new Rectangle((int) hudPosition.X + _hpFillOffset, (int) hudPosition.Y, (int) (_hpBounds.Width * hpFillWidth()),
                    _hpBounds.Height));
        }

        private float hpFillWidth()
        {
            var battler = _playerComponent.getComponent<BattleComponent>();
            return battler.HP / battler.MaxHP;
        }
    }
}
