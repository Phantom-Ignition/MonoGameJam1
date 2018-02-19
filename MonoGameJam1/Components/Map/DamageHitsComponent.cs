using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Tweens;

namespace MonoGameJam1.Components.Map
{
    class DamageHitsComponent : RenderableComponent, IUpdatable
    {
        public override float width => 300;
        public override float height => 300;

        protected class DamageHitInstance
        {
            public Vector2 absolutePosition;
            public Vector2 positionOffset;
            public Vector2 position;
            public bool done;

            public int damage;
            public float halfWidth;

            public DamageHitInstance(int damage, Vector2 absolutePosition, Vector2 positionOffset)
            {
                this.damage = damage;
                this.absolutePosition = absolutePosition;

                halfWidth = GameMain.smallBitmapFont.measureString($"{damage}").X / 2;
                this.positionOffset = positionOffset - halfWidth * Vector2.UnitX;
            }

            public void animate()
            {
                this.tween("position", new Vector2(0, -30), 0.5f )
                    .setEaseType(EaseType.CubicIn)
                    .setCompletionHandler((value) => { done = true; })
                    .start();
            }

            public void render(Graphics graphics)
            {
                graphics.batcher.drawString(GameMain.smallBitmapFont, $"{damage}", absolutePosition + positionOffset + position + Vector2.One, Color.Black);
                graphics.batcher.drawString(GameMain.smallBitmapFont, $"{damage}", absolutePosition + positionOffset + position, Color.White);
            }
        }

        private readonly List<DamageHitInstance> _damageHits;
        private readonly List<DamageHitInstance> _damageHitsToRemove;

        public DamageHitsComponent()
        {
            _damageHits = new List<DamageHitInstance>();
            _damageHitsToRemove = new List<DamageHitInstance>();
        }

        public void addNumber(int damage, Vector2 positionOffset)
        {
            var damageHit = new DamageHitInstance(damage, entity.position, positionOffset);
            damageHit.animate();
            _damageHits.Add(damageHit);
        }

        public void update()
        {
            foreach (var damageHitInstance in _damageHits)
            {
                if (damageHitInstance.done)
                {
                    _damageHitsToRemove.Add(damageHitInstance);
                }
            }
            foreach (var damageHitInstance in _damageHitsToRemove)
            {
                _damageHits.Remove(damageHitInstance);
            }
            _damageHitsToRemove.Clear();
        }

        public override void render(Graphics graphics, Camera camera)
        {
            foreach (var damageHitInstance in _damageHits)
            {
                damageHitInstance.render(graphics);
            }
        }
    }
}
