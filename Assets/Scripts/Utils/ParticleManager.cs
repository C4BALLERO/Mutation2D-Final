using System.Collections;
using UnityEngine;

namespace MutationSwarm
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SpawnBurst(Vector3 pos, Color color, int count, float speed)
        {
            StartCoroutine(SpawnParticles(pos, color, count, speed));
        }

        IEnumerator SpawnParticles(Vector3 pos, Color color, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Particle");
                go.transform.position = pos;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite       = SpriteFactory.Instance?.PixelSprite;
                sr.color        = color;
                sr.sortingOrder = 10;
                float s = Random.Range(0.05f, 0.18f);
                go.transform.localScale = Vector3.one * s;

                var rb = go.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1.5f;
                Vector2 vel = Random.insideUnitCircle.normalized * speed * Random.Range(0.5f, 1f);
                vel.y = Mathf.Abs(vel.y) * 0.5f + speed * 0.3f;
                rb.linearVelocity = vel;

                var pa = go.AddComponent<ParticleActor>();
                pa.Init(sr, Random.Range(0.3f, 0.8f));
            }
            yield break;
        }
    }

    public class ParticleActor : MonoBehaviour
    {
        SpriteRenderer _sr;
        float _life, _maxLife;

        public void Init(SpriteRenderer sr, float life)
        {
            _sr = sr; _life = _maxLife = life;
        }

        void Update()
        {
            _life -= Time.deltaTime;
            if (_sr && _life > 0f)
            {
                var c = _sr.color;
                _sr.color = new Color(c.r, c.g, c.b, _life / _maxLife);
            }
            if (_life <= 0f) Destroy(gameObject);
        }
    }
}
