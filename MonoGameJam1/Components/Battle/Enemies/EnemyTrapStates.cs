using MonoGameJam1.Extensions;
using MonoGameJam1.FSM;
using MonoGameJam1.Managers;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public class EnemyTrapState : State<EnemyTrapState, EnemyTrapComponent>
    {
        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyTrapIdle : EnemyTrapState
    {
        public override void begin()
        {
            entity.sprite.play("idle");
        }

        public override void update()
        {
            if (entity.Cooldown <= 0.0f && entity.canSeeThePlayer())
            {
                entity.Cooldown = 2.0f;
                fsm.resetStackTo(new EnemyTrapActive());
            }
        }
    }

    public class EnemyTrapActive : EnemyTrapState
    {
        private bool _playedSe;

        public override void begin()
        {
            entity.sprite.play("active");
        }

        public override void update()
        {
            if (!_playedSe && entity.sprite.CurrentFrame == 5)
            {
                _playedSe = true;
                AudioManager.trap.Play(0.5f);
            }
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyTrapIdle());
            }
        }
    }
}
