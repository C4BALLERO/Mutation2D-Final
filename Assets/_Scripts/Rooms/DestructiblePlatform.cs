using System.Collections;
using UnityEngine;

namespace MutationSwarm.Rooms
{
    /// <summary>
    /// Platform that starts shaking and then falls after a player stands on it.
    /// Respawns after a configurable delay.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DestructiblePlatform : MonoBehaviour
    {
        [SerializeField] private float _breakDelay   = 1.2f;
        [SerializeField] private float _respawnDelay = 4f;
        [SerializeField] private LayerMask _playerMask;

        private SpriteRenderer _sr;
        private BoxCollider2D  _col;
        private Vector3 _originPos;
        private bool _triggered;

        private void Awake()
        {
            _sr      = GetComponent<SpriteRenderer>();
            _col     = GetComponent<BoxCollider2D>();
            _originPos = transform.position;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (_triggered) return;
            if (((1 << col.gameObject.layer) & _playerMask) == 0) return;

            // Only trigger when player lands on top
            foreach (ContactPoint2D cp in col.contacts)
            {
                if (cp.normal.y < -0.5f)
                {
                    StartCoroutine(BreakSequence());
                    return;
                }
            }
        }

        private IEnumerator BreakSequence()
        {
            _triggered = true;
            float elapsed = 0f;
            float shakeAmp = 0.05f;

            // Shake phase
            while (elapsed < _breakDelay)
            {
                float t = elapsed / _breakDelay;
                float shake = Mathf.Sin(elapsed * 40f) * shakeAmp * (1f - t);
                transform.position = _originPos + new Vector3(shake, 0f, 0f);
                _sr.color = Color.Lerp(Color.white, new Color(1f, 0.4f, 0.2f), t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Disable
            _col.enabled = false;
            _sr.enabled  = false;
            transform.position = _originPos;

            // Respawn
            yield return new WaitForSeconds(_respawnDelay);

            _col.enabled = true;
            _sr.enabled  = true;
            _sr.color    = Color.white;
            _triggered   = false;
        }
    }
}
