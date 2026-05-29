using System.Collections;
using MutationSwarm.Combat;
using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MutationSwarm.Entities
{
    /// <summary>
    /// Movimiento 2D: salto, dash, wall jump y combate por jugador (0–3).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Script_12_PlayerStats))]
    public class Script_11_PlayerController : MonoBehaviour
    {
        [Header("Jugador")]
        [SerializeField] private int _playerIndex;
        [SerializeField] private Script_20_WeaponBase _primaryWeapon;
        [SerializeField] private Script_20_WeaponBase _secondaryWeapon;

        [Header("Referencias")]
        [SerializeField] private CapsuleCollider2D _groundTrigger;
        [SerializeField] private Transform _groundCheckPoint;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private LayerMask _wallMask;
        [SerializeField] private ParticleSystem _dashVfx;

        [Header("Movimiento")]
        [SerializeField, Range(0.01f, 0.5f)] private float _moveSmoothing = 0.1f;

        [Header("Salto")]
        [SerializeField] private float _coyoteTime = 0.15f;
        [SerializeField] private float _jumpBufferTime = 0.1f;
        [SerializeField] private float _jumpCutMultiplier = 0.5f;
        [SerializeField] private float _groundCheckRadius = 0.2f;

        [Header("Dash")]
        [SerializeField] private float _dashDuration = 0.15f;

        [Header("Wall Jump")]
        [SerializeField] private Vector2 _wallJumpForce = new(10f, 12f);
        [SerializeField] private float _wallSlideGravityMultiplier = 0.3f;
        [SerializeField] private float _wallRayDistance = 0.65f;
        [SerializeField] private float _wallJumpCooldown = 0.3f;

        private Script_12_PlayerStats _stats;
        private Rigidbody2D _rb;
        private Animator _animator;
        private SpriteRenderer _spriteRenderer;

        private float _baseGravityScale;
        private float _currentHorizontalVelocity;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private float _wallJumpLockTimer;
        private bool _usedDoubleJump;
        private bool _jumpHeldLastFrame;
        private bool _isGrounded;
        private bool _isTouchingWall;
        private bool _isDashing;
        private bool _isInvulnerable;
        private bool _inputEnabled = true;
        private bool _isDead;
        private int _wallDir;

        private Coroutine _deathRoutine;

        public int PlayerIndex => _playerIndex;
        public bool IsInvulnerable => _isInvulnerable;
        public bool IsDead => _isDead;

        private void Awake()
        {
            _stats = GetComponent<Script_12_PlayerStats>();
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _groundTrigger ??= GetComponent<CapsuleCollider2D>();
            _baseGravityScale = _rb.gravityScale;

            _playerIndex = Mathf.Clamp(_playerIndex, 0, 3);
            _stats.OnDeath += HandleDeath;
        }

        private void OnDestroy()
        {
            if (_stats != null)
                _stats.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (!_inputEnabled || _isDead)
            {
                UpdateAnimator();
                return;
            }

            TickTimers();
            UpdateGroundAndWallState();

            Vector2 moveInput = ReadMoveInput();
            HandleHorizontalMovement(moveInput.x);
            HandleJumpInput();
            HandleDashInput(moveInput.x);
            HandleCombatInput();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            if (_isDashing || _isDead)
                return;

            if (ShouldWallSlide())
            {
                _rb.gravityScale = _baseGravityScale * _wallSlideGravityMultiplier;
                if (_rb.linearVelocity.y < -6f)
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -6f);
            }
            else if (!_isDashing)
            {
                _rb.gravityScale = _baseGravityScale;
            }
        }

        /// <summary>
        /// Entrada de daño del jugador para integrar combate y animación de hit.
        /// </summary>
        public void ApplyDamage(float amount)
        {
            if (_isInvulnerable || _isDead)
                return;

            _stats.TakeDamage(amount);
            if (!_isDead)
                _animator.SetTrigger("Hit");
        }

        private void TickTimers()
        {
            if (_coyoteTimer > 0f)
                _coyoteTimer -= Time.deltaTime;
            if (_jumpBufferTimer > 0f)
                _jumpBufferTimer -= Time.deltaTime;
            if (_dashCooldownTimer > 0f)
                _dashCooldownTimer -= Time.deltaTime;
            if (_wallJumpLockTimer > 0f)
                _wallJumpLockTimer -= Time.deltaTime;
            if (_dashTimer > 0f)
                _dashTimer -= Time.deltaTime;
        }

        private Vector2 ReadMoveInput()
        {
            Vector2 keyboardInput = ReadKeyboardMoveInput();
            Vector2 gamepadInput = ReadGamepadMoveInput();
            return gamepadInput.sqrMagnitude > 0.01f ? gamepadInput : keyboardInput;
        }

        private Vector2 ReadKeyboardMoveInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical);
        }

        private Vector2 ReadGamepadMoveInput()
        {
            if (_playerIndex < 0 || _playerIndex >= Gamepad.all.Count)
                return Vector2.zero;

            Gamepad pad = Gamepad.all[_playerIndex];
            return pad.leftStick.ReadValue();
        }

        private void UpdateGroundAndWallState()
        {
            Vector2 origin = _groundCheckPoint != null ? (Vector2)_groundCheckPoint.position : (Vector2)transform.position;
            _isGrounded = Physics2D.OverlapCircle(origin, _groundCheckRadius, _groundMask);
            if (_isGrounded)
            {
                _coyoteTimer = _coyoteTime;
                _usedDoubleJump = false;
            }

            Vector2 rayOrigin = _groundTrigger != null ? _groundTrigger.bounds.center : (Vector2)transform.position;
            bool wallRight = Physics2D.Raycast(rayOrigin, Vector2.right, _wallRayDistance, _wallMask);
            bool wallLeft = Physics2D.Raycast(rayOrigin, Vector2.left, _wallRayDistance, _wallMask);
            _isTouchingWall = wallRight || wallLeft;
            _wallDir = wallRight ? 1 : wallLeft ? -1 : 0;
        }

        private void HandleHorizontalMovement(float inputX)
        {
            if (_isDashing)
                return;

            // Suavizado de aceleración/frenado horizontal.
            _currentHorizontalVelocity = Mathf.Lerp(_currentHorizontalVelocity, inputX * _stats.MoveSpeed, _moveSmoothing);
            _rb.linearVelocity = new Vector2(_currentHorizontalVelocity, _rb.linearVelocity.y);

            if (Mathf.Abs(inputX) > 0.05f)
                _spriteRenderer.flipX = inputX < 0f;
        }

        private void HandleJumpInput()
        {
            bool jumpPressed = IsJumpPressedThisFrame();
            bool jumpHeld = IsJumpHeld();
            bool jumpReleased = _jumpHeldLastFrame && !jumpHeld;
            _jumpHeldLastFrame = jumpHeld;

            if (jumpPressed)
                _jumpBufferTimer = _jumpBufferTime;

            if (_jumpBufferTimer > 0f)
            {
                if (_coyoteTimer > 0f)
                {
                    DoGroundJump();
                    return;
                }

                if (TryWallJump())
                    return;

                if (_stats.HasDoubleJump && !_usedDoubleJump)
                {
                    DoDoubleJump();
                    return;
                }
            }

            // Variable jump height: corta el salto al soltar temprano.
            if (jumpReleased && _rb.linearVelocity.y > 0f)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * _jumpCutMultiplier);
        }

        private void HandleDashInput(float inputX)
        {
            if (!IsDashPressedThisFrame() || _dashCooldownTimer > 0f || _isDashing)
                return;

            Vector2 dashDirection = ResolveDashDirection(inputX);
            StartCoroutine(DashRoutine(dashDirection));
        }

        private Vector2 ResolveDashDirection(float inputX)
        {
            Vector2 gamepadDir = ReadGamepadMoveInput();
            if (gamepadDir.sqrMagnitude > 0.2f)
                return new Vector2(Mathf.Sign(gamepadDir.x == 0f ? inputX : gamepadDir.x), 0f).normalized;

            Vector2 keyboardDir = ReadKeyboardMoveInput();
            if (keyboardDir.sqrMagnitude > 0.2f)
                return new Vector2(Mathf.Sign(keyboardDir.x == 0f ? inputX : keyboardDir.x), 0f).normalized;

            // Mouse+keyboard: si no hay dirección, usa orientación visual actual.
            return _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        private void HandleCombatInput()
        {
            Vector2 fireDirection = _spriteRenderer.flipX ? Vector2.left : Vector2.right;

            if (IsPrimaryFireHeld())
            {
                _primaryWeapon?.Fire(fireDirection);
                _animator.SetTrigger("Attack");
            }

            if (IsSecondaryFireHeld())
                _secondaryWeapon?.Fire(fireDirection);

            // Q/E o Y para cambiar arma principal/secundaria.
            if (Keyboard.current != null && (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
                SwapWeapons();

            if (_playerIndex >= 0 && _playerIndex < Gamepad.all.Count && Gamepad.all[_playerIndex].buttonNorth.wasPressedThisFrame)
                SwapWeapons();
        }

        private bool TryWallJump()
        {
            if (!_stats.HasWallJump || !_isTouchingWall || _wallJumpLockTimer > 0f)
                return false;

            Vector2 force = new(_wallJumpForce.x * -_wallDir, _wallJumpForce.y);
            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(force, ForceMode2D.Impulse);
            _wallJumpLockTimer = _wallJumpCooldown;
            _jumpBufferTimer = 0f;
            _animator.SetTrigger("Jump");
            return true;
        }

        private void DoGroundJump()
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _stats.JumpForce);
            _coyoteTimer = 0f;
            _jumpBufferTimer = 0f;
            _animator.SetTrigger("Jump");
        }

        private void DoDoubleJump()
        {
            _usedDoubleJump = true;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _stats.JumpForce);
            _jumpBufferTimer = 0f;
            _animator.SetTrigger("Jump");
        }

        private bool ShouldWallSlide()
        {
            return _stats.HasWallJump && _isTouchingWall && !_isGrounded && _rb.linearVelocity.y < 0f;
        }

        private IEnumerator DashRoutine(Vector2 direction)
        {
            _isDashing = true;
            _isInvulnerable = true;
            _dashTimer = _dashDuration;
            _dashCooldownTimer = _stats.DashCooldown;
            _animator.SetTrigger("Dash");
            if (_dashVfx != null)
                _dashVfx.Play();

            float previousGravity = _rb.gravityScale;
            _rb.gravityScale = 0f;
            _rb.linearVelocity = direction.normalized * _stats.DashForce;

            while (_dashTimer > 0f)
                yield return null;

            _rb.gravityScale = previousGravity;
            _isInvulnerable = false;
            _isDashing = false;
        }

        private void HandleDeath()
        {
            if (_isDead)
                return;

            _isDead = true;
            _inputEnabled = false;
            _isInvulnerable = false;
            _rb.linearVelocity = Vector2.zero;
            _animator.SetTrigger("Die");

            if (_deathRoutine != null)
                StopCoroutine(_deathRoutine);
            _deathRoutine = StartCoroutine(ReviveWindowRoutine());
        }

        private IEnumerator ReviveWindowRoutine()
        {
            float reviveWindow = 10f;
            while (reviveWindow > 0f)
            {
                // Si otro jugador revive este actor externamente, se aborta el conteo.
                if (!_isDead)
                    yield break;

                reviveWindow -= Time.deltaTime;
                yield return null;
            }

            if (_isDead && Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.OnPlayerDiedHandler(_playerIndex);
        }

        public void Revive(float hpPercent = 0.5f)
        {
            if (!_isDead)
                return;

            _isDead = false;
            _inputEnabled = true;
            _isInvulnerable = true;
            _stats.Heal(_stats.MaxHp * Mathf.Clamp01(hpPercent));
            _rb.linearVelocity = Vector2.zero;

            if (Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.RevivePlayer(_playerIndex);

            StartCoroutine(ClearReviveInvulnerability());
        }

        private IEnumerator ClearReviveInvulnerability()
        {
            yield return new WaitForSeconds(1f);
            _isInvulnerable = false;
        }

        private void SwapWeapons()
        {
            (_primaryWeapon, _secondaryWeapon) = (_secondaryWeapon, _primaryWeapon);
        }

        private bool IsJumpPressedThisFrame()
        {
            bool keyboardPressed = Keyboard.current != null &&
                                   (Keyboard.current.spaceKey.wasPressedThisFrame ||
                                    Keyboard.current.wKey.wasPressedThisFrame ||
                                    Keyboard.current.upArrowKey.wasPressedThisFrame);

            bool gamepadPressed = _playerIndex >= 0 && _playerIndex < Gamepad.all.Count &&
                                  (Gamepad.all[_playerIndex].buttonSouth.wasPressedThisFrame ||
                                   Gamepad.all[_playerIndex].buttonEast.wasPressedThisFrame);

            return keyboardPressed || gamepadPressed;
        }

        private bool IsJumpHeld()
        {
            bool keyboardHeld = Keyboard.current != null &&
                                (Keyboard.current.spaceKey.isPressed ||
                                 Keyboard.current.wKey.isPressed ||
                                 Keyboard.current.upArrowKey.isPressed);

            bool gamepadHeld = _playerIndex >= 0 && _playerIndex < Gamepad.all.Count &&
                               (Gamepad.all[_playerIndex].buttonSouth.isPressed ||
                                Gamepad.all[_playerIndex].buttonEast.isPressed);

            return keyboardHeld || gamepadHeld;
        }

        private bool IsDashPressedThisFrame()
        {
            bool keyboardPressed = Keyboard.current != null &&
                                   (Keyboard.current.leftShiftKey.wasPressedThisFrame ||
                                    Keyboard.current.rightShiftKey.wasPressedThisFrame);

            bool gamepadPressed = _playerIndex >= 0 && _playerIndex < Gamepad.all.Count &&
                                  Gamepad.all[_playerIndex].rightShoulder.wasPressedThisFrame;

            return keyboardPressed || gamepadPressed;
        }

        private bool IsPrimaryFireHeld()
        {
            bool mouseHeld = Mouse.current != null && Mouse.current.leftButton.isPressed;
            bool triggerHeld = _playerIndex >= 0 && _playerIndex < Gamepad.all.Count &&
                               Gamepad.all[_playerIndex].rightTrigger.ReadValue() > 0.2f;
            return mouseHeld || triggerHeld;
        }

        private bool IsSecondaryFireHeld()
        {
            bool mouseHeld = Mouse.current != null && Mouse.current.rightButton.isPressed;
            bool triggerHeld = _playerIndex >= 0 && _playerIndex < Gamepad.all.Count &&
                               Gamepad.all[_playerIndex].leftTrigger.ReadValue() > 0.2f;
            return mouseHeld || triggerHeld;
        }

        private void UpdateAnimator()
        {
            _animator.SetBool("IsGrounded", _isGrounded);
            _animator.SetBool("IsRunning", Mathf.Abs(_rb.linearVelocity.x) > 0.1f && _isGrounded);
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheckPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_groundCheckPoint.position, _groundCheckRadius);
            }

            Vector3 center = _groundTrigger != null ? _groundTrigger.bounds.center : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center, center + Vector3.right * _wallRayDistance);
            Gizmos.DrawLine(center, center + Vector3.left * _wallRayDistance);
        }
    }
}
