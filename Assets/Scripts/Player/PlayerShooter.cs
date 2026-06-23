using UnityEngine;

namespace MutationSwarm
{
    public class PlayerShooter : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public Transform  muzzle;
        public float      bulletSpeed = 18f;
        public float      baseCooldown = 0.2f;

        float _cd;
        int   _electricCounter;
        Camera _cam;

        void Start() => _cam = Camera.main;

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.Phase != GamePhase.Playing) return;
            _cd -= Time.deltaTime;

            float cd = baseCooldown;
            if (PlayerStats.Instance.HasUpgrade("fasterReload")) cd *= 0.45f;

            if (Input.GetMouseButton(0) && _cd <= 0f)
            {
                Shoot(cd);
                _cd = cd;
            }
        }

        void Shoot(float cd)
        {
            Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 dir = ((Vector2)(mouseWorld - muzzle.position)).normalized;

            float dmg = 12f + (PlayerStats.Instance.HasUpgrade("moreDamage") ? 8f : 0f);
            int piercing = PlayerStats.Instance.HasUpgrade("piercing") ? 2 : 0;

            _electricCounter++;
            bool isElectric = PlayerStats.Instance.HasUpgrade("electric") && _electricCounter % 5 == 0;

            var go = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity);
            go.SetActive(true);
            var b  = go.GetComponent<Bullet>();
            b.Init(dir, bulletSpeed, dmg, piercing, isElectric, fromPlayer: true);

            if (WaveManager.Instance != null) WaveManager.Instance.CurrentStats.bulletsShot++;
            ParticleManager.Instance?.SpawnBurst(muzzle.position, new Color(1f,1f,0.5f), 3, 5f);

            // Face direction of mouse
            var pc = PlayerController.Instance;
            if (pc != null) transform.localScale = new Vector3(mouseWorld.x < transform.position.x ? -1 : 1, 1, 1);
        }
    }
}
