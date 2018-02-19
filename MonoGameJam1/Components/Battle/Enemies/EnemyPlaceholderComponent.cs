using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Colliders;
using MonoGameJam1.Components.Sprites;
using MonoGameJam1.Scenes;
using Nez;

namespace MonoGameJam1.Components.Battle.Enemies
{
    class EnemyPlaceholderComponent : EnemyComponent
    {
        public override void initialize()
        {
            base.initialize();

            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.placeholder);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.25f);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 32, 32),
            });

            // View range
            areaOfSight = entity.addComponent(new AreaOfSightCollider(-24, -12, 92, 32));

            shot(true);
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            // Change move speed
            platformerObject.maxMoveSpeed = 60;
            platformerObject.moveSpeed = 60;
        }

        public void createShot(int type)
        {
            var shots = entity.scene.findEntitiesWithTag(SceneMap.PROJECTILES);
            var shot = entity.scene.createEntity($"shot:${shots.Count}");
            var direction = sprite.spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1;
            shot.addComponent(new ProjectileComponent(direction, 1000));
            var position = entity.getComponent<BoxCollider>().absolutePosition;
            shot.transform.position = position;
            /*
            if (sprite.spriteEffects == SpriteEffects.FlipHorizontally)
            {
                shot.transform.position += new Vector2(-12, 12);
            }
            else
            {
                shot.transform.position += 12 * Vector2.One;
            }
            */
        }

        public void shot(bool a)
        {
            Core.schedule(a ? 0 : 1, t =>
            {
                createShot(0);
                shot(false);
            });
        }
    }
}
