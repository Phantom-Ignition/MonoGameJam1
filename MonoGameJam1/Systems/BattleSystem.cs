using System;
using System.Collections.Generic;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Components.Colliders;
using MonoGameJam1.Components.Sprites;
using Nez;

namespace MonoGameJam1.Systems
{
    class BattleSystem : EntityProcessingSystem
    {
        public BattleSystem() : base(new Matcher().all(typeof(BattleComponent), typeof(AnimatedSprite), typeof(BoxCollider))) { }

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
    }
}
