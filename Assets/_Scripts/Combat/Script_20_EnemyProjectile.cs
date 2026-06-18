using MutationSwarm.Entities;
using UnityEngine;

namespace MutationSwarm.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Script_20_EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float _damage   = 8f;
        [SerializeField] private float _speed    = 7f;
        [SerializeField] private float _lifetime = 4f;

        private Rigidbody2D _rb;
        private float _timer;
        private bool _active;

        private void Awake() => _rb = GetComponent<Rigidbody2D>();

        public void Configure(float damage, float speed)
        {
            _damage = damage;
            _speed  = speed;
        }

        public void Launch(Vector2 direction)
        {
            _timer  = _lifetime;
            _active = false;
            _rb.linearVelocity = direction.normalized * _speed;
            Invoke(nameof(Activate), 0.15f);
        }

        private void Activate() => _active = true;

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_active) return;
            if (!other.CompareTag("Player")) return;

            if (other.TryGetComponent(out Script_11_PlayerController pc))
                pc.ApplyDamage(_damage);

            Destroy(gameObject);
        }
    }
}
