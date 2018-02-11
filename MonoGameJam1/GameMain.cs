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
        }
    }
}
