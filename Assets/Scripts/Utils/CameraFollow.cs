using System.Collections;
using UnityEngine;

namespace MutationSwarm
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

        public float smoothSpeed  = 5f;
        public Vector2 offset     = new Vector2(0f, 1.5f);
        public float minX = -12f, maxX = 12f;
        public float minY = -1f,  maxY = 8f;

        float _shakeTimer, _shakeAmt;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void LateUpdate()
        {
            if (PlayerController.Instance == null) return;
            Vector3 target = (Vector3)(Vector2)PlayerController.Instance.transform.position + (Vector3)offset;
            target.x = Mathf.Clamp(target.x, minX, maxX);
            target.y = Mathf.Clamp(target.y, minY, maxY);
            target.z = -10f;
            transform.position = Vector3.Lerp(transform.position, target, smoothSpeed * Time.deltaTime);

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                transform.position += (Vector3)(Random.insideUnitCircle * _shakeAmt);
            }
        }

        public void Shake(float duration, float amount)
        {
            _shakeTimer = Mathf.Max(_shakeTimer, duration);
            _shakeAmt   = amount;
        }
    }
}
