using System;
using Microsoft.Xna.Framework;
using MonoGameJam1.Components.Sprites;
using Nez;
using Nez.Sprites;

namespace MonoGameJam1.Components.Battle
{
    public interface IBattleEntity
    {
        bool canTakeDamage { get; }

        void onHit(Vector2 knockback);
        void onDeath();
    }

    public class BattleComponent: Component, IUpdatable
    {
        //--------------------------------------------------
        // Battle Entity

        public IBattleEntity battleEntity;

        //--------------------------------------------------
        // HP

        private float _hp;
        public float HP => _hp;

        //--------------------------------------------------
        // Death animation

        private bool _dying;
        public bool Dying => _dying;

        private const float DeathDuration = 0.2f;
        private float _deathTime;

        //--------------------------------------------------
        // Immunity Duration

        public float ImmunityDuration = 0.1f;
        public float ImmunityTime { get; set; }
        public bool ForceImmunity { get; set; }

        //--------------------------------------------------
        // Hit Animation

        private const float HitAnimationDuration = 0.3f;
        private float _hitAnimation;

        //--------------------------------------------------
        // Sprites

        private SpriteMime _spriteMime;
        private AnimatedSprite _animatedSprite;

        //--------------------------------------------------
        // Destroy entity

        public Action destroyEntityAction;

        //----------------------//------------------------//

        public override void onAddedToEntity()
        {
            _spriteMime = entity.addComponent<SpriteMime>();
            _spriteMime.color = Color.Transparent;
            _animatedSprite = entity.getComponent<AnimatedSprite>();

            destroyEntityAction = destroyEntity;
        }

        public void setHp(int hp)
        {
            _hp = hp;
        }

        public bool onHit(CollisionResult collisionResult)
        {
            var knockback = new Vector2(Math.Sign(collisionResult.minimumTranslationVector.X), 0);
            return onHit(knockback);
        }

        public bool onHit(Vector2 knockback)
        {
            if (_dying || ImmunityTime > 0.0f || battleEntity != null && !battleEntity.canTakeDamage) return false;

            knockback *= Vector2.UnitX;
            battleEntity?.onHit(knockback);
            _hitAnimation = 0.25f;
            ImmunityTime = ImmunityDuration;

            _hp--;
            if (_hp <= 0)
            {
                _animatedSprite.play("dying");
                _dying = true;
                _deathTime = DeathDuration;
                battleEntity?.onDeath();
            }

            return true;
        }

        public void update()
        {
            if (ImmunityTime > 0.0f) ImmunityTime -= Time.deltaTime;

            if (_hitAnimation > 0.0f)
            {
                var n = MathHelper.Max(0, _hitAnimation - Time.deltaTime);
                _hitAnimation = n;
                if (n > 0.0f)
                {
                    var color = Color.Red * (n / HitAnimationDuration);
                    _spriteMime.setColor(color);
                }
                else
                {
                    _spriteMime.setColor(Color.Transparent);
                }
            }

            _spriteMime.setLocalOffset(_animatedSprite.localOffset);

            if (_dying && _animatedSprite.Looped)
            {
                _deathTime = MathHelper.Max(0, _deathTime - Time.deltaTime);
                if (_deathTime > 0.0f)
                {
                    var color = Color.White * (_deathTime / DeathDuration);
                    _animatedSprite.setColor(color);
                }
                else
                {
                    destroyEntityAction?.Invoke();
                }
            }
        }

        public virtual void destroyEntity()
        {
            entity.destroy();
        }

        public bool isOnImmunity()
        {
            return ForceImmunity || ImmunityTime > 0.0f;
        }
    }
}
