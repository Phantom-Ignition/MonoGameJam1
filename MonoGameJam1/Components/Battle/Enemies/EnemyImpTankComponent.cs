using Microsoft.Xna.Framework.Graphics;
using Nez;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public class EnemyImpTankComponent : EnemyImpComponent
    {
        public EnemyImpTankComponent(bool patrolStartRight) : base(patrolStartRight)
        {
        }

        public override void onAddedToEntity()
        {
            base.onAddedToEntity();
            _battleComponent.setHp(200);
        }

        protected override Texture2D getImpTexture()
        {
            return entity.scene.content.Load<Texture2D>(Content.Characters.impTank);
        }
    }
}
