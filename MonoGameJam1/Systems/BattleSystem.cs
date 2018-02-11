using System.Collections.Generic;
using MonoGameJam1.Components.Battle;
using MonoGameJam1.Components.Sprites;
using Nez;

namespace MonoGameJam1.Systems
{
    class BattleSystem : EntityProcessingSystem
    {
        public List<Entity> Entities => _entities;

        public BattleSystem() : base(new Matcher().all(typeof(BattleComponent), typeof(AnimatedSprite), typeof(BoxCollider))) { }

        public override void process(Entity entity)
        {

        }
    }
}
