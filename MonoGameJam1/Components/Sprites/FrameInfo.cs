using System.Collections.Generic;
using MonoGameJam1.Components.Colliders;
using Nez.Textures;

namespace MonoGameJam1.Components.Sprites
{
    public class FrameInfo
    {
        public Subtexture Subtexture { get; }
        public List<AttackCollider> AttackColliders { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public FrameInfo(Subtexture subtexture, int offsetX, int offsetY)
        {
            Subtexture = subtexture;
            AttackColliders = new List<AttackCollider>();
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}
