using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameJam1.Managers;
using MonoGameJam1.Scenes;
using Nez;
using Nez.BitmapFonts;

namespace MonoGameJam1
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class GameMain : Core
    {
        public static BitmapFont bigBitmapFont;
        public static BitmapFont smallBitmapFont;

        public GameMain() : base(width: 854, height: 480, isFullScreen: false, enableEntitySystems: true, windowTitle: "MonoGame Jam")
        {
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            IsFixedTimeStep = true;
            debugRenderEnabled = true;

            // Register Global Managers
            registerGlobalManager(new InputManager());
            registerGlobalManager(new SystemManager());
            registerGlobalManager(new ScoreManager());
        }

        protected override void LoadContent()
        {
            bigBitmapFont = content.Load<BitmapFont>(Nez.Content.Fonts.titleFont);
            smallBitmapFont = content.Load<BitmapFont>(Nez.Content.Fonts.smallFont);
            AudioManager.loadAllSounds();

            // MediaPlayer.Play(AudioManager.BGM);
            // MediaPlayer.Volume = 0.8f;
            // MediaPlayer.IsRepeating = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Scene.setDefaultDesignResolution(427, 240, Scene.SceneResolutionPolicy.FixedHeight);

            // PP Fix
            scene = Scene.createWithDefaultRenderer();
            base.Update(new GameTime());
            base.Draw(new GameTime());

            getGlobalManager<SystemManager>().setMapId(1);

            // Set first scene
            scene = new SceneMap();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Input.isKeyPressed(Keys.F1))
            {
                debugRenderEnabled = !debugRenderEnabled;
            }
        }
    }
}
