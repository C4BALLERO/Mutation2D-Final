using UnityEngine;

namespace MutationSwarm
{
    // Holds the player's animation frames (baked at edit time by SceneSetup) and
    // drives the FrameAnimator based on PlayerController state.
    [RequireComponent(typeof(FrameAnimator))]
    public class PlayerVisual : MonoBehaviour
    {
        public Sprite[] idle;
        public Sprite[] walk;
        public Sprite[] jump;
        public Sprite[] dash;

        FrameAnimator _anim;
        PlayerController _pc;
        Rigidbody2D _rb;

        void Awake()
        {
            _anim = GetComponent<FrameAnimator>();
            _rb = GetComponentInParent<Rigidbody2D>();
        }

        void LateUpdate()
        {
            if (_pc == null) _pc = PlayerController.Instance;
            if (_pc == null || _anim == null) return;

            if (_pc.IsDashing && dash != null && dash.Length > 0)
            {
                _anim.Play(dash, true);
                return;
            }

            if (!_pc.OnGround && jump != null && jump.Length > 0)
            {
                _anim.Play(jump, true);
                return;
            }

            float vx = _rb != null ? Mathf.Abs(_rb.linearVelocity.x) : 0f;
            if (vx > 0.5f && walk != null && walk.Length > 0)
                _anim.Play(walk, true);
            else if (idle != null && idle.Length > 0)
                _anim.Play(idle, true);
        }
    }
}
