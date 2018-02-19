using MonoGameJam1.FSM;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public class EnemyMineStates : State<EnemyMineStates, EnemyMineComponent>
    {
        public override void begin() { }

        public override void end() { }

        public override void update() { }
    }

    public class EnemyMineIdle : EnemyMineStates
    {
        public override void begin()
        {
            entity.sprite.play("idle");
        }

        public override void update()
        {
            if (entity.canSeeThePlayer())
            {
                fsm.resetStackTo(new EnemyMineTicking());
            }
        }
    }

    public class EnemyMineTicking : EnemyMineStates
    {
        public override void begin()
        {
            entity.sprite.play("ticking");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                fsm.resetStackTo(new EnemyMineExplode());
            }
        }
    }

    public class EnemyMineExplode : EnemyMineStates
    {
        public override void begin()
        {
            entity.sprite.play("explode");
        }

        public override void update()
        {
            if (entity.sprite.Looped)
            {
                entity.entity.destroy();
            }
        }
    }
}
