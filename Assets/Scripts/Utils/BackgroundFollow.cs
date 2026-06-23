using UnityEngine;

namespace MutationSwarm
{
    // Keeps a full-screen background sprite centered on the camera and scaled to
    // always cover the viewport (uniform scale, no distortion — excess is cropped).
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundFollow : MonoBehaviour
    {
        public float parallax = 1f; // 1 = follows camera exactly; <1 = slower (depth)
        public float margin = 1.06f;

        Camera _cam;
        SpriteRenderer _sr;
        Vector3 _origin;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _origin = transform.position;
        }

        void LateUpdate()
        {
            if (_cam == null) { _cam = Camera.main; if (_cam == null) return; }
            if (_sr == null || _sr.sprite == null) return;

            // Cover the viewport with uniform scale.
            float h = _cam.orthographicSize * 2f;
            float w = h * _cam.aspect;
            Vector2 size = _sr.sprite.bounds.size;
            if (size.x > 0.0001f && size.y > 0.0001f)
            {
                float k = Mathf.Max(w / size.x, h / size.y) * margin;
                transform.localScale = new Vector3(k, k, 1f);
            }

            // Follow the camera (parallax) while keeping our own Z.
            Vector3 cam = _cam.transform.position;
            transform.position = new Vector3(
                _origin.x + (cam.x - _origin.x) * parallax,
                _origin.y + (cam.y - _origin.y) * parallax,
                transform.position.z);
        }
    }
}
