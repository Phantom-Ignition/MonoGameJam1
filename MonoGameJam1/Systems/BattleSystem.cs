using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Components.Colliders;
using MonoGameJam1.Components.Map;
using MonoGameJam1.Components.Player;
using MonoGameJam1.Components.Sprites;
using Nez;
using Random = Nez.Random;

namespace MonoGameJam1.Systems
{
    public class BattleSystem : EntityProcessingSystem
    {
        //--------------------------------------------------
        // Player Component

        private readonly PlayerComponent _playerComponent;

        //--------------------------------------------------
        // Effects

        public Texture2D BulletEffectTexture { get; set; }

        //----------------------//------------------------//

        public BattleSystem(PlayerComponent playerComponent) : base(new Matcher().all(typeof(BattleComponent),
            typeof(AnimatedSprite), typeof(BoxCollider)))
        {
            _playerComponent = playerComponent;
        }

        public override void process(Entity entity)
        {
            var sprite = entity.getComponent<AnimatedSprite>();
            if (sprite.getCurrentAnimation().FramesToAttack.Contains(sprite.CurrentFrame))
            {
                var fIsEnemy = entity.getComponent<EnemyComponent>() != null;
                foreach (var otherEntity in _entities)
                {
                    if (otherEntity == entity) continue;
                    var otherBattler = otherEntity.getComponent<BattleComponent>();
                    if (otherBattler.isOnImmunity() || otherBattler.Dying || !otherBattler.battleEntity.canTakeDamage) continue;
                    var tIsEnemy = otherBattler.getComponent<EnemyComponent>() != null;
                    if (fIsEnemy && tIsEnemy) return;
                    var collider = getBattleCollider(otherEntity);
                    foreach (var attackCollider in sprite.getCurrentFrame().AttackColliders)
                    {
                        CollisionResult collisionResult;
                        if (attackCollider.collidesWith(collider, out collisionResult))
                        {
                            // Freeze time
                            Time.timeScale = 0.2f;
                            Core.schedule(0.01f, t =>
                            {
                                Time.timeScale = 1;
                            });
                            otherBattler.onHit(collisionResult);
                            if (!fIsEnemy)
                                playHitEffects(attackCollider.bounds, collider.bounds);
                        }
                    }
                }
            }
        }

        private BoxCollider getBattleCollider(Entity entity)
        {
            var hurtCollider = entity.getComponent<HurtCollider>();
            return hurtCollider ?? entity.getComponent<BoxCollider>();
        }

        private void playHitEffects(RectangleF attackRect, RectangleF targetRect)
        {
            if (BulletEffectTexture == null || !_playerComponent.IsInPistolMode()) return;

            var intersection = RectangleF.intersect(attackRect, targetRect);
            var w = intersection.width * 0.6f;
            var diffX = intersection.width - w;
            intersection.width = w;
            intersection.x += diffX;
            
            var point = RandomPointInRect(intersection);

            var direction = Math.Sign(_playerComponent.transform.position.X - targetRect.center.X) * Vector2.UnitX;
            createBulletEffect(direction, point);
        }

        public void createBulletEffect(Vector2 direction, Vector2 position)
        {
            if (_playerComponent.entity == null) return;

            var randIndex = Random.nextInt(2);
            var effect = _playerComponent.entity.scene.createEntity();
            var effectSprite = effect.addComponent(new AnimatedSprite(BulletEffectTexture, "default"));
            effectSprite.CreateAnimation("default", 0.07f, false);
            effectSprite.AddFrames("default", new List<Rectangle>
            {
                new Rectangle(randIndex == 0 ? 0 : 60, 0, 20, 20),
                new Rectangle(randIndex == 0 ? 20 : 80, 0, 20, 20),
                new Rectangle(randIndex == 0 ? 40 : 100, 0, 20, 20),
            });
            effectSprite.spriteEffects = direction.X < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            effect.position = position + 10 * direction.X * Vector2.UnitX;
            effect.addComponent<SpriteEffectComponent>();
        }

        private Vector2 RandomPointInRect(Rectangle rect)
        {
            var min = new Vector2(rect.Left, rect.Top);
            var point = new Vector2(Random.nextInt(rect.Width), Random.nextInt(rect.Height));
            return min + point;
        }
    }
}
