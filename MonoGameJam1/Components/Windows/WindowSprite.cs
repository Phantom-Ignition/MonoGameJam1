using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace MonoGameJam1.Components.Windows
{
    class WindowSprite : RenderableComponent
    {
        //--------------------------------------------------
        // Width & Height

        public override float width { get { return _size.X; } }
        public override float height { get { return _size.Y; } }
        
        //--------------------------------------------------
        // Windowskin

        private Windowskin _windowskin;
        public Windowskin Windowskin => _windowskin;

        //--------------------------------------------------
        // Is Visible

        private bool _windowIsVisible;

        //--------------------------------------------------
        // Origin

        private Vector2 _originNormalized;
        Vector2 OriginNormalized
        {
            get { return _originNormalized; }
            set
            {
                _origin = new Vector2(value.X * _size.X, value.Y * _size.Y);
                _originNormalized = value;
            }
        }

        private Vector2 _origin;

        //--------------------------------------------------
        // Size

        private Vector2 _size;
        public Vector2 Size
        {
            get { return _size; }
            set
            {
                _origin = new Vector2(OriginNormalized.X * value.X, OriginNormalized.Y * value.Y);
                _size = value;

            }
        }

        public WindowSprite(Texture2D texture)
        {
            var edgeSize = new Vector2(4, 4);
            var borderSize = new Vector2(4, 4);
            _windowskin = new Windowskin(texture, edgeSize, borderSize);
        }

        public void setVisible(bool visible)
        {
            _windowIsVisible = visible;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            if (!_windowIsVisible) return;

            var edgeSX = _windowskin.EdgeSize.X;
            var edgeSY = _windowskin.EdgeSize.Y;

            var ws = new Vector2((int)((_size.X - edgeSX * 2) / edgeSX), 0);
            var hs = new Vector2(0, (int)((_size.Y - edgeSY * 2) / edgeSY));
            
            var edgeX = edgeSX * Vector2.UnitX;
            var edgeY = edgeSY * Vector2.UnitY;
            var pos = entity.transform.position + _localOffset;

            graphics.batcher.draw(_windowskin.Subtextures[0], pos, color, entity.transform.rotation, _origin, entity.transform.scale, SpriteEffects.None, layerDepth);
            graphics.batcher.draw(_windowskin.Subtextures[1], pos + edgeX, color, entity.transform.rotation, _origin, entity.transform.scale + ws, SpriteEffects.None, layerDepth);
            graphics.batcher.draw(_windowskin.Subtextures[2], pos + edgeX * 2 + ws * edgeSX, color, entity.transform.rotation, _origin, entity.transform.scale, SpriteEffects.None, layerDepth);

            graphics.batcher.draw(_windowskin.Subtextures[3], pos + edgeY, color, entity.transform.rotation, _origin, entity.transform.scale + hs, SpriteEffects.None, layerDepth);
            graphics.batcher.draw(_windowskin.Subtextures[4], pos + edgeY + edgeX, color, entity.transform.rotation, _origin, entity.transform.scale + hs + ws, SpriteEffects.None, layerDepth);
            graphics.batcher.draw(_windowskin.Subtextures[5], pos + edgeY + edgeX * 2 + ws * edgeSX, color, entity.transform.rotation, _origin, entity.transform.scale + hs, SpriteEffects.None, layerDepth);

            graphics.batcher.draw(_windowskin.Subtextures[6], pos + edgeY * 2 + hs * edgeSY, color, entity.transform.rotation, _origin, entity.transform.scale, SpriteEffects.None, layerDepth);
            graphics.batcher.draw(_windowskin.Subtextures[7], pos + edgeY * 2 + hs * edgeSY + edgeX, color, entity.transform.rotation, _origin, entity.transform.scale + ws, SpriteEffects.None, layerDepth);
            graphics.batcher.draw(_windowskin.Subtextures[8], pos + edgeY * 2 + hs * edgeSY + edgeX * 2 + ws * edgeSX, color, entity.transform.rotation, _origin, entity.transform.scale, SpriteEffects.None, layerDepth);
        }
    }
}

