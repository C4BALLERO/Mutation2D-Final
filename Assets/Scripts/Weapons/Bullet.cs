using UnityEngine;

namespace MutationSwarm
{
    public class Bullet : MonoBehaviour
    {
        float _speed, _dmg, _life = 1.5f;
        int   _piercing, _hitsLeft;
        bool  _isElectric, _fromPlayer;
        Vector2 _dir;

        public void Init(Vector2 dir, float speed, float dmg, int piercing, bool electric, bool fromPlayer)
        {
            _dir = dir; _speed = speed; _dmg = dmg;
            _piercing = piercing; _hitsLeft = 1 + piercing;
            _isElectric = electric; _fromPlayer = fromPlayer;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Color — el sprite ya es dorado; solo re-teñimos las balas eléctricas
            var sr = GetComponent<SpriteRenderer>();
            if (sr) sr.color = electric ? new Color(0.6f, 1f, 1f) : Color.white;
        }

        void Update()
        {
            transform.Translate(_dir * _speed * Time.deltaTime, Space.World);
            _life -= Time.deltaTime;
            if (_life <= 0f || _hitsLeft <= 0) Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            if (_hitsLeft <= 0) return;

            if (_fromPlayer && col.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                var e = col.GetComponent<EnemyBase>();
                if (e == null) return;

                float dmg = _dmg * e.ArmorFactor;
                e.TakeDamage(dmg);
                if (WaveManager.Instance != null) WaveManager.Instance.CurrentStats.bulletsHit++;
                _hitsLeft--;
                ParticleManager.Instance?.SpawnBurst(transform.position, Color.white, 3, 4f);

                if (_isElectric) ChainElectric(e);
                if (_hitsLeft <= 0) Destroy(gameObject);
            }
            else
            {
                int gl = col.gameObject.layer;
                if (gl == LayerMask.NameToLayer("Ground") || gl == LayerMask.NameToLayer("Platform"))
                    Destroy(gameObject);
            }
        }

        void ChainElectric(EnemyBase source)
        {
            if (WaveManager.Instance == null) return;
            int count = 0;
            // Iterate a snapshot: TakeDamage can kill -> RemoveEnemy -> modify the list.
            foreach (var e in WaveManager.Instance.ActiveEnemies.ToArray())
            {
                if (e == null || e == source) continue;
                if (Vector2.Distance(e.transform.position, transform.position) < 4f)
                {
                    e.TakeDamage(8f);
                    ParticleManager.Instance?.SpawnBurst(e.transform.position, new Color(0.5f,1f,1f), 5, 3f);
                    if (++count >= 3) break;
                }
            }
        }
    }
}
