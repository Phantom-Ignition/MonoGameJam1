using Microsoft.Xna.Framework;
using Nez;

namespace MonoGameJam1.Components.Colliders
{
    public class AttackCollider : BoxCollider
    {
        private Vector2 _originalLocalOffset;

        public float X => _originalLocalOffset.X;
        public float Y => _originalLocalOffset.Y;

        public AttackCollider(float x, float y, float width, float height) : base(x, y, width, height)
        {
            _originalLocalOffset = new Vector2(_localOffset.X, _localOffset.Y);
        }

        public void ApplyOffset(float x, float y)
        {
            var off = new Vector2(_originalLocalOffset.X + x, _originalLocalOffset.Y + y);
            setLocalOffset(off);
        }

        public override void debugRender(Graphics graphics) { }

        public void debugRender(Graphics graphics, bool draw)
        {
            if (draw)
            {
                graphics.batcher.drawRect(bounds.x-1, bounds.y-1, bounds.width+1, bounds.height+1, Color.Orchid * 0.3f);
            }
        }
    }
}
