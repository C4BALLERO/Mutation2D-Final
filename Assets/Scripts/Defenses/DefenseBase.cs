using UnityEngine;

namespace MutationSwarm
{
    public enum DefenseType { Barricade, Turret, Mine }

    public class DefenseBase : MonoBehaviour
    {
        public DefenseType Type;
        public float MaxHp = 200f;

        float _hp;
        float _turretCd;
        bool  _destroyed;
        public bool IsDestroyed => _destroyed;

        public void Init(DefenseType type)
        {
            Type = type;
            MaxHp = type switch { DefenseType.Barricade => 200f, DefenseType.Turret => 100f, _ => 50f };
            _hp = MaxHp;
            tag = "Defense";

            // Mine: no collider blocking — use trigger
            if (type == DefenseType.Mine)
            {
                var col = GetComponent<Collider2D>();
                if (col) col.isTrigger = true;
            }
        }

        void Update()
        {
            if (GameManager.Instance == null) return;
            if (_destroyed || GameManager.Instance.Phase != GamePhase.Playing) return;

            if (Type == DefenseType.Turret) UpdateTurret();
            if (Type == DefenseType.Mine)   CheckMine();
        }

        void UpdateTurret()
        {
            _turretCd -= Time.deltaTime;
            if (_turretCd > 0f) return;
            if (WaveManager.Instance == null) return;

            EnemyBase nearest = null;
            float nearDist = 8f;
            foreach (var e in WaveManager.Instance.ActiveEnemies)
            {
                if (e == null) continue;
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d < nearDist) { nearDist = d; nearest = e; }
            }
            if (nearest == null) return;

            Vector2 dir = ((Vector2)(nearest.transform.position - transform.position)).normalized;
            var go = new GameObject("TurretBullet");
            go.transform.position = transform.position + (Vector3)(dir * 0.6f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Instance.BulletSprite;
            sr.color  = new Color(1f, 1f, 0.3f);
            sr.sortingOrder = 8;
            // Sin este collider trigger la bala atravesaba a los enemigos sin dañarlos.
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;
            var b = go.AddComponent<Bullet>();
            b.Init(dir, 12f, 15f, 0, false, fromPlayer: true);
            _turretCd = 1f;
            ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f,1f,0.3f), 2, 3f);
        }

        void CheckMine()
        {
            if (WaveManager.Instance == null) return;
            foreach (var e in WaveManager.Instance.ActiveEnemies)
            {
                if (Vector2.Distance(transform.position, e.transform.position) < 1.2f)
                {
                    Explode();
                    return;
                }
            }
        }

        void Explode()
        {
            ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f,0.5f,0f), 16, 8f);
            CameraFollow.Instance?.Shake(0.35f, 0.2f);
            if (WaveManager.Instance != null)
            {
                foreach (var e in WaveManager.Instance.ActiveEnemies.ToArray())
                {
                    float d = Vector2.Distance(transform.position, e.transform.position);
                    if (d < 3f) e.TakeDamage(60f * (1f - d / 3f));
                }
            }
            _destroyed = true;
            Destroy(gameObject);
        }

        public void TakeDamage(float amount)
        {
            _hp -= amount;
            if (_hp <= 0f && !_destroyed)
            {
                _destroyed = true;
                if (WaveManager.Instance != null) WaveManager.Instance.CurrentStats.defKilled++;
                ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f,0.3f,0f), 10, 5f);
                Destroy(gameObject);
            }
        }

        public float HpRatio => _hp / MaxHp;
    }
}
