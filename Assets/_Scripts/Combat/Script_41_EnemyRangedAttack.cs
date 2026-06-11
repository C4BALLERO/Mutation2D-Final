using MutationSwarm.Core;
using MutationSwarm.Entities;
using UnityEngine;

namespace MutationSwarm.Combat
{
    /// <summary>
    /// Sistema de ataque a distancia para enemigos.
    /// Los drones pueden disparar proyectiles hacia el jugador o en un patrón.
    /// Se añade a Script_13_EnemyBase como componente opcional.
    /// </summary>
    public class Script_41_EnemyRangedAttack : MonoBehaviour
    {
        [Header("Disparo")]
        [SerializeField] private bool _enabled = true;
        [SerializeField] private float _fireRate = 2f;
        [SerializeField] private float _fireRange = 8f;
        [SerializeField] private string _projectilePoolKey = "Projectile_Enemy_Basic";
        [SerializeField] private Transform _firePoint;

        [Header("Patrón (opcional)")]
        [SerializeField] private bool _useSpreadPattern = false;
        [SerializeField] private int _bulletsPerShot = 1;
        [SerializeField] private float _spreadAngle = 30f;

        private float _nextFireTime;
        private Script_13_EnemyBase _enemy;

        private void Awake()
        {
            _enemy = GetComponent<Script_13_EnemyBase>();
            if (_firePoint == null)
                _firePoint = transform;
        }

        private void Update()
        {
            if (!_enabled || _enemy == null)
                return;

            // Solo disparar si hay un jugador visible
            Transform player = _enemy.GetNearestPlayer();
            if (player == null || _enemy.DistanceTo(player) > _fireRange)
                return;

            if (Time.time >= _nextFireTime)
            {
                FireAtPlayer(player);
                _nextFireTime = Time.time + _fireRate;
            }
        }

        /// <summary>
        /// Dispara en dirección al jugador.
        /// </summary>
        private void FireAtPlayer(Transform playerTarget)
        {
            if (playerTarget == null || Script_04_ObjectPool.Instance == null)
                return;

            Vector2 directionToPlayer = ((Vector2)playerTarget.position - (Vector2)_firePoint.position).normalized;

            if (_useSpreadPattern && _bulletsPerShot > 1)
            {
                FireSpread(directionToPlayer);
            }
            else
            {
                FireSingle(directionToPlayer);
            }
        }

        /// <summary>
        /// Dispara un único proyectil.
        /// </summary>
        private void FireSingle(Vector2 direction)
        {
            GameObject proj = Script_04_ObjectPool.Instance.Get(_projectilePoolKey);
            if (proj == null)
                return;

            proj.transform.position = _firePoint.position;
            if (proj.TryGetComponent(out Script_19_Projectile projectile))
                projectile.Launch(direction);
        }

        /// <summary>
        /// Dispara múltiples proyectiles en patrón de abanico.
        /// </summary>
        private void FireSpread(Vector2 baseDirection)
        {
            float angleStep = _spreadAngle / (_bulletsPerShot - 1);
            float startAngle = -_spreadAngle * 0.5f;

            for (int i = 0; i < _bulletsPerShot; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector2 direction = RotateVector(baseDirection, angle);

                GameObject proj = Script_04_ObjectPool.Instance.Get(_projectilePoolKey);
                if (proj == null)
                    continue;

                proj.transform.position = _firePoint.position;
                if (proj.TryGetComponent(out Script_19_Projectile projectile))
                    projectile.Launch(direction);
            }
        }

        /// <summary>
        /// Rota un vector por un ángulo en grados.
        /// </summary>
        private Vector2 RotateVector(Vector2 vec, float angleDegrees)
        {
            float rad = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(vec.x * cos - vec.y * sin, vec.x * sin + vec.y * cos);
        }

        public void SetEnabled(bool enabled) => _enabled = enabled;
        public bool IsEnabled => _enabled;
    }
}
