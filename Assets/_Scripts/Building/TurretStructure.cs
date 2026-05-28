using MutationSwarm.Combat;
using MutationSwarm.Core;
using MutationSwarm.Entities;
using UnityEngine;

namespace MutationSwarm.Building
{
    /// <summary>
    /// Torreta automática: detecta enemigos y dispara con ObjectPool.
    /// </summary>
    public class TurretStructure : Script_24_StructureBase
    {
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _firePoint;

        private float _scanTimer;
        private float _shotTimer;
        private Script_13_EnemyBase _currentTarget;

        protected override void Update()
        {
            base.Update();
            if (_data == null)
                return;

            _scanTimer -= Time.deltaTime;
            _shotTimer -= Time.deltaTime;

            if (_scanTimer <= 0f)
            {
                _scanTimer = 0.5f;
                _currentTarget = FindNearestEnemy(_data.range);
            }

            if (_currentTarget == null)
                return;

            Vector2 toTarget = _currentTarget.transform.position - (_pivot != null ? _pivot.position : transform.position);
            float angle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            Transform rotateTarget = _pivot != null ? _pivot : transform;
            rotateTarget.rotation = Quaternion.RotateTowards(rotateTarget.rotation, Quaternion.Euler(0f, 0f, angle), 360f * Time.deltaTime);

            if (_shotTimer <= 0f)
            {
                _shotTimer = Mathf.Max(0.05f, _data.fireRate);
                Fire(toTarget.normalized);
            }
        }

        private Script_13_EnemyBase FindNearestEnemy(float radius)
        {
            Collider2D[] candidates = Physics2D.OverlapCircleAll(transform.position, radius, LayerMask.GetMask("Enemy"));
            Script_13_EnemyBase best = null;
            float nearest = float.MaxValue;
            foreach (Collider2D col in candidates)
            {
                if (!col.TryGetComponent(out Script_13_EnemyBase enemy))
                    continue;

                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < nearest)
                {
                    nearest = dist;
                    best = enemy;
                }
            }

            return best;
        }

        private void Fire(Vector2 direction)
        {
            if (Script_04_ObjectPool.Instance == null)
                return;

            GameObject proj = Script_04_ObjectPool.Instance.Get(_data.projectilePoolKey);
            if (proj == null)
                return;

            proj.transform.position = _firePoint != null ? _firePoint.position : transform.position;
            if (proj.TryGetComponent(out Script_19_Projectile projectile))
                projectile.Launch(direction);
        }
    }
}
