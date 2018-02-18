using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Components.Player;
using MonoGameJam1.Managers;
using Nez;
using Nez.Textures;
using System.Collections.Generic;
using Nez.Tweens;

namespace MonoGameJam1.Components.Map
{
    public class MapHudComponent : RenderableComponent, IUpdatable
    {
        public override float width => Scene.virtualSize.X;
        public override float height => Scene.virtualSize.Y; 

        private PlayerComponent _playerComponent;

        private Rectangle _hpBounds;
        private int _hpFillOffset;
        private Texture2D _hpBackTexture;
        private Texture2D _hpFillTexture;

        private Vector2 _iconPosition;
        private Dictionary<Weapon, Subtexture> _iconsSubtextures;

        private Vector2 _damageScalePosition;
        private Texture2D _numbersTexture;

        // Managers
        private ScoreManager _scoreManager;
        private SystemManager _systemManager;

        // GO!
        private Vector2 _goPosition;
        private float _goExtraPosition;
        private Texture2D _goTexture;
        private int _goIndex;
        private float _goAnimationTick;
        private float _goAlpha;

        // Instruction
        private Texture2D _guide1Texture;
        private Texture2D _guide2Texture;

        public override void onAddedToEntity()
        {
            _scoreManager = Core.getGlobalManager<ScoreManager>();
            _systemManager = Core.getGlobalManager<SystemManager>();

            var playerEntity = entity.scene.findEntity("player");
            _playerComponent = playerEntity.getComponent<PlayerComponent>();

            // Hp
            _hpBackTexture = entity.scene.content.Load<Texture2D>(Content.Misc.hphud);
            _hpFillTexture = entity.scene.content.Load<Texture2D>(Content.Misc.hpbar);
            _hpBounds = new Rectangle(20, 30, _hpFillTexture.Width, _hpFillTexture.Height);
            _hpFillOffset = 14;

            // Icons
            _iconPosition = new Vector2(33, 23);
            var iconsTexture = entity.scene.content.Load<Texture2D>(Content.Misc.icons);
            _iconsSubtextures = new Dictionary<Weapon, Subtexture>
            {
                { Weapon.Fist, new Subtexture(iconsTexture, new Rectangle(0, 0, 20, 20)) },
                { Weapon.Sword, new Subtexture(iconsTexture, new Rectangle(20, 0, 20, 20)) },
                { Weapon.Pistol, new Subtexture(iconsTexture, new Rectangle(40, 0, 20, 20)) },
                { Weapon.Quarterstaff, new Subtexture(iconsTexture, new Rectangle(60, 0, 20, 20)) },
            };

            // Damage Scale
            _damageScalePosition = new Vector2(50, 31);
            _numbersTexture = entity.scene.content.Load<Texture2D>(Content.Misc.numbers);

            // Go!
            _goPosition = new Vector2(335, 40);
            _goTexture = entity.scene.content.Load<Texture2D>(Content.Misc.go);
            _goAlpha = 0.0f;

            // Instructions
            _guide1Texture = entity.scene.content.Load<Texture2D>(Content.Misc.guide1);
            _guide2Texture = entity.scene.content.Load<Texture2D>(Content.Misc.guide2);
        }

        public override void render(Graphics graphics, Camera camera)
        {
            // Icon
            var iconTexture = _iconsSubtextures[_playerComponent.CurrentWeapon];
            graphics.batcher.draw(iconTexture, entity.position + _iconPosition, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);

            var hudPosition = entity.position + _hpBounds.Location.ToVector2();
            graphics.batcher.draw(_hpBackTexture, hudPosition);
            graphics.batcher.draw(_hpFillTexture,
                new Rectangle((int)hudPosition.X + _hpFillOffset, (int)hudPosition.Y, (int)(_hpBounds.Width * hpFillWidth()),
                    _hpBounds.Height));

            // Damage scale
            var damageScalePosition = entity.position + _damageScalePosition;
            var damageScale = $"{_playerComponent.CurrentDamageScale() * 100}";

            for (var i = 0; i < damageScale.Length; i++)
            {
                var pos = damageScalePosition + 6 * i * Vector2.UnitX;
                var drect = new Rectangle((int)pos.X, (int)pos.Y, 6, 6);
                var srect = new Rectangle(int.Parse(damageScale[i].ToString()) * 6, 0, 6, 6);
                graphics.batcher.draw(_numbersTexture, drect, srect, Color.White);
            }
            graphics.batcher.draw(_numbersTexture,
                new Rectangle((int) damageScalePosition.X + damageScale.Length * 6, (int) damageScalePosition.Y, 6, 6),
                new Rectangle(60, 0, 6, 6), Color.White);

            // Score
            var scorePosition = entity.position + new Vector2(20, 55);
            const string score = "Score: ";
            var scoreNum = $"{_scoreManager.Score}";
            graphics.batcher.drawString(GameMain.smallBitmapFont, score, scorePosition, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            var pScorePosition = scorePosition + GameMain.smallBitmapFont.measureString(score).X * Vector2.UnitX;
            graphics.batcher.drawString(GameMain.smallBitmapFont, scoreNum, pScorePosition, Color.White, 0.0f, Vector2.Zero, _scoreManager.ScaleMultiplier, SpriteEffects.None, 1.0f);

            // Instructions
            if (_systemManager.getSwitch("guide1"))
                graphics.batcher.draw(_guide1Texture, entity.position, Color.White);
            if (_systemManager.getSwitch("guide2"))
                graphics.batcher.draw(_guide2Texture, entity.position, Color.White);

            // Go
            if (_goAlpha > 0.0f)
            {
                var pos = entity.position + _goPosition;
                var drect = new Rectangle((int)(pos.X + _goExtraPosition), (int)pos.Y, 72, _goTexture.Height);
                var srect = new Rectangle(72 * _goIndex, 0, 72, 20);
                graphics.batcher.draw(_goTexture, drect, srect, Color.White * _goAlpha);
            }
        }

        private float hpFillWidth()
        {
            var battler = _playerComponent.getComponent<BattleComponent>();
            return battler?.HP / battler?.MaxHP ?? 0;
        }

        public void showGo()
        {
            _goAlpha = 0.0f;
            _goExtraPosition = 0.0f;
            this.tween("_goAlpha", 1.0f, 0.3f)
                .setEaseType(EaseType.CubicIn)
                .start();
        }

        public void hideGo()
        {
            this.tween("_goExtraPosition", 100.0f, 0.5f)
                .setEaseType(EaseType.CubicIn)
                .setCompletionHandler((value) => { _goAlpha = 0f; })
                .start();
            this.tween("_goAlpha", 0.0f, 0.5f)
                .setEaseType(EaseType.CubicIn)
                .start();
        }

        public void update()
        {
            if (_goAlpha > 0.0f)
            {
                _goAnimationTick -= Time.deltaTime;
                if (_goAnimationTick <= 0.0f)
                {
                    _goAnimationTick = 0.1f;
                    _goIndex++;
                    if (_goIndex > 3)
                        _goIndex = 0;
                }
            }
        }
    }
}
