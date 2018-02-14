using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGameJam1.Components.Map
{
    class BattleAreaComponent
    {
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public Vector2 Size { get; set; }
        public bool Activated { get; set; }
    }
}
