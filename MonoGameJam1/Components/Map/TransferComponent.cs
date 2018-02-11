using Microsoft.Xna.Framework;
using MonoGameJam1.Managers;
using Nez;
using Nez.Tiled;

namespace MonoGameJam1.Components.Map
{
    public enum TransferDirection
    {
        Left,
        Right
    }

    class TransferComponent : Component
    {
        public TransferDirection direction;
        public int destinyId;
        public Vector2 destinyPosition;

        private TiledObject _tiledObject;

        public TransferComponent(TiledObject tiledObject)
        {
            _tiledObject = tiledObject;
        }

        public override void onAddedToEntity()
        {
            var tiledMap = Core.getGlobalManager<SystemManager>().TiledMap;

            destinyId = int.Parse(_tiledObject.properties["destinyId"]);

            var destinyX = int.Parse(_tiledObject.properties["destinyX"]) * tiledMap.tileWidth;
            var destinyY = int.Parse(_tiledObject.properties["destinyY"]) * tiledMap.tileHeight;
            destinyPosition = new Vector2(destinyX, destinyY);

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
