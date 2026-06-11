using UnityEngine;

namespace MutationSwarm.Entities
{
    [DisallowMultipleComponent]
    public sealed class EnemySpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Frames")]
        [Tooltip("Optional idle loop. Falls back to walkFrames if empty.")]
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private Sprite[] attackFrames;
        [SerializeField] private Sprite[] deathFrames;

        [Header("FPS")]
        [SerializeField] private float idleFps   = 8f;
        [SerializeField] private float walkFps   = 12f;
        [SerializeField] private float attackFps = 14f;
        [SerializeField] private float deathFps  = 10f;
        [SerializeField] private bool  flipXWithMovement = true;

        private enum State { Idle, Walk, Attack, Dead }

        private State   _state = State.Idle;
        private int     _frame;
        private float   _nextFrameAt;
        private Vector2 _lastFacing = Vector2.right;

        private void Reset() => spriteRenderer = GetComponent<SpriteRenderer>();

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            ApplyFrame();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Call every frame with the current movement direction and state.</summary>
        public void SetMovement(Vector2 facing, bool moving)
        {
            if (_state is State.Dead or State.Attack) return;
            if (facing.sqrMagnitude > 0.001f) _lastFacing = facing.normalized;
            var next = moving ? State.Walk : State.Idle;
            if (next != _state) { _state = next; _frame = 0; }
        }

        public void TriggerAttack()
        {
            if (_state == State.Dead || _state == State.Attack) return;
            _state = State.Attack;
            _frame = 0;
            _nextFrameAt = 0f;
            ApplyFrame();
        }

        public void TriggerDeath()
        {
            _state = State.Dead;
            _frame = 0;
            _nextFrameAt = 0f;
            ApplyFrame();
        }

        // ── Update ────────────────────────────────────────────────────────────

        private void Update()
        {
            if (Time.time < _nextFrameAt) return;
            _nextFrameAt = Time.time + 1f / Mathf.Max(1f, GetFps());

            switch (_state)
            {
                case State.Idle:
                    var iFrames = GetIdleFrames();
                    _frame = iFrames.Length > 0 ? (_frame + 1) % iFrames.Length : 0;
                    break;

                case State.Walk:
                    _frame = HasFrames(walkFrames) ? (_frame + 1) % walkFrames.Length : 0;
                    break;

                case State.Attack:
                    _frame++;
                    if (!HasFrames(attackFrames) || _frame >= attackFrames.Length)
                    {
                        _state = State.Idle;
                        _frame = 0;
                    }
                    break;

                case State.Dead:
                    if (HasFrames(deathFrames) && _frame < deathFrames.Length - 1)
                        _frame++;
                    break;
            }

            ApplyFrame();
        }

        // ── Internals ─────────────────────────────────────────────────────────

        private float GetFps() => _state switch
        {
            State.Walk   => walkFps,
            State.Attack => attackFps,
            State.Dead   => deathFps,
            _            => idleFps
        };

        private void ApplyFrame()
        {
            if (spriteRenderer == null) return;
            Sprite[] frames = GetCurrentFrames();
            if (frames == null || frames.Length == 0) return;
            spriteRenderer.sprite = frames[Mathf.Clamp(_frame, 0, frames.Length - 1)];

            if (flipXWithMovement && Mathf.Abs(_lastFacing.x) > 0.001f)
                spriteRenderer.flipX = _lastFacing.x < 0f;
        }

        private Sprite[] GetCurrentFrames() => _state switch
        {
            State.Attack when HasFrames(attackFrames) => attackFrames,
            State.Dead   when HasFrames(deathFrames)  => deathFrames,
            State.Walk   when HasFrames(walkFrames)   => walkFrames,
            _                                         => GetIdleFrames()
        };

        private Sprite[] GetIdleFrames() =>
            HasFrames(idleFrames) ? idleFrames :
            HasFrames(walkFrames) ? walkFrames :
            System.Array.Empty<Sprite>();

        private static bool HasFrames(Sprite[] arr) => arr != null && arr.Length > 0;
    }
}
