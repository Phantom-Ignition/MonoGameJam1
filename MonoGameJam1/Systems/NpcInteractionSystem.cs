using System.Collections.Generic;
using MonoGameJam1.Components;
using MonoGameJam1.Components.Player;
using MonoGameJam1.Managers;
using MonoGameJam1.NPCs;
using Nez;

namespace MonoGameJam1.Systems
{
    class NpcInteractionSystem : EntityProcessingSystem
    {
        private PlayerComponent _player;
        private InteractionCollider _interactionCollider;

        public List<NpcBase> _autorunNpcs;

        public bool IsBusy { get; set; }

        public NpcInteractionSystem(PlayerComponent player) : base(Matcher.empty())
        {
            _player = player;
            _interactionCollider = _player.getComponent<InteractionCollider>();
            _autorunNpcs = new List<NpcBase>();
        }

        public void addAutorun(NpcBase npc)
        {
            _autorunNpcs.Add(npc);
        }

        public void mapStart()
        {
            foreach (var npc in _autorunNpcs)
            {
                executeActionList(npc, false);
            }
            _autorunNpcs.Clear();
        }

        public override void onChange(Entity entity)
        {
            var contains = _entities.Contains(entity);
            var interest = entity.getComponent<NpcBase>() != null;

            if (interest && !contains)
            {
                add(entity);
            }
            else if (!interest && contains)
            {
                remove(entity);
            }
        }

        protected override void process(List<Entity> entities)
        {
            var inputManager = Core.getGlobalManager<InputManager>();
            if (!inputManager.IsBusy)
            {
                base.process(entities);
            }
        }

        public override void process(Entity entity)
        {
            var npcComp = entity.getComponent<NpcBase>();
            if (!npcComp.Enabled) return;

            if (npcComp.RunOnTouch)
            {
                CollisionResult collisionResult;
                var collider = entity.getComponent<Collider>();
                if (collider == null) return;
                if (collider.collidesWith(_player.getComponent<InteractionCollider>(), out collisionResult))
                {
                    executeActionList(entity.getComponent<NpcBase>(), true);
                }
            }
            else
            {
                var inputManager = Core.getGlobalManager<InputManager>();
                if (inputManager.InteractionButton.isPressed)
                {
                    CollisionResult collisionResult;
                    if (entity.getComponent<Collider>().collidesWith(_player.getComponent<InteractionCollider>(), out collisionResult))
                    {
                        executeActionList(entity.getComponent<NpcBase>(), true);
                    }
                }
            }
        }

        private void executeActionList(NpcBase npc, bool turnToPlayer)
        {
            Core.getGlobalManager<InputManager>().IsBusy = true;
            npc.executeActionList(turnToPlayer);
        }
    }
}
