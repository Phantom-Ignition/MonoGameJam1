using System;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tiled;

namespace MonoGameJam1.Components
{
    public class PlatformerObject : Component, IUpdatable
    {
        //--------------------------------------------------
        // Physics

        public float moveSpeed = 1000;
        public float maxMoveSpeed = 150;
        public float gravity = 1200;
        public float jumpHeight = 17 * 3;
        public float wallGravity = 100;

        //--------------------------------------------------
        // Walljump

        public bool grabbingWall;
        public int grabbingWallSide;

        //--------------------------------------------------
        // Tiled Mover

        TiledMapMover _mover;
        public TiledMapMover mover => _mover;

        //--------------------------------------------------
        // Box Collider

        BoxCollider _boxCollider;
        
        //--------------------------------------------------
        // Velocity

        public Vector2 velocity;
        
        //--------------------------------------------------
        // Collision State

        public TiledMapMover.CollisionState collisionState = new TiledMapMover.CollisionState();

        //--------------------------------------------------
        // Map

        private TiledMap _tiledMap;

        //--------------------------------------------------
        // Delta time

        private Func<float> _getDeltaTimeFunc;

        //----------------------//------------------------//

        public PlatformerObject(TiledMap map)
        {
            _tiledMap = map;
            _getDeltaTimeFunc = defaultGetDeltaTime;
        }

        public void setGetDeltaTimeFunc(Func<float> func)
        {
            _getDeltaTimeFunc = func;
        }

        public void setMap(TiledMap map)
        {
            _tiledMap = map;
        }

        public override void onAddedToEntity()
        {
            _mover = this.getComponent<TiledMapMover>();
            _boxCollider = entity.getComponent<BoxCollider>();
        }

        public void update()
        {
            var deltaTime = _getDeltaTimeFunc?.Invoke() ?? 0.0f;

            // apply gravity
            velocity.Y += (grabbingWall && velocity.Y > 0 ? wallGravity : gravity) * deltaTime;
            velocity.Y = MathHelper.Clamp(velocity.Y, -maxMoveSpeed * 3, maxMoveSpeed * 3);

            // apply movement
            _mover.move(velocity * deltaTime, _boxCollider, collisionState);

            // handle map bounds
            var x = MathHelper.Clamp(_mover.transform.position.X, 0, _tiledMap.widthInPixels);
            _mover.transform.position = new Vector2(x, _mover.transform.position.Y);

            // update velocity
            if (collisionState.right || collisionState.left)
                velocity.X = 0;
            if (collisionState.above || collisionState.below)
                velocity.Y = 0;
        }

        public float defaultGetDeltaTime()
        {
            return Time.deltaTime;
        }

        public void jump()
        {
            velocity.Y = -Mathf.sqrt(2 * jumpHeight * gravity);
        }
    }
}
