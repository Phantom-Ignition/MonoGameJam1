using Nez;

namespace MonoGameJam1.Components.Map
{
    public class BattleAreaComponent : Component
    {
        public BoxCollider collider;
        public bool Activated { get; private set; }

        public override void onAddedToEntity()
        {
            collider = entity.getComponent<BoxCollider>();
        }

        public void SetActivated()
        {
            Activated = true;
        }
    }
}
