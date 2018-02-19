using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nez;

namespace MonoGameJam1.Scenes
{
    public class SceneTitle: Scene
    {
        public override void initialize()
        {
            addRenderer(new DefaultRenderer());
            clearColor = Color.Coral;
        }
    }
}
