using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Nez;

namespace MonoGameJam1.Managers
{
    public static class AudioManager
    {
        public static void loadAllSounds()
        {
            // no songs yet
        }

        private static SoundEffect load(string name)
        {
            return Core.content.Load<SoundEffect>(name);
        }

        private static Song loadBgm(string name)
        {
            return Core.content.Load<Song>(name);
        }
    }
}
