using UnityEngine;

namespace MutationSwarm.Rooms
{
    /// <summary>
    /// Moves a kinematic platform between two world-space waypoints.
    /// The Rigidbody2D on this object must be set to Kinematic.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector2 _pointA;
        [SerializeField] private Vector2 _pointB;
        [SerializeField] private float   _speed = 2f;
        [SerializeField] private bool    _startAtB;

        private Rigidbody2D _rb;
        private Vector2 _target;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Convert local offsets to world positions if they look like relative offsets
            // (store as world positions by adding transform.position at setup time)
            _target = _startAtB ? _pointA : _pointB;
            if (_startAtB) _rb.position = _pointB;
        }

        private void FixedUpdate()
        {
            Vector2 next = Vector2.MoveTowards(_rb.position, _target, _speed * Time.fixedDeltaTime);
            _rb.MovePosition(next);

            if (Vector2.Distance(_rb.position, _target) < 0.02f)
                _target = (_target == _pointA) ? _pointB : _pointA;
        }

        // Sets the two waypoints in world space.
        public void SetWaypoints(Vector2 a, Vector2 b)
        {
            _pointA = a;
            _pointB = b;
            _target = b;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_pointA, 0.2f);
            Gizmos.DrawWireSphere(_pointB, 0.2f);
            Gizmos.DrawLine(_pointA, _pointB);
        }
    }
}
