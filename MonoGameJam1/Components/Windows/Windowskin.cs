using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace MonoGameJam1.Components.Windows
{
    class Windowskin
    {
        //--------------------------------------------------
        // Texture

        private Texture2D _texture;

        //--------------------------------------------------
        // Sizes

        public Vector2 EdgeSize;
        public Vector2 BorderSize;
        
        //--------------------------------------------------
        // Subtextures

        private Subtexture[] _subtextures;
        public Subtexture[] Subtextures => _subtextures;

        //----------------------//------------------------//

        public Windowskin(Texture2D texture, Vector2 edgeSize, Vector2 borderSize)
        {
            _texture = texture;
            EdgeSize = edgeSize;
            BorderSize = borderSize;
            createSubtextures();
        }

        private void createSubtextures()
        {
            _subtextures = new Subtexture[9];
            var ert = new Rectangle(0, 0, (int)EdgeSize.X, (int)EdgeSize.Y);
            var brt = new Rectangle(0, 0, (int)BorderSize.X, (int)BorderSize.Y);
            var t = _texture;

            var st = new Subtexture[9];

            // First row
            var y = 0;
            _subtextures[0] = new Subtexture(t, ert);
            _subtextures[1] = new Subtexture(t, new Rectangle(ert.Width, y, brt.Width, brt.Height));
            _subtextures[2] = new Subtexture(t, new Rectangle(ert.Width + brt.Width, y, ert.Width, ert.Height));

            // Second row
            y += ert.Height;
            _subtextures[3] = new Subtexture(t, new Rectangle(0, y, brt.Width, brt.Height));
            _subtextures[4] = new Subtexture(t, new Rectangle(ert.Width, y, brt.Width, brt.Height));
            _subtextures[5] = new Subtexture(t, new Rectangle(ert.Width + brt.Height, y, brt.Width, brt.Height));

            // Third row
            y += brt.Width;
            _subtextures[6] = new Subtexture(t, new Rectangle(0, y, brt.Width, brt.Height));
            _subtextures[7] = new Subtexture(t, new Rectangle(ert.Width, y, brt.Width, brt.Height));
            _subtextures[8] = new Subtexture(t, new Rectangle(ert.Width + brt.Height, y, brt.Width, brt.Height));
        }
    }
}
