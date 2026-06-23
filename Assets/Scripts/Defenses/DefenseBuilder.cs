using UnityEngine;

namespace MutationSwarm
{
    public class DefenseBuilder : MonoBehaviour
    {
        public GameObject barricadePrefab;
        public GameObject turretPrefab;
        public GameObject minePrefab;
        public Transform  defensesContainer;

        public int  SelectedType { get; private set; } = 1; // 1=Barricade,2=Turret,3=Mine
        const float COST = 30f;

        Camera _cam;

        void Start() => _cam = Camera.main;

        void Update()
        {
            if (GameManager.Instance == null) return;
            // B (enter/exit build mode) is handled solely by PlayerController to avoid
            // multiple ToggleBuild() calls in the same frame cancelling each other out.
            if (GameManager.Instance.Phase != GamePhase.Building) return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectedType = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2)) SelectedType = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3)) SelectedType = 3;

            if (Input.GetMouseButtonDown(0))
            {
                if (!PlayerStats.Instance.SpendDna(COST)) return;

                Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
                wp.z = 0f;
                // Snap X to 1-unit grid
                wp.x = Mathf.Round(wp.x);

                // Snap Y to nearest platform
                wp.y = SnapToSurface(wp);

                PlaceDefense(wp);
            }
        }

        float SnapToSurface(Vector3 wp)
        {
            // Raycast down to find ground/platform
            var hit = Physics2D.Raycast(new Vector2(wp.x, wp.y + 2f), Vector2.down, 10f,
                LayerMask.GetMask("Ground", "Platform"));
            if (hit.collider) return hit.point.y;
            return wp.y;
        }

        void PlaceDefense(Vector3 pos)
        {
            GameObject prefab = SelectedType switch
            {
                2 => turretPrefab,
                3 => minePrefab,
                _ => barricadePrefab,
            };
            var go = Instantiate(prefab, pos, Quaternion.identity, defensesContainer);
            go.SetActive(true);
            var db = go.GetComponent<DefenseBase>();
            db?.Init((DefenseType)(SelectedType - 1));
            if (WaveManager.Instance != null) WaveManager.Instance.CurrentStats.defensesBuilt++;
            ParticleManager.Instance?.SpawnBurst(pos, new Color(0.3f, 1f, 0.5f), 8, 4f);
        }
    }
}
