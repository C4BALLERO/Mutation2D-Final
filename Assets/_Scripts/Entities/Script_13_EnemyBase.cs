using System;
using System.Collections.Generic;
using MutationSwarm.Combat;
using MutationSwarm.Core;
using MutationSwarm.Evolution;
using UnityEngine;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Clase base de enemigo: aplica Genome, stats y datos de combate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Script_13_EnemyBase : MonoBehaviour
    {
        [Header("Base")]
        [SerializeField] private float _baseSpeed = 2f;
        [SerializeField] private float _baseHp = 30f;
        [SerializeField] private float _baseDamage = 8f;
        [SerializeField] private float _attackRange = 0.8f;
        [SerializeField] private float _spinesDamageMultiplier = 10f;

        [Header("Detección")]
        [SerializeField] private LayerMask _playerMask;
        [SerializeField] private LayerMask _enemyMask;

        [Header("Visual")]
        [SerializeField] private Material _mutationMaterialTemplate;

        public Genome Genome { get; private set; }
        public float TimeAlive { get; private set; }
        public float DamageDone { get; private set; }
        public float CurrentHp => _currentHp;
        public float MaxHp => _maxHp;
        public float AttackRange => _attackRange;
        public float SpinesDamageMultiplier => _spinesDamageMultiplier;
        public Script_14_EnemyStateMachine StateMachine => _stateMachine;

        private Script_14_EnemyStateMachine _stateMachine;
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb;
        private Script_22_StatusEffects _statusEffects;
        private Material _runtimeMutationMaterial;
        private float _currentHp;
        private float _maxHp;
        private bool _hasDied;
        private Vector2 _lastMoveDirection = Vector2.right;

        public void Initialize(Genome genome)
        {
            Genome = genome;
            _maxHp = _baseHp * genome.GetFitnessModifier();
            _currentHp = _maxHp;
            transform.localScale = Vector3.one * genome.Tamaño;

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_rb == null)
                _rb = GetComponent<Rigidbody2D>();
            if (_statusEffects == null)
                _statusEffects = GetComponent<Script_22_StatusEffects>();

            if (_spriteRenderer != null)
            {
                Color mutationColor = genome.GetMutationColor();
                _spriteRenderer.color = mutationColor;
                if (_mutationMaterialTemplate != null)
                {
                    _runtimeMutationMaterial = new Material(_mutationMaterialTemplate);
                    _runtimeMutationMaterial.color = mutationColor;
                    _spriteRenderer.material = _runtimeMutationMaterial;
                }
            }

            // Ajuste de física 2D según el genoma mutado.
            _rb.mass = Mathf.Lerp(0.8f, 2.2f, Mathf.InverseLerp(Genome.TamañoMin, Genome.TamañoMax, genome.Tamaño));
            _rb.linearDamping = Mathf.Lerp(0f, 1.2f, genome.Armadura);
            _rb.gravityScale = 1f + genome.Salto * 0.4f;

            // Inicialización de la StateMachine con el Genome recibido del WaveManager.
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

        public void MoveTowards(Vector2 worldTarget, float speedMultiplier = 1f)
        {
            Vector2 currentPosition = _rb.position;
            Vector2 targetPosition = Vector2.MoveTowards(currentPosition, worldTarget, GetMoveSpeed() * speedMultiplier * Time.deltaTime);
            Vector2 delta = (targetPosition - currentPosition);
            if (delta.sqrMagnitude > 0.0001f)
                _lastMoveDirection = delta.normalized;

            _rb.MovePosition(targetPosition);
        }

        public void MoveInDirection(Vector2 direction, float speedMultiplier = 1f)
        {
            Vector2 normalized = direction.sqrMagnitude < 0.001f ? _lastMoveDirection : direction.normalized;
            _lastMoveDirection = normalized;
            _rb.MovePosition(_rb.position + normalized * GetMoveSpeed() * speedMultiplier * Time.deltaTime);
        }

        public void AddKnockback(Vector2 direction, float force)
        {
            _rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        }

        public Transform GetNearestPlayer()
        {
            Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, GetVisionRadius(), _playerMask);
            Transform nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (Collider2D player in players)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = player.transform;
                }
            }
            return nearest;
        }

        public bool CanSeePlayer()
        {
            Collider2D target = Physics2D.OverlapCircle(transform.position, GetVisionRadius(), _playerMask);
            return target != null;
        }

        public List<Script_13_EnemyBase> GetNearbyAllies(float radius)
        {
            Collider2D[] allies = Physics2D.OverlapCircleAll(transform.position, radius, _enemyMask);
            List<Script_13_EnemyBase> result = new();
            foreach (Collider2D ally in allies)
            {
                if (ally.gameObject == gameObject)
                    continue;
                if (ally.TryGetComponent(out Script_13_EnemyBase enemy))
                    result.Add(enemy);
            }
            return result;
        }

        public Vector2 GetAlliesCentroid(float radius)
        {
            List<Script_13_EnemyBase> allies = GetNearbyAllies(radius);
            if (allies.Count == 0)
                return transform.position;

            Vector2 sum = Vector2.zero;
            foreach (Script_13_EnemyBase ally in allies)
                sum += (Vector2)ally.transform.position;
            return sum / allies.Count;
        }

        public float DistanceTo(Transform target)
        {
            if (target == null)
                return float.MaxValue;
            return Vector2.Distance(transform.position, target.position);
        }

        public void DealMeleeDamageTo(Transform playerTarget)
        {
            if (playerTarget == null)
                return;

            if (playerTarget.TryGetComponent(out Script_11_PlayerController playerController))
                playerController.ApplyDamage(_baseDamage * (1f + Genome.Velocidad * 0.2f));

            RegisterDamageDealt(_baseDamage);
        }

        public void ApplySpinesCounterDamage(Transform attacker)
        {
            if (attacker == null || Genome.Espinas <= 0.3f)
                return;

            float damage = Genome.Espinas * _spinesDamageMultiplier;
            if (attacker.TryGetComponent(out Script_12_PlayerStats playerStats))
                playerStats.TakeDamage(damage);
        }

        public void TryApplyPoison(Transform playerTarget)
        {
            if (playerTarget == null || Genome.Veneno <= 0.3f)
                return;

            if (playerTarget.TryGetComponent(out Script_22_StatusEffects effects))
                effects.Apply(StatusEffectType.Poison, Genome.Veneno * 10f, Genome.Veneno);
        }

        public bool ShouldSuicideExplode()
        {
            return Genome.ExplosionAlMorir > 0.5f && _currentHp <= _maxHp * 0.2f;
        }

        public void TriggerSuicideExplosion()
        {
            // Placeholder VFX/daño en área: se integra cuando exista prefab de explosión.
            Die(false);
        }

        public void HealByRegen(float regenRatePerSecond)
        {
            float regenAmount = Genome.Regeneracion * regenRatePerSecond * Time.deltaTime;
            _currentHp = Mathf.Min(_maxHp, _currentHp + regenAmount);
        }

        public void TakeDamage(float amount)
        {
            if (_hasDied)
                return;

            _currentHp -= amount;
            if (_currentHp <= 0f)
                Die(false);
        }

        public void Die(bool survived)
        {
            if (_hasDied)
                return;

            _hasDied = true;
            EnemyCombatData data = GetCombatData(survived);
            Script_03_EventBus.Publish(new EnemyDiedEvent { enemy = this, combatData = data });
            Destroy(gameObject);
        }

        public float GetMoveSpeed() => _baseSpeed * (Genome?.Velocidad ?? 1f);
        public float GetVisionRadius() => 5f * (Genome?.RangoVision ?? 0.5f);

        public void DrawVisionGizmo()
        {
            Gizmos.color = new Color(0.8f, 0.1f, 0.8f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, GetVisionRadius());
        }

        private void OnDrawGizmosSelected()
        {
            if (Genome == null)
                return;
            DrawVisionGizmo();
        }
    }
}
