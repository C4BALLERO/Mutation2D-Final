using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Clase base de enemigo: aplica Genome, stats y datos de combate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Script_13_EnemyBase : MonoBehaviour
    {
        [SerializeField] private float _baseSpeed = 2f;
        [SerializeField] private float _baseHp = 30f;

        public Genome Genome { get; private set; }
        public float TimeAlive { get; private set; }
        public float DamageDone { get; private set; }

        private Script_14_EnemyStateMachine _stateMachine;
        private SpriteRenderer _spriteRenderer;
        private float _currentHp;

        public void Initialize(Genome genome)
        {
            Genome = genome;
            _currentHp = _baseHp * genome.GetFitnessModifier();
            transform.localScale = Vector3.one * genome.Tamaño;

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer != null)
                _spriteRenderer.color = genome.GetMutationColor();

            _stateMachine = new Script_14_EnemyStateMachine(this);
            _stateMachine.Initialize();
        }

        private void Update()
        {
            TimeAlive += Time.deltaTime;
            _stateMachine?.Tick();
        }

        public void RegisterDamageDealt(float amount) => DamageDone += amount;

        public EnemyCombatData GetCombatData(bool survived) => new()
        {
            genome = Genome,
            timeAlive = TimeAlive,
            damageDone = DamageDone,
            survived = survived
        };

        public float GetMoveSpeed() => _baseSpeed * (Genome?.Velocidad ?? 1f);
        public Transform GetNearestPlayer() => null;
        public float GetVisionRadius() => 5f * (Genome?.RangoVision ?? 0.5f);
    }
}
