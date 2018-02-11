using Microsoft.Xna.Framework;
using Nez;

namespace MonoGameJam1.Components
{
    class InteractionCollider : BoxCollider
    {
        public InteractionCollider(float x, float y, float width, float height) : base(x, y, width, height)
        { }

        public override void debugRender(Graphics graphics)
        {
            var color = Debug.Colors.colliderEdge;
            Debug.Colors.colliderEdge = Color.DarkGreen;
            base.debugRender(graphics);
            Debug.Colors.colliderEdge = color;
        }
    }
}
