using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez;

namespace MonoGameJam1.Systems
{
    class CameraSystem : EntitySystem
    {
        public Camera camera;

        /// <summary>
        /// how fast the camera closes the distance to the target position
        /// </summary>
        public float followLerp = 0.2f;

        /// <summary>
        /// size of the deadzone
        /// </summary>
        public Vector2 deadzoneSize = new Vector2(10, 10);

        /// <summary>
        /// when in CameraWindow mode the width/height is used as a bounding box to allow movement within it without moving the camera.
        /// when in LockOn mode only the deadzone x/y values are used. This is set to sensible defaults when you call follow but you are
        /// free to override it to get a custom deadzone directly or via the helper setCenteredDeadzone.
        /// </summary>
        public RectangleF deadzone;

        /// <summary>
        /// offset from the screen center that the camera will focus on
        /// </summary>
        public Vector2 focusOffset;

        /// <summary>
        /// If true, the camera position will not got out of the map rectangle (0,0, mapwidth, mapheight)
        /// </summary>
        public bool mapLockEnabled;

        /// <summary>
        /// Contains the width and height of the current map.
        /// </summary>
        public Vector2 mapSize;

        Entity _targetEntity;
        Collider _targetCollider;
        Vector2 _desiredPositionDelta;
        RectangleF _worldSpaceDeadzone;

        //--------------------------------------------------
        // Camera Shake

        private bool _shakeEnabled;
        private float _shakeDuration;
        private float _shakeElapsed;
        private float _shakeMagnitude;
        private Vector2 _shakeOffset;

        //----------------------//------------------------//

        public CameraSystem(Entity target, Camera camera) : base()
        {
            _targetEntity = target;
            this.camera = camera ?? target.scene.camera;

            follow(_targetEntity);

            Core.emitter.addObserver(CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset);
        }

        public CameraSystem(Entity targetEntity) : this( targetEntity, null )
		{ }
        
        public void SetTarget(Entity target)
        {
            _targetEntity = target;
        }

        public void startCameraShake(float magnitude, float duration)
        {
            _shakeMagnitude = _shakeEnabled ? _shakeMagnitude + magnitude * 0.1f : magnitude;
            _shakeEnabled = true;
            _shakeElapsed = 0;
            _shakeDuration = duration;
        }

        protected override void lateProcess(List<Entity> entities)
        {
            var entity = _targetEntity;
            if (entity.scene == null || entity.scene?.sceneRenderTarget == null) return;
            // translate the deadzone to be in world space
            var halfScreen = entity.scene.sceneRenderTargetSize.ToVector2() * 0.5f;
            _worldSpaceDeadzone.x = (int)(camera.position.X - halfScreen.X + deadzone.x + focusOffset.X);
            _worldSpaceDeadzone.y = (int)(camera.position.Y - halfScreen.Y + deadzone.y + focusOffset.Y);
            _worldSpaceDeadzone.width = deadzone.width;
            _worldSpaceDeadzone.height = deadzone.height;

            if (_targetEntity != null)
                updateFollow();

            updateCameraShake();

            camera.position = Vector2.Lerp(camera.position, camera.position + _desiredPositionDelta, followLerp);
            camera.position += _shakeOffset;
            camera.entity.transform.roundPosition();

            if (mapLockEnabled)
            {
                camera.position = clampToMapSize(camera.position);
                camera.entity.transform.roundPosition();
            }
        }


        /// <summary>
        /// Clamps the camera so it never leaves the visible area of the map.
        /// </summary>
        /// <returns>The to map size.</returns>
        /// <param name="position">Position.</param>
        Vector2 clampToMapSize(Vector2 position)
        {
            var halfScreen = new Vector2(camera.bounds.width, camera.bounds.height) * 0.5f;
            var cameraMax = new Vector2(mapSize.X - halfScreen.X, mapSize.Y - halfScreen.Y);

            return Vector2.Clamp(position, halfScreen, cameraMax);
        }

        void onGraphicsDeviceReset()
        {
            // we need this to occur on the next frame so the camera bounds are updated
            Core.schedule(0f, this, t =>
            {
                var self = t.context as CameraSystem;
                self.follow(self._targetEntity);
            });
        }

        void updateFollow()
        {
            _desiredPositionDelta.X = _desiredPositionDelta.Y = 0;

            // make sure we have a targetCollider for CameraWindow. If we dont bail out.
            if (_targetCollider == null)
            {
                _targetCollider = _targetEntity.getComponent<Collider>();
                if (_targetCollider == null)
                    return;
            }

            var targetBounds = _targetEntity.getComponent<Collider>().bounds;
            if (!_worldSpaceDeadzone.contains(targetBounds))
            {
                // x-axis
                if (_worldSpaceDeadzone.left > targetBounds.left)
                    _desiredPositionDelta.X = targetBounds.left - _worldSpaceDeadzone.left;
                else if (_worldSpaceDeadzone.right < targetBounds.right)
                    _desiredPositionDelta.X = targetBounds.right - _worldSpaceDeadzone.right;

                // y-axis
                if (_worldSpaceDeadzone.bottom < targetBounds.bottom)
                    _desiredPositionDelta.Y = targetBounds.bottom - _worldSpaceDeadzone.bottom;
                else if (_worldSpaceDeadzone.top > targetBounds.top)
                    _desiredPositionDelta.Y = targetBounds.top - _worldSpaceDeadzone.top;
            }
        }


        public void follow(Entity targetEntity)
        {
            _targetEntity = targetEntity;
            var cameraBounds = camera.bounds;
            deadzone = new RectangleF(cameraBounds.width / 2, cameraBounds.height / 2, deadzoneSize.X, deadzoneSize.Y);
        }

        private void updateCameraShake()
        {
            if (_shakeEnabled)
            {
                if (_shakeElapsed > _shakeDuration)
                {
                    _shakeEnabled = false;
                    _shakeOffset = Vector2.Zero;
                }
                else
                {
                    _shakeElapsed += Time.deltaTime * 1000;
                    var percentComplete = _shakeElapsed / _shakeDuration;
                    var damper = 1.0f - MathHelper.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);
                    var x = Random.nextFloat() * 2.0f - 1.0f;
                    var y = Random.nextFloat() * 2.0f - 1.0f;
                    x *= _shakeMagnitude * damper;
                    y *= _shakeMagnitude * damper;
                    _shakeOffset = new Vector2(x, y);
                }
            }
        }


        /// <summary>
        /// sets up the deadzone centered in the current cameras bounds with the given size
        /// </summary>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public void setCenteredDeadzone(int width, int height)
        {
            Assert.isFalse(camera == null, "camera is null. We cant get its bounds if its null. Either set it or wait until after this Component is added to the Entity.");
            var cameraBounds = camera.bounds;
            deadzone = new RectangleF((cameraBounds.width - width) / 2, (cameraBounds.height - height) / 2, width, height);
        }

    }
}
