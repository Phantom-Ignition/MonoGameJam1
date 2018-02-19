using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Managers;
using Nez;
using Nez.Sprites;

namespace MonoGameJam1.Scenes
{
    class SceneCredits : Scene
    {
        private bool _startedTransition;

        public override void initialize()
        {
            base.initialize();
            
            addRenderer(new DefaultRenderer());
            clearColor = new Color(35, 35, 35);

            var sprite = createEntity("image")
                .addComponent(new Sprite(content.Load<Texture2D>(Content.System.credits)));
            sprite.originNormalized = Vector2.Zero;
        }

        public override void update()
        {
            base.update();

            if (!Core.isOnTransition() && !_startedTransition && Core.getGlobalManager<InputManager>().InteractionButton.isPressed)
            {
                _startedTransition = true;
                Core.startSceneTransition(new FadeTransition(() => new SceneTitle()));
            }
        }
    }
}
