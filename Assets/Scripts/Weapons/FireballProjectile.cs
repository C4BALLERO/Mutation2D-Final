using UnityEngine;

namespace MutationSwarm
{
    public class FireballProjectile : MonoBehaviour
    {
        float _speed, _dmg, _life = 3.5f;
        Vector2 _dir;

        public void Init(Vector2 dir, float speed, float dmg)
        {
            _dir = dir.normalized;
            _speed = speed;
            _dmg = dmg;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Playing) return;

            transform.Translate(_dir * _speed * Time.deltaTime, Space.World);
            _life -= Time.deltaTime;

            // Pulso visual de llama
            float pulse = 1f + 0.18f * Mathf.Sin(Time.time * 12f);
            transform.localScale = Vector3.one * 0.55f * pulse;

            if (_life <= 0f) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                if (PlayerStats.Instance != null) PlayerStats.Instance.TakeDamage(_dmg);
                CameraFollow.Instance?.Shake(0.28f, 0.22f);
                ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f, 0.4f, 0.1f), 10, 5f);
                Destroy(gameObject);
                return;
            }

            int gl = col.gameObject.layer;
            if (gl == LayerMask.NameToLayer("Ground") || gl == LayerMask.NameToLayer("Platform"))
            {
                ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f, 0.35f, 0.05f), 6, 4f);
                Destroy(gameObject);
            }
        }
    }
}
