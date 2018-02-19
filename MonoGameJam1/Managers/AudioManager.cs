using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Nez;
using Nez.Audio;

namespace MonoGameJam1.Managers
{
    public static class AudioManager
    {
        public static AudioSource attackSounds;
        public static SoundEffect cancel;
        public static SoundEffect footstep;
        public static SoundEffect gun;
        public static SoundEffect hit;
        public static SoundEffect hitPlayer;
        public static SoundEffect jump;
        public static SoundEffect select;
        public static SoundEffect enemyDeath;
        public static SoundEffect click;
        public static SoundEffect ambience;

        public static void loadAllSounds()
        {
            attackSounds = new AudioSource();
            attackSounds.addSoundEffect(load(Content.Audios.Se.wosh1));
            attackSounds.addSoundEffect(load(Content.Audios.Se.wosh2));
            attackSounds.addSoundEffect(load(Content.Audios.Se.wosh3));
            attackSounds.addSoundEffect(load(Content.Audios.Se.wosh4));
            attackSounds.addSoundEffect(load(Content.Audios.Se.wosh5));
            attackSounds.setPitchRange(-0.5f, 0.5f);
            cancel = load(Content.Audios.Se.cancel);
            footstep = load(Content.Audios.Se.footstep);
            gun = load(Content.Audios.Se.gun);
            hit = load(Content.Audios.Se.hit);
            hitPlayer = load(Content.Audios.Se.hitPlayer);
            jump = load(Content.Audios.Se.jump);
            select = load(Content.Audios.Se.select);
            enemyDeath = load(Content.Audios.Se.enemyDeath);
            click = load(Content.Audios.Se.click);
            ambience = load(Content.Audios.Se.ambience);
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
