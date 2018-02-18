using Microsoft.Xna.Framework;
using Nez.Sprites;

namespace MonoGameJam1.Components.Player
{
    public class DashTrailComponent : SpriteTrail
    {
        public DashTrailComponent()
        {
            minDistanceBetweenInstances = 9.0f;
            initialColor = Color.Violet * 0.8f;
            fadeDelay = 0f;
            fadeDuration = 0.35f;
        }

        public void SetSpawnEnabled(bool isEnabled)
        {
            if (isEnabled)
            {
                enableSpriteTrail();
            }
            else
            {
                disableSpriteTrail();
            }
        }
    }
}
