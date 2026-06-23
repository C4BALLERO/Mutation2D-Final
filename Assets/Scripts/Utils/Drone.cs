using UnityEngine;

namespace MutationSwarm
{
    public class Drone : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public Sprite[] chickenFrames;
        float _angle, _shootCd;
        SpriteRenderer _sr;

        void Start()
        {
            _sr = gameObject.AddComponent<SpriteRenderer>();
            _sr.sortingOrder = 4;

            if (chickenFrames != null && chickenFrames.Length > 0)
            {
                _sr.color = Color.white;
                transform.localScale = Vector3.one * 0.35f;
                var anim = gameObject.AddComponent<FrameAnimator>();
                anim.fps = 10f;
                anim.Play(chickenFrames);
            }
            else
            {
                _sr.sprite = SpriteFactory.Instance != null ? SpriteFactory.Instance.DroneSprite : null;
                _sr.color = new Color(0.3f, 0.7f, 1f);
                transform.localScale = Vector3.one * 0.5f;
            }
        }

        void Update()
        {
            if (PlayerStats.Instance == null || PlayerController.Instance == null) return;

            // POLLO es el dron que se compra en la tienda (upgrade "drone"). Solo aparece
            // tras comprarlo y únicamente mientras juegas; usamos sr.enabled (no SetActive)
            // para que este Update siga corriendo y reaccione cuando lo compres.
            var gm = GameManager.Instance;
            bool active = PlayerStats.Instance.HasUpgrade("drone")
                          && gm != null && (gm.Phase == GamePhase.Playing || gm.Phase == GamePhase.Building);
            if (_sr != null) _sr.enabled = active;
            if (!active) return;

            _angle += Time.deltaTime * 120f;
            var player = PlayerController.Instance.transform;
            float r = 2.5f;
            Vector3 offset = new Vector3(
                Mathf.Cos(_angle * Mathf.Deg2Rad) * r,
                Mathf.Sin(_angle * Mathf.Deg2Rad) * r * 0.5f, 0f);
            transform.position = Vector3.Lerp(transform.position, player.position + offset, 0.15f);

            // Flip to face movement direction
            float dx = (player.position + offset - transform.position).x;
            if (Mathf.Abs(dx) > 0.05f)
                transform.localScale = new Vector3(
                    (dx > 0 ? 1f : -1f) * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y, transform.localScale.z);

            _shootCd -= Time.deltaTime;
            if (_shootCd > 0f) return;
            if (WaveManager.Instance == null) return;

            EnemyBase nearest = null; float nearDist = 9f;
            foreach (var e in WaveManager.Instance.ActiveEnemies)
            {
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d < nearDist) { nearDist = d; nearest = e; }
            }
            if (nearest == null) { _shootCd = 0.3f; return; }

            Vector2 dir = ((Vector2)(nearest.transform.position - transform.position)).normalized;
            var go = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            go.SetActive(true);
            go.GetComponent<Bullet>()?.Init(dir, 14f, 6f, 0, false, fromPlayer: true);
            _shootCd = 0.8f;
        }
    }
}
