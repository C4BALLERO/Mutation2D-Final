using System.Collections.Generic;
using UnityEngine;

namespace MutationSwarm
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBase : MonoBehaviour
    {
        public GeneType Gene { get; private set; }
        public GeneType[] ExtraGenes { get; private set; } = {};
        public float ArmorFactor { get; private set; } = 1f;
        public bool  IsBoss { get; private set; }
        public static GameObject FireballPrefab;

        float _hp, _maxHp, _spd, _dmg;
        bool  _flies, _slow, _leavesPoison, _spiny, _isCorrupt;

        Rigidbody2D _rb;
        float _attackCd;
        float _wiggle;
        SpriteRenderer _sr;
        Color _baseColor;
        float _flashTimer;
        bool _initialized;

        // Animation state (read by EnemyVisual)
        public int  CreatureIndex { get; private set; }
        public bool IsDying       { get; private set; }
        public bool IsAttacking => _atkAnim > 0f;
        float _atkAnim;
        float _shootCd;

        // Index order matches EnemyVisual.creatures: 0=Dino, 1=Mono, 2=enemy3, 3=Diablito
        static int CreatureForGene(GeneType g) => g switch
        {
            GeneType.Poison  => 0, // Dino
            GeneType.Speed   => 1, // Mono
            GeneType.Spiny   => 2, // enemy3
            GeneType.Armored => 0, // Dino (tanky)
            GeneType.Psychic => 3, // Diablito (flying)
            GeneType.Corrupt => 2, // enemy3
            _                => 0,
        };

        void Awake() => _rb = GetComponent<Rigidbody2D>();

        public void Init(GeneType gene, GeneType[] extra, float hp, float spd, float dmg,
                         float armorFactor, bool flies, bool slow, bool leavesPoison, bool spiny, bool corrupt)
        {
            Gene = gene; ExtraGenes = extra;
            CreatureIndex = CreatureForGene(gene);
            _hp = _maxHp = hp; _spd = spd; _dmg = dmg;
            ArmorFactor = armorFactor; _flies = flies; _slow = slow;
            _leavesPoison = leavesPoison; _spiny = spiny; _isCorrupt = corrupt;

            _baseColor = GeneData.GetColor(gene);
            _sr = GetComponentInChildren<SpriteRenderer>();
            if (_sr) _sr.color = _baseColor;

            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_flies)
            {
                _rb.gravityScale = 0f;
                _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
            }
            else
            {
                _rb.gravityScale = 3f;
                _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
            }

            _wiggle = Random.value * Mathf.PI * 2f;
            _shootCd = Random.Range(1f, 2.5f);
            _initialized = true;
        }

        // Turns a freshly-Init'd enemy into a boss: bigger, tankier visuals, ranged attacks.
        public void MakeBoss()
        {
            IsBoss = true;
            _shootCd = 2f;
            if (_sr != null) _sr.color = Color.Lerp(_baseColor, new Color(0.8f, 0.1f, 0.5f), 0.5f);
        }

        public const float BossScale = 2.7f;

        void Update()
        {
            if (!_initialized || _rb == null) return;
            if (IsDying) return;
            if (GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Playing) return;
            if (PlayerController.Instance == null) return;
            if (PlayerStats.Instance == null) return;

            _wiggle += Time.deltaTime * 3f;
            _attackCd = Mathf.Max(0f, _attackCd - Time.deltaTime);
            _atkAnim  = Mathf.Max(0f, _atkAnim - Time.deltaTime);

            if (_flashTimer > 0f)
            {
                _flashTimer -= Time.deltaTime;
                if (_sr) _sr.color = _flashTimer > 0f ? Color.white : _baseColor;
            }

            var target = PlayerController.Instance.transform.position;
            Vector2 dir = ((Vector2)(target - transform.position)).normalized;

            if (_flies)
            {
                _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, dir * _spd, 0.08f * Time.deltaTime * 60f);
                _shootCd -= Time.deltaTime;
                if (_shootCd <= 0f)
                {
                    _shootCd = Random.Range(1.8f, 2.8f);
                    ShootFireball();
                }
            }
            else
            {
                float vx = dir.x > 0f ? _spd : -_spd;
                _rb.linearVelocity = new Vector2(vx, _rb.linearVelocity.y);
            }

            // Bosses lob fireballs as they march toward the player.
            if (IsBoss)
            {
                _shootCd -= Time.deltaTime;
                if (_shootCd <= 0f) { _shootCd = Random.Range(1.4f, 2.4f); ShootFireball(); }
            }

            if (_rb.linearVelocity.x != 0)
            {
                float facing = _rb.linearVelocity.x > 0 ? 1f : -1f;
                if (CreatureIndex == 2) facing = -facing; // enemy3 sprite está dibujado al revés
                float sc = IsBoss ? BossScale : 1f;
                transform.localScale = new Vector3(facing * sc, sc, 1);
            }

            // Daño continuo al contacto — todos los enemigos dañan igual que el Diablito
            float contactRange = (_flies ? 3.5f : 1.1f) * (IsBoss ? BossScale : 1f);
            var pc = PlayerController.Instance;
            if (pc != null && Vector2.Distance(transform.position, pc.transform.position) < contactRange)
            {
                float dmgRate = _dmg * 1.5f;
                if (_spiny && pc.IsDashing) dmgRate += 8f;
                PlayerStats.Instance.TakeDamage(dmgRate * Time.deltaTime);
                // Mutación SANGRE TÓXICA: el enemigo se daña al tocarte.
                if (PlayerStats.Instance.HasMutation("toxicBlood")) TakeDamage(20f * Time.deltaTime);
                if (Gene == GeneType.Speed && WaveManager.Instance != null) WaveManager.Instance.CurrentStats.contactHitsFromSpeed++;
                _atkAnim = 0.3f;

                if (_attackCd <= 0f)
                {
                    CameraFollow.Instance?.Shake(0.18f, 0.14f);
                    AudioManager.Instance?.PlayGrowl();
                    _attackCd = 0.35f;
                }
            }
        }

        public void TakeDamage(float amount)
        {
            _hp -= amount;
            _flashTimer = 0.07f;
            if (_hp <= 0f) Die();
        }

        void ShootFireball()
        {
            // Lazy-load: el campo estático se pierde en builds; lo buscamos en la escena si es null
            if (FireballPrefab == null)
            {
                foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                    if (go.name == "FireballPrefab") { FireballPrefab = go; break; }
            }
            if (FireballPrefab == null || PlayerController.Instance == null) return;
            var projContainer = GameObject.Find("Projectiles");
            var fb = Instantiate(FireballPrefab, transform.position, Quaternion.identity,
                                 projContainer != null ? projContainer.transform : null);
            fb.SetActive(true);
            Vector2 toPlayer = ((Vector2)(PlayerController.Instance.transform.position - transform.position)).normalized;
            fb.GetComponent<FireballProjectile>()?.Init(toPlayer, 5f, _dmg * 0.8f);
            ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f, 0.5f, 0.1f), 4, 3f);
        }

        void Die()
        {
            if (IsDying) return;
            IsDying = true;

            AudioManager.Instance?.PlayEnemyDeath();
            PlayerStats.Instance?.OnEnemyKilled(IsBoss);
            WaveManager.Instance?.RemoveEnemy(this);
            ParticleManager.Instance?.SpawnBurst(transform.position, _baseColor, IsBoss ? 40 : 12, IsBoss ? 12f : 5f);

            // Mutación VOLÁTIL: el cadáver explota dañando enemigos cercanos.
            if (PlayerStats.Instance != null && PlayerStats.Instance.HasMutation("volatile") && WaveManager.Instance != null)
            {
                ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f, 0.6f, 0.1f), 14, 7f);
                foreach (var e in WaveManager.Instance.ActiveEnemies.ToArray())
                {
                    if (e == null || e == this) continue;
                    if (Vector2.Distance(transform.position, e.transform.position) < 2.5f) e.TakeDamage(35f);
                }
            }

            var pickup = new GameObject("DNAPickup");
            pickup.transform.position = transform.position;
            var dp = pickup.AddComponent<DnaPickup>();
            dp.Amount = IsBoss ? Random.Range(120, 160) : _isCorrupt ? Random.Range(15, 25) : Random.Range(3, 8);

            // Mutágeno: los jefes siempre lo sueltan, los corruptos a veces.
            if (IsBoss || (_isCorrupt && Random.value < 0.5f))
            {
                var mg = new GameObject("MutagenPickup");
                mg.transform.position = transform.position + Vector3.up * 0.3f;
                mg.AddComponent<MutagenPickup>().Amount = IsBoss ? 3 : 1;
            }

            if (_leavesPoison)
            {
                for (int i = 0; i < 5; i++)
                {
                    var cloud = new GameObject("PoisonCloud");
                    cloud.transform.position = transform.position + (Vector3)(Random.insideUnitCircle * 1.2f);
                    cloud.AddComponent<PoisonCloud>();
                }
            }

            if (_isCorrupt || _slow)
                ParticleManager.Instance?.SpawnBurst(transform.position, new Color(0.8f,0.3f,1f), 8, 6f);

            // Stop interacting and freeze in place while the death animation plays.
            foreach (var col in GetComponents<Collider2D>()) col.enabled = false;
            if (_rb != null) { _rb.linearVelocity = Vector2.zero; _rb.gravityScale = 0f; }

            // Destroy after the death animation has time to play.
            bool hasDeathAnim = GetComponentInChildren<EnemyVisual>()?.Current?.death is { Length: > 0 };
            Destroy(gameObject, hasDeathAnim ? 0.7f : 0f);
        }

        public float HpRatio => _hp / _maxHp;

        static bool IsGroundLayer(GameObject go)
        {
            int l = go.layer;
            return l == LayerMask.NameToLayer("Ground") || l == LayerMask.NameToLayer("Platform");
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (_spiny && col.gameObject.layer == LayerMask.NameToLayer("Defense"))
            {
                var def = col.gameObject.GetComponent<DefenseBase>();
                if (def != null)
                {
                    def.TakeDamage(_dmg * 0.5f * Time.deltaTime * 30f);
                    if (WaveManager.Instance != null) WaveManager.Instance.CurrentStats.defKilled += def.IsDestroyed ? 1 : 0;
                }
            }
        }
    }
}
