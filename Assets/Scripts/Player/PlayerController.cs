using UnityEngine;

namespace MutationSwarm
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement")]
        public float moveSpeed    = 7f;
        public float jumpForce    = 14f;
        public float doubleJumpForce = 12f;
        public float dashSpeed    = 18f;
        public float dashDuration = 0.23f;
        public float dashCooldown = 0.65f;

        public bool  IsDashing   { get; private set; }
        public int   Facing      { get; private set; } = 1;
        public bool  OnGround    { get; private set; }
        public float DashCdRatio => Mathf.Clamp01(1f - _dashCd / dashCooldown);

        Rigidbody2D _rb;
        int _jumpCount;
        float _dashCd, _dashTimer;
        float _dashVx;
        bool _regenActive;
        float _regenTick;
        int _electricCounter;

        const float HIGH_Y = 2f;
        WaveStats _waveStats;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            if (WaveManager.Instance != null)
                _waveStats = WaveManager.Instance.CurrentStats;
        }

        void Update()
        {
            if (GameManager.Instance == null) return;
            var phase = GameManager.Instance.Phase;

            // Global keys (work regardless of movement state)
            if (Input.GetKeyDown(KeyCode.Escape) && (phase == GamePhase.Playing || phase == GamePhase.Paused))
                GameManager.Instance.TogglePause();
            if (phase == GamePhase.Dead && Input.GetKeyDown(KeyCode.R))
                GameManager.Instance.Restart();
            if (Input.GetKeyDown(KeyCode.B) && (phase == GamePhase.Playing || phase == GamePhase.Building))
                GameManager.Instance.ToggleBuild();

            // Movement / actions only while actively playing
            if (phase != GamePhase.Playing) return;

            // Fury / Overdrive: unleash when the meter is full.
            if (Input.GetKeyDown(KeyCode.F) && PlayerStats.Instance != null && PlayerStats.Instance.CanOverdrive)
                PlayerStats.Instance.ActivateOverdrive();

            if (WaveManager.Instance != null) _waveStats = WaveManager.Instance.CurrentStats;

            HandleRegen();
            HandleDash();
            HandleHorizontal();
            HandleJump();

            if (_waveStats != null)
            {
                if (transform.position.y > HIGH_Y) _waveStats.timeHigh += Time.deltaTime;
                _waveStats.timeTotal += Time.deltaTime;
            }
        }

        void HandleHorizontal()
        {
            if (IsDashing) return;
            float h = 0f;
            if (Input.GetKey(KeyCode.A)) h = -1f;
            if (Input.GetKey(KeyCode.D)) h =  1f;
            if (h != 0f) Facing = h > 0 ? 1 : -1;
            float spd = moveSpeed * (PlayerStats.Instance != null && PlayerStats.Instance.Overdrive ? 1.4f : 1f);
            _rb.linearVelocity = new Vector2(h * spd, _rb.linearVelocity.y);
        }

        void HandleJump()
        {
            if (IsDashing) return;
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                if (OnGround)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
                    _jumpCount = 1; OnGround = false;
                }
                else if (_jumpCount < 2)
                {
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, doubleJumpForce);
                    _jumpCount++;
                    ParticleManager.Instance?.SpawnBurst(transform.position, Color.cyan, 6, 3f);
                }
            }
        }

        void HandleDash()
        {
            if (_dashCd > 0f) _dashCd -= Time.deltaTime;

            if (IsDashing)
            {
                _dashTimer -= Time.deltaTime;
                _rb.linearVelocity = new Vector2(_dashVx, 0f);

                if (PlayerStats.Instance.HasUpgrade("dashExplosive") && Time.frameCount % 3 == 0)
                    ParticleManager.Instance?.SpawnBurst(transform.position, new Color(1f,0.5f,0f), 5, 4f);

                if (_dashTimer <= 0f) { IsDashing = false; _rb.linearVelocity = Vector2.zero; }
                return;
            }

            if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && _dashCd <= 0f)
            {
                float dir = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : Facing;
                IsDashing  = true;
                _dashTimer = dashDuration;
                _dashCd    = dashCooldown;
                _dashVx    = dir * dashSpeed;
                _rb.linearVelocity = new Vector2(_dashVx, 0f);
                _waveStats.dashUsed++;
                ParticleManager.Instance?.SpawnBurst(transform.position, new Color(0.5f, 0.8f, 1f), 8, 5f);
            }
        }

        void HandleRegen()
        {
            if (!PlayerStats.Instance.HasUpgrade("regen")) return;
            _regenTick += Time.deltaTime;
            if (_regenTick >= 1f) { _regenTick = 0f; PlayerStats.Instance.Heal(2f); }
        }

        static bool IsGroundLayer(GameObject go)
        {
            int l = go.layer;
            return l == LayerMask.NameToLayer("Ground") || l == LayerMask.NameToLayer("Platform");
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            if (IsGroundLayer(col.gameObject))
            {
                foreach (var cp in col.contacts)
                    if (cp.normal.y > 0.5f) { OnGround = true; _jumpCount = 0; return; }
            }
        }

        void OnCollisionExit2D(Collision2D col)
        {
            if (IsGroundLayer(col.gameObject))
                OnGround = false;
        }
    }
}
