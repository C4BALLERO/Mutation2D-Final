using UnityEngine;

namespace MutationSwarm.Entities
{
    [DisallowMultipleComponent]
    public sealed class PlayerSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Frames")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private Sprite[] jumpFrames;    // Jump_Player.png
        [SerializeField] private Sprite[] attackFrames;  // Posicion_Arma.png
        [SerializeField] private Sprite[] dashFrames;    // Dahs_Player.png

        [Header("FPS")]
        [SerializeField] private float idleFps    = 10f;
        [SerializeField] private float walkFps    = 14f;
        [SerializeField] private float jumpFps    = 18f;
        [SerializeField] private float attackFps  = 18f;
        [SerializeField] private float dashFps    = 24f;
        [SerializeField] private bool  faceWithFlipX = true;

        private enum AnimState { Idle, Walk, Jump, Attack, Dash, Dead }

        private AnimState _state = AnimState.Idle;
        private int   _frame;
        private float _nextFrameAt;
        private Vector2 _lastFacing = Vector2.right;

        private void Reset() => spriteRenderer = GetComponent<SpriteRenderer>();

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            ApplyCurrentFrame();
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void SetMovement(Vector2 facing, bool moving)
        {
            if (_state is AnimState.Dead or AnimState.Dash or AnimState.Attack or AnimState.Jump) return;
            if (facing.sqrMagnitude > 0.001f) _lastFacing = facing.normalized;
            _state = moving ? AnimState.Walk : AnimState.Idle;
        }

        /// <summary>Call every frame with the current airborne state.</summary>
        public void SetAirborne(bool airborne)
        {
            if (_state is AnimState.Dead or AnimState.Dash or AnimState.Attack) return;
            if (airborne && _state != AnimState.Jump)
            {
                SetState(AnimState.Jump);
            }
            else if (!airborne && _state == AnimState.Jump)
            {
                SetState(AnimState.Idle);
            }
        }

        public void TriggerAttack()
        {
            if (_state is AnimState.Dead or AnimState.Dash) return;
            if (_state == AnimState.Attack) return; // don't restart while already playing
            SetState(AnimState.Attack);
        }

        public void TriggerDash()
        {
            if (_state == AnimState.Dead) return;
            SetState(AnimState.Dash);
        }

        public void TriggerDeath()
        {
            SetState(AnimState.Dead);
        }

        public void TriggerRevive()
        {
            SetState(AnimState.Idle);
        }

        // ── Internal ─────────────────────────────────────────────────────────

        private void SetState(AnimState next)
        {
            _state = next;
            _frame = 0;
            _nextFrameAt = 0f;
            ApplyCurrentFrame();
        }

        private void Update()
        {
            float now = Time.time;
            if (now < _nextFrameAt) return;
            _nextFrameAt = now + 1f / Mathf.Max(1f, GetStateFps());

            switch (_state)
            {
                case AnimState.Idle:
                    _frame = HasFrames(idleFrames) ? (_frame + 1) % idleFrames.Length : 0;
                    break;

                case AnimState.Walk:
                    _frame = HasFrames(walkFrames) ? (_frame + 1) % walkFrames.Length : 0;
                    break;

                case AnimState.Jump:
                    _frame = HasFrames(jumpFrames) ? (_frame + 1) % jumpFrames.Length : 0;
                    break;

                case AnimState.Attack:
                    _frame++;
                    if (!HasFrames(attackFrames) || _frame >= attackFrames.Length)
                        SetState(AnimState.Idle);
                    break;

                case AnimState.Dash:
                    _frame++;
                    if (!HasFrames(dashFrames) || _frame >= dashFrames.Length)
                        SetState(AnimState.Idle);
                    break;

                case AnimState.Dead:
                    if (HasFrames(idleFrames) && _frame < idleFrames.Length - 1)
                        _frame++;
                    break;
            }

            ApplyCurrentFrame();
        }

        private float GetStateFps() => _state switch
        {
            AnimState.Walk   => walkFps,
            AnimState.Jump   => jumpFps,
            AnimState.Attack => attackFps,
            AnimState.Dash   => dashFps,
            _                => idleFps
        };

        private void ApplyCurrentFrame()
        {
            if (spriteRenderer == null) return;
            Sprite[] frames = GetStateFrames();
            if (frames == null || frames.Length == 0) return;
            spriteRenderer.sprite = frames[Mathf.Clamp(_frame, 0, frames.Length - 1)];
            UpdateFacing();
        }

        private Sprite[] GetStateFrames() => _state switch
        {
            AnimState.Jump   when HasFrames(jumpFrames)   => jumpFrames,
            AnimState.Attack when HasFrames(attackFrames) => attackFrames,
            AnimState.Dash   when HasFrames(dashFrames)   => dashFrames,
            AnimState.Walk   when HasFrames(walkFrames)   => walkFrames,
            _ when HasFrames(idleFrames)                  => idleFrames,
            _                                             => walkFrames
        };

        private static bool HasFrames(Sprite[] arr) => arr != null && arr.Length > 0;

        private void UpdateFacing()
        {
            if (spriteRenderer == null || !faceWithFlipX) return;
            if (Mathf.Abs(_lastFacing.x) < 0.001f) return;
            spriteRenderer.flipX = _lastFacing.x < 0f;
        }
    }
}
