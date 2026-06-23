using UnityEngine;

namespace MutationSwarm
{
    // Draws enemy HP bars using Unity's GL system via Camera
    [DefaultExecutionOrder(100)]
    public class EnemyRenderer : MonoBehaviour
    {
        Camera _cam;

        void Start() => _cam = Camera.main;

        void OnGUI()
        {
            if (WaveManager.Instance == null) return;
            foreach (var e in WaveManager.Instance.ActiveEnemies)
            {
                if (e == null) continue;
                Vector3 sp = _cam.WorldToScreenPoint(e.transform.position + Vector3.up * 0.8f);
                if (sp.z < 0f) continue;
                float barW = 40f, barH = 5f;
                float sx = sp.x - barW * 0.5f, sy = Screen.height - sp.y;
                GUI.color = new Color(0.13f, 0f, 0f);
                GUI.DrawTexture(new Rect(sx, sy, barW, barH), Texture2D.whiteTexture);
                float ratio = e.HpRatio;
                GUI.color = ratio > 0.5f ? new Color(0.27f,1f,0.1f) : ratio > 0.25f ? new Color(1f,0.8f,0f) : new Color(1f,0.13f,0f);
                GUI.DrawTexture(new Rect(sx, sy, barW * ratio, barH), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }
    }
}
