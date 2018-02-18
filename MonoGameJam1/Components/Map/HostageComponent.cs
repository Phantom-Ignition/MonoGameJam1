using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Sprites;
using Nez;
using System.Collections.Generic;
using System.Linq;

namespace MonoGameJam1.Components.Map
{
    public class HostageComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;

        //--------------------------------------------------
        // Rescued

        public bool Rescued { get; private set; }

        //----------------------//------------------------//

        public override void initialize()
        {
            // Init sprite
            var texture = entity.scene.content.Load<Texture2D>(Content.Characters.hostage);
            sprite = entity.addComponent(new AnimatedSprite(texture, "stand"));
            sprite.CreateAnimation("stand", 0.15f);
            sprite.AddFrames("stand", new List<Rectangle>
            {
                new Rectangle(0, 0, 100, 100),
                new Rectangle(100, 0, 100, 100),
                new Rectangle(200, 0, 100, 100),
                new Rectangle(300, 0, 100, 100),
                new Rectangle(400, 0, 100, 100),
                new Rectangle(500, 0, 100, 100),
            }, new[] {0, 0, 0, 0, 0, 0}, new[] {9, 9, 9, 9, 9, 9});

            sprite.CreateAnimation("thanks", 0.1f, false);
            sprite.AddFrames("thanks", new List<Rectangle>
            {
                new Rectangle(300, 0, 100, 100),
                new Rectangle(400, 0, 100, 100),
                new Rectangle(500, 0, 100, 100),
                new Rectangle(300, 0, 100, 100),
                new Rectangle(400, 0, 100, 100),
                new Rectangle(500, 0, 100, 100),

                new Rectangle(0, 100, 100, 100),
                new Rectangle(100, 100, 100, 100),
                new Rectangle(200, 100, 100, 100),
                new Rectangle(300, 100, 100, 100),
                new Rectangle(400, 100, 100, 100),
                new Rectangle(500, 100, 100, 100),

                new Rectangle(0, 200, 100, 100),
                new Rectangle(100, 200, 100, 100),
                new Rectangle(200, 200, 100, 100),
                new Rectangle(300, 200, 100, 100),
                new Rectangle(400, 200, 100, 100),
            }, Enumerable.Repeat(0, 17).ToArray(), Enumerable.Repeat(9, 17).ToArray());

            // collisor init
            entity.addComponent(new BoxCollider(-12, -2, 32, 34));
        }

        public void getRescued()
        {
            if (Rescued) return;
            Rescued = true;
            sprite.play("thanks");
        }

        public void update()
        {

            if (Rescued && sprite.CurrentAnimation == "thanks" && sprite.Looped)
            {
                entity.destroy();
            }
        }
    }
}
