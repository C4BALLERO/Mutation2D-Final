using UnityEngine;

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
        public IEnemyState CurrentState => _currentState;

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
        private float _waitTimer;

        public void Enter(Script_13_EnemyBase enemy)
        {
            // Espera aleatoria corta para evitar sincronización robótica.
            _waitTimer = Random.Range(0.5f, 1.5f);
        }

        public void Execute(Script_13_EnemyBase enemy)
        {
            _waitTimer -= Time.deltaTime;

            if (enemy.Genome.Regeneracion > 0.4f && enemy.CurrentHp < enemy.MaxHp * 0.15f)
            {
                enemy.StateMachine.ChangeState(new FleeState());
                return;
            }

            if (enemy.CanSeePlayer() && _waitTimer <= 0f)
            {
                enemy.StateMachine.ChangeState(new PursueState());
            }
        }

        public void Exit(Script_13_EnemyBase enemy) { }
    }

    public class PursueState : IEnemyState
    {
        private float _loseSightTimer;

        public void Enter(Script_13_EnemyBase enemy)
        {
            _loseSightTimer = 2f;
        }

        public void Execute(Script_13_EnemyBase enemy)
        {
            Transform target = enemy.GetNearestPlayer();
            if (target == null)
            {
                _loseSightTimer -= Time.deltaTime;
                if (_loseSightTimer <= 0f)
                    enemy.StateMachine.ChangeState(new IdleState());
                return;
            }

            Vector2 directionToPlayer = ((Vector2)target.position - (Vector2)enemy.transform.position).normalized;
            Vector2 finalDirection = directionToPlayer;

            if (enemy.Genome.ComportamientoGrupal > 0.5f)
            {
                Vector2 centroidDirection = (enemy.GetAlliesCentroid(3f) - (Vector2)enemy.transform.position).normalized;
                finalDirection = Vector2.Lerp(directionToPlayer, centroidDirection, 0.3f).normalized;
            }

            enemy.MoveInDirection(finalDirection);

            if (enemy.DistanceTo(target) <= enemy.AttackRange)
            {
                enemy.StateMachine.ChangeState(new AttackState());
                return;
            }

            if (enemy.Genome.ComportamientoGrupal > 0.6f && enemy.GetNearbyAllies(3f).Count >= 3)
            {
                enemy.StateMachine.ChangeState(new SwarmState());
                return;
            }

            if (enemy.Genome.Regeneracion > 0.4f && enemy.CurrentHp < enemy.MaxHp * 0.15f)
            {
                enemy.StateMachine.ChangeState(new FleeState());
            }
        }

        public void Exit(Script_13_EnemyBase enemy) { }
    }

    public class AttackState : IEnemyState
    {
        private float _attackCooldown;

        public void Enter(Script_13_EnemyBase enemy)
        {
            _attackCooldown = 0f;
        }

        public void Execute(Script_13_EnemyBase enemy)
        {
            Transform target = enemy.GetNearestPlayer();
            if (target == null)
            {
                enemy.StateMachine.ChangeState(new IdleState());
                return;
            }

            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown > 0f)
                return;

            float distance = enemy.DistanceTo(target);
            if (distance < 0.8f)
            {
                enemy.DealMeleeDamageTo(target);

                Vector2 pushDir = ((Vector2)enemy.transform.position - (Vector2)target.position).normalized;
                enemy.AddKnockback(pushDir, 1.5f);

                // Espinas: contra-daño al atacante.
                enemy.ApplySpinesCounterDamage(target);

                // Veneno: aplica DOT al jugador.
                enemy.TryApplyPoison(target);

                if (enemy.ShouldSuicideExplode())
                {
                    enemy.TriggerSuicideExplosion();
                    return;
                }
            }

            _attackCooldown = 1f / (1f + enemy.Genome.Velocidad * 0.3f);
            enemy.StateMachine.ChangeState(new PursueState());
        }

        public void Exit(Script_13_EnemyBase enemy) { }
    }

    public class FleeState : IEnemyState
    {
        private const float RegenRate = 8f;

        public void Enter(Script_13_EnemyBase enemy) { }

        public void Execute(Script_13_EnemyBase enemy)
        {
            Transform target = enemy.GetNearestPlayer();
            if (target == null)
            {
                enemy.StateMachine.ChangeState(new IdleState());
                return;
            }

            Vector2 fleeDirection = ((Vector2)enemy.transform.position - (Vector2)target.position).normalized;
            enemy.MoveInDirection(fleeDirection, 1.5f);
            enemy.HealByRegen(RegenRate);

            if (enemy.CurrentHp > enemy.MaxHp * 0.5f)
                enemy.StateMachine.ChangeState(new PursueState());
        }

        public void Exit(Script_13_EnemyBase enemy) { }
    }

    public class SwarmState : IEnemyState
    {
        private bool _flankRight;

        public void Enter(Script_13_EnemyBase enemy)
        {
            _flankRight = Random.value > 0.5f;
        }

        public void Execute(Script_13_EnemyBase enemy)
        {
            Transform target = enemy.GetNearestPlayer();
            if (target == null)
            {
                enemy.StateMachine.ChangeState(new IdleState());
                return;
            }

            int allies = enemy.GetNearbyAllies(3f).Count;
            if (allies < 2)
            {
                enemy.StateMachine.ChangeState(new PursueState());
                return;
            }

            Vector2 toPlayer = ((Vector2)target.position - (Vector2)enemy.transform.position).normalized;
            float roleRoll = Mathf.Abs(Mathf.Sin(Time.time * 11f + enemy.GetHashCode()));

            // 40% frontal, 60% flanqueo.
            if (roleRoll <= 0.4f)
            {
                enemy.MoveInDirection(toPlayer);
            }
            else
            {
                Vector2 flankDir = _flankRight
                    ? new Vector2(-toPlayer.y, toPlayer.x)
                    : new Vector2(toPlayer.y, -toPlayer.x);
                Vector2 flankTarget = (Vector2)target.position + flankDir.normalized * 1.8f;
                enemy.MoveTowards(flankTarget);
            }

            if (enemy.DistanceTo(target) <= enemy.AttackRange)
                enemy.StateMachine.ChangeState(new AttackState());
        }

        public void Exit(Script_13_EnemyBase enemy) { }
    }
}
