using Microsoft.Xna.Framework;
using Nez;

namespace MonoGameJam1.Components.Colliders
{
    public class AreaOfSightCollider : BoxCollider
    {
        private Vector2 _originalLocalOffset;

        public float X => _originalLocalOffset.X;
        public float Y => _originalLocalOffset.Y;

        public AreaOfSightCollider(float x, float y, float width, float height) : base(x, y, width, height)
        {
            _originalLocalOffset = new Vector2(_localOffset.X, _localOffset.Y);
        }

        public void ApplyOffset(float x, float y)
        {
            var off = new Vector2(_originalLocalOffset.X + x, _originalLocalOffset.Y + y);
            setLocalOffset(off);
        }

        public override void debugRender(Graphics graphics)
        {
            var color = Debug.Colors.colliderEdge;
            Debug.Colors.colliderEdge = Color.DarkGreen;
            base.debugRender(graphics);
            Debug.Colors.colliderEdge = color;
        }
    }
}
