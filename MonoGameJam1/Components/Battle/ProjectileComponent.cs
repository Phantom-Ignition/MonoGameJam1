using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Sprites;
using MonoGameJam1.Scenes;
using Nez;

namespace MonoGameJam1.Components.Battle
{
    public class ProjectileComponent : Component
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Direction, Speed and Rotation

        private readonly int _direction;
        private readonly float _speed;
        private readonly float _rotation;

        //----------------------//------------------------//

        public ProjectileComponent(int direction, float speed, float rotation = 0.0f)
        {
            _direction = direction;
            _speed = speed;
            _rotation = rotation;
        }

        public override void initialize()
        {
            /*var texture = entity.scene.content.Load<Texture2D>(Content.Misc.arrow);
            sprite = entity.addComponent(new AnimatedSprite(texture, "default"));
            sprite.CreateAnimation("default", 0.2f);
            sprite.AddFrames("default", new List<Rectangle>
            {
                new Rectangle(0, 0, 12, 5)
            });

            var collider = entity.addComponent(new BoxCollider(-6, -2, 12, 5));
            Flags.setFlagExclusive(ref collider.physicsLayer, SceneMap.PROJECTILES_LAYER);
            
            if (_direction < 0)
            {
                sprite.spriteEffects = SpriteEffects.FlipHorizontally;
            }

            sprite.entity.setRotation(_rotation);
            sprite.renderLayer = SceneMap.MISC_RENDER_LAYER;*/
        }

        public override void onAddedToEntity()
        {
            entity.setTag(SceneMap.PROJECTILES);
        }

        public void update()
        {
            var scale = Time.timeScale < 1 ? 0.1f : Time.timeScale;
            var deltaTime = Time.unscaledDeltaTime * scale;
            var velx = Mathf.cos(entity.transform.rotation) * _speed * _direction * deltaTime;
            var vely = Mathf.sin(entity.transform.rotation) * _speed * _direction * deltaTime;
            var vel = new Vector2(velx, vely);
            entity.setPosition(entity.position + vel);
        }
    }
}
