using MonoGameJam1.FSM;

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
        public override void begin()
        {
            entity.sprite.play("active");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyTrapIdle());
            }
        }
    }
}
