using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nez;

namespace MonoGameJam1.Scenes
{
    class SceneCredits : Scene
    {
        public override void initialize()
        {
            base.initialize();

            addRenderer(new DefaultRenderer());
            clearColor = new Color(35, 35, 35);
        }
    }
}
