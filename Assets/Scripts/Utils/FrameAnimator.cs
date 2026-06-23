using UnityEngine;

namespace MutationSwarm
{
    // Lightweight sprite-sheet frame player. Cycles a Sprite[] on a SpriteRenderer.
    // Driven by gameplay scripts (PlayerController / EnemyBase) instead of an AnimatorController,
    // so it integrates cleanly with the MutationSwarm logic and carries no external GUID refs.
    [RequireComponent(typeof(SpriteRenderer))]
    public class FrameAnimator : MonoBehaviour
    {
        public float fps = 12f;

        SpriteRenderer _sr;
        Sprite[] _cur;
        bool _loop;
        float _t;
        int _idx;
        bool _done;

        public bool IsDone => _done;

        void Awake() { if (_sr == null) _sr = GetComponent<SpriteRenderer>(); }

        // Play a clip. Re-calling with the same array is a no-op (keeps it running smoothly).
        public void Play(Sprite[] frames, bool loop = true)
        {
            if (frames == null || frames.Length == 0) return;
            if (_cur == frames) return;
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            _cur = frames; _loop = loop; _t = 0f; _idx = 0; _done = false;
            _sr.sprite = frames[0];
        }

        void Update()
        {
            if (_cur == null || _cur.Length == 0) return;
            _t += Time.deltaTime * fps;
            while (_t >= 1f)
            {
                _t -= 1f;
                _idx++;
                if (_idx >= _cur.Length)
                {
                    if (_loop) { _idx = 0; }
                    else { _idx = _cur.Length - 1; _done = true; }
                }
            }
            if (_sr != null) _sr.sprite = _cur[_idx];
        }
    }
}
