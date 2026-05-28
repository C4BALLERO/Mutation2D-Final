using MutationSwarm.Core;
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

        public void OnSpawn() { }
        public void OnDespawn()
        {
            if (TryGetComponent(out Rigidbody2D rb))
                rb.velocity = Vector2.zero;
        }

        public void Launch(Vector2 direction)
        {
            if (TryGetComponent(out Rigidbody2D rb))
                rb.velocity = direction.normalized * _speed;
        }
    }
}
