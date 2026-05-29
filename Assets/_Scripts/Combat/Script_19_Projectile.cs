using MutationSwarm.Core;
using MutationSwarm.Entities;
using UnityEngine;

namespace MutationSwarm.Combat
{
    /// <summary>
    /// Proyectil pooled con lifetime automático.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Script_19_Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _lifetime = 3f;
        [SerializeField] private float _speed = 12f;
        [SerializeField] private float _damage = 10f;
        [SerializeField] private string _poolKey = "Projectile_Basic";

        public float PoolLifetime => _lifetime;

        public void OnDespawn()
        {
            if (TryGetComponent(out Rigidbody2D rb))
                rb.linearVelocity = Vector2.zero;
        }

        public void Launch(Vector2 direction)
        {
            if (TryGetComponent(out Rigidbody2D rb))
                rb.linearVelocity = direction.normalized * _speed;
        }

        private void Update()
        {
            if (PoolLifetime > 0f)
            {
                _lifeTimer -= Time.deltaTime;
                if (_lifeTimer <= 0f && Script_04_ObjectPool.Instance != null)
                    Script_04_ObjectPool.Instance.Return(_poolKey, gameObject);
            }
        }

        private float _lifeTimer;

        public void OnSpawn()
        {
            _lifeTimer = _lifetime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out Script_13_EnemyBase enemy))
                return;

            enemy.TakeDamage(_damage);
            if (Script_04_ObjectPool.Instance != null)
                Script_04_ObjectPool.Instance.Return(_poolKey, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
