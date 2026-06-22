using MutationSwarm.Combat;
using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MutationSwarm.Entities
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Script_12_PlayerStats))]
    [DisallowMultipleComponent]
    public class Script_11_PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private float _jumpForce = 10f;

        [Header("Dash")]
        [SerializeField] private float _dashForce    = 14f;
        [SerializeField] private float _dashDuration = 0.18f;
        [SerializeField] private float _dashCooldown = 1.2f;

        [Header("Ground Check")]
        [SerializeField] private LayerMask _groundLayers = ~0;
        [SerializeField] private float _groundCheckRadius = 0.12f;

        [Header("Speed Upgrades")]
        [SerializeField] private float _speedBonusPerTier = 0.35f;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private PlayerSpriteAnimator _spriteAnimator;

        [Header("Combat")]
        [SerializeField] private Script_20_WeaponBase _primaryWeapon;
        [SerializeField] private Script_20_WeaponBase _secondaryWeapon;

        [Header("Gun Visual")]
        [SerializeField] private Transform _gunPivot;

        private Script_12_PlayerStats _stats;
        private float _moveX;
        private Vector2 _aimDir = Vector2.right;
        private int _speedTier;
        private bool _isDead;
        private bool _pendingJump;
        private bool _isGrounded;
        private bool _isDashing;
        private float _dashEndTime;
        private float _dashNextAllowed;
        private Vector2 _dashDirection;
        private SpriteRenderer _gunRenderer;
        private readonly Collider2D[] _groundBuffer = new Collider2D[4];

        public bool IsDead => _isDead;
        public int SpeedTier => _speedTier;

        public void SetPrimaryWeapon(Script_20_WeaponBase weapon) => _primaryWeapon = weapon;
        public void IncrementSpeedTier() => _speedTier++;

        private float EffectiveMoveSpeed =>
            (_stats != null ? _stats.MoveSpeed : 4.5f) + _speedBonusPerTier * _speedTier;

        private void Reset()
        {
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponentInChildren<Animator>();
            _spriteAnimator = GetComponent<PlayerSpriteAnimator>();
        }

        private void Awake()
        {
            _stats = GetComponent<Script_12_PlayerStats>();
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_animator == null) _animator = GetComponentInChildren<Animator>();
            if (_animator != null && _animator.runtimeAnimatorController == null) _animator = null;
            if (_spriteAnimator == null) _spriteAnimator = GetComponent<PlayerSpriteAnimator>();
            if (_gunPivot == null) _gunPivot = transform.Find("GunPivot");
            if (_gunPivot != null) _gunRenderer = _gunPivot.GetComponent<SpriteRenderer>();

            _stats.OnDeath += HandleDeath;
        }

        private void OnDestroy()
        {
            if (_stats != null)
                _stats.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (_isDead)
            {
                _animator?.SetBool("IsMoving", false);
                return;
            }

            if (ShouldBlockWorldInput())
            {
                _moveX = 0f;
                _animator?.SetBool("IsMoving", false);
                _spriteAnimator?.SetMovement(_aimDir, false);
                return;
            }

            _moveX = ReadHorizontalInput();

            // Aim toward mouse cursor in world space
            _aimDir = GetWorldAimDirection();
            RotateGunPivot(_aimDir);

            // Sprite: airborne state feeds jump animation; then idle/walk
            Vector2 facing = Mathf.Abs(_moveX) > 0.01f ? new Vector2(_moveX, 0f) : _aimDir;
            bool moving = Mathf.Abs(_moveX) > 0.01f;
            _spriteAnimator?.SetAirborne(!_isGrounded);
            _spriteAnimator?.SetMovement(facing, moving);
            _animator?.SetBool("IsMoving", moving);

            // Jump
            if (IsJumpPressed() && _isGrounded && !_isDashing)
                _pendingJump = true;

            // Dash
            if (IsDashPressed() && !_isDashing && Time.time >= _dashNextAllowed)
                StartDash();

            // Fire: hold left click to shoot at weapon's fire rate
            if (IsFireHeld() && !_isDashing)
            {
                _primaryWeapon?.Fire(_aimDir);
                _spriteAnimator?.TriggerAttack();
            }

            if (WasSecondaryPressed())
                _secondaryWeapon?.Fire(_aimDir);
        }

        private void FixedUpdate()
        {
            if (_rb == null) return;

            CheckGround();

            if (_isDead || ShouldBlockWorldInput())
            {
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                return;
            }

            // End dash when duration expires
            if (_isDashing && Time.time >= _dashEndTime)
                EndDash();

            if (_isDashing)
            {
                _rb.linearVelocity = _dashDirection * _dashForce;
                return;
            }

            // Replace horizontal velocity; preserve vertical (gravity + jump)
            _rb.linearVelocity = new Vector2(_moveX * EffectiveMoveSpeed, _rb.linearVelocity.y);

            if (_pendingJump)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
                _pendingJump = false;
            }
        }

        private void StartDash()
        {
            // Dash in movement direction, or aim direction if standing
            _dashDirection = Mathf.Abs(_moveX) > 0.01f
                ? new Vector2(_moveX, 0f).normalized
                : new Vector2(_aimDir.x >= 0 ? 1f : -1f, 0f);

            _isDashing        = true;
            _dashEndTime      = Time.time + _dashDuration;
            _dashNextAllowed  = Time.time + _dashCooldown;

            // Suppress gravity during dash
            _rb.gravityScale = 0f;
            _rb.linearVelocity = _dashDirection * _dashForce;

            _spriteAnimator?.TriggerDash();
        }

        private void EndDash()
        {
            _isDashing       = false;
            _rb.gravityScale = 1f;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x * 0.3f, _rb.linearVelocity.y);
        }

        private void CheckGround()
        {
            // Circle just below feet; skip the player's own colliders
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, -0.08f);
            int count = Physics2D.OverlapCircleNonAlloc(origin, _groundCheckRadius, _groundBuffer, _groundLayers);
            _isGrounded = false;
            for (int i = 0; i < count; i++)
            {
                if (_groundBuffer[i] != null && _groundBuffer[i].transform.root != transform)
                {
                    _isGrounded = true;
                    break;
                }
            }
        }

        private void RotateGunPivot(Vector2 aimDir)
        {
            if (_gunPivot == null) return;
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            _gunPivot.rotation = Quaternion.Euler(0f, 0f, angle);

            // Flip gun sprite vertically when aiming left so it doesn't appear upside-down
            if (_gunRenderer != null)
                _gunRenderer.flipY = aimDir.x < 0f;
        }

        private Vector2 GetWorldAimDirection()
        {
            var mouse = Mouse.current;
            if (mouse == null) return _aimDir;

            Camera cam = Camera.main;
            if (cam == null) return _aimDir;

            Vector2 screenPos = mouse.position.ReadValue();
            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.nearClipPlane));
            Vector2 dir = (Vector2)world - (Vector2)transform.position;
            return dir.sqrMagnitude > 0.001f ? dir.normalized : _aimDir;
        }

        public void ApplyDamage(float amount)
        {
            if (_isDead) return;
            _stats.TakeDamage(amount);
            if (!_isDead)
                _animator?.SetTrigger("Hit");
        }

        private static float ReadHorizontalInput()
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                float ax = gamepad.leftStick.x.ReadValue();
                if (Mathf.Abs(ax) > 0.1f) return ax;
            }

            var k = Keyboard.current;
            if (k == null) return 0f;

            float h = 0f;
            if (k.aKey.isPressed || k.leftArrowKey.isPressed)  h -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) h += 1f;
            return h;
        }

        private static bool IsJumpPressed()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            return (k != null && (k.spaceKey.wasPressedThisFrame ||
                                  k.wKey.wasPressedThisFrame ||
                                  k.upArrowKey.wasPressedThisFrame)) ||
                   (g != null && g.buttonSouth.wasPressedThisFrame);
        }

        private static bool IsDashPressed()
        {
            var k = Keyboard.current;
            var g = Gamepad.current;
            return (k != null && (k.leftShiftKey.wasPressedThisFrame || k.rightShiftKey.wasPressedThisFrame)) ||
                   (g != null && g.buttonEast.wasPressedThisFrame);
        }

        private static bool IsFireHeld()
        {
            var m = Mouse.current;
            var g = Gamepad.current;
            return (m != null && m.leftButton.isPressed) ||
                   (g != null && g.rightTrigger.ReadValue() > 0.5f);
        }

        private static bool WasSecondaryPressed()
        {
            var m = Mouse.current;
            var g = Gamepad.current;
            return (m != null && m.rightButton.wasPressedThisFrame) ||
                   (g != null && g.leftTrigger.wasPressedThisFrame);
        }

        private static bool ShouldBlockWorldInput()
        {
            if (Script_39_WeaponShopManager.Instance != null && Script_39_WeaponShopManager.Instance.IsShopOpen)
                return true;
            return Script_01_GameManager.Instance != null && Script_01_GameManager.Instance.IsGameplayFrozen;
        }

        private void HandleDeath()
        {
            if (_isDead) return;
            _isDead = true;
            _isDashing = false;
            if (_rb != null)
            {
                _rb.gravityScale = 1f;
                _rb.linearVelocity = Vector2.zero;
            }
            _animator?.SetTrigger("Die");
            _spriteAnimator?.TriggerDeath();
            Script_01_GameManager.Instance?.OnPlayerDiedHandler(0);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.08f, _groundCheckRadius);
        }
    }
}
