using Nez;

namespace MonoGameJam1.Components.Map
{
    public class BattleAreaComponent : Component
    {
        public BoxCollider collider;
        public bool Activated { get; private set; }

        public string[] Enemies { get; set; }
        public int[] Waves { get; set; }

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
