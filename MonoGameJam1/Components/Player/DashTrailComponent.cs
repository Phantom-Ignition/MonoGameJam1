﻿using Microsoft.Xna.Framework;
using Nez.Sprites;

namespace MonoGameJam1.Components.Player
{
    public class DashTrailComponent : SpriteTrail
    {
        public DashTrailComponent()
        {
            minDistanceBetweenInstances = 7.0f;
            initialColor = Color.Violet * 0.8f;
            fadeDelay = 0.07f;
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
