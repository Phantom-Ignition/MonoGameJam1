using MonoGameJam1.FSM;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public class EnemyTargetDummyStates : State<EnemyTargetDummyStates, EnemyTargetDummyComponent>
    {
        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyTargetDumbIdle : EnemyTargetDummyStates
    {
        public override void begin()
        {
            entity.sprite.play("stand");
        }
    }

    public class EnemyTargetDumbHit : EnemyTargetDummyStates
    {
        public override void begin()
        {
            entity.sprite.play("hit");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
                fsm.resetStackTo(new EnemyTargetDumbIdle());
        }
    }
}
