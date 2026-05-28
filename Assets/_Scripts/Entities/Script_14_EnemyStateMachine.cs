namespace MutationSwarm.Entities
{
    public interface IEnemyState
    {
        void Enter(Script_13_EnemyBase enemy);
        void Execute(Script_13_EnemyBase enemy);
        void Exit(Script_13_EnemyBase enemy);
    }

    /// <summary>
    /// Máquina de estados: Idle, Pursue, Attack, Flee, Swarm.
    /// </summary>
    public class Script_14_EnemyStateMachine
    {
        private readonly Script_13_EnemyBase _enemy;
        private IEnemyState _currentState;

        public Script_14_EnemyStateMachine(Script_13_EnemyBase enemy) => _enemy = enemy;

        public void Initialize()
        {
            ChangeState(new IdleState());
        }

        public void Tick() => _currentState?.Execute(_enemy);

        public void ChangeState(IEnemyState newState)
        {
            _currentState?.Exit(_enemy);
            _currentState = newState;
            _currentState?.Enter(_enemy);
        }
    }

    public class IdleState : IEnemyState
    {
        public void Enter(Script_13_EnemyBase enemy) { }
        public void Execute(Script_13_EnemyBase enemy) { }
        public void Exit(Script_13_EnemyBase enemy) { }
    }
}
