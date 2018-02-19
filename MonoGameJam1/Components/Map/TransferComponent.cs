using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace MonoGameJam1.Components.Map
{
    public enum TransferDirection
    {
        Left,
        Right
    }

    public class TransferComponent : Component
    {
        public TransferDirection direction;
        public int destinyId;

        private readonly TiledObject _tiledObject;

        public TransferComponent(TiledObject tiledObject)
        {
            _tiledObject = tiledObject;
        }

        public override void onAddedToEntity()
        {
            destinyId = int.Parse(_tiledObject.properties["destinyId"]);

            direction = _tiledObject.properties["direction"] == "left" ? TransferDirection.Left : TransferDirection.Right;
            if (direction == TransferDirection.Left)
            {
                transform.position = new Vector2(_tiledObject.position.X - 5, _tiledObject.position.Y);
            }
            else
            {
                transform.position = new Vector2(_tiledObject.position.X + _tiledObject.width + 5, _tiledObject.position.Y);
            }

            entity.addComponent(new BoxCollider(0, 0, 1, _tiledObject.height));
        }
    }
}
