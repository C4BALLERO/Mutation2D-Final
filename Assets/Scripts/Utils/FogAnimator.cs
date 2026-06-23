using UnityEngine;

namespace MutationSwarm
{
    // Animated acid fog overlay for the background.
    // Must live in its own file (filename == class name) so Unity can serialize it as a component.
    public class FogAnimator : MonoBehaviour
    {
        float _t;
        SpriteRenderer _sr;

        void Start() => _sr = GetComponent<SpriteRenderer>();

        void Update()
        {
            _t += Time.deltaTime;
            if (_sr) _sr.color = new Color(0.05f, 0.25f, 0.02f, 0.35f + Mathf.Sin(_t * 0.4f) * 0.1f);
            transform.position = new Vector3(Mathf.Sin(_t * 0.15f) * 0.5f, transform.position.y, transform.position.z);
        }
    }
}
