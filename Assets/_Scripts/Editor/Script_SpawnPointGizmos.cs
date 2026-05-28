#if UNITY_EDITOR
using UnityEngine;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Gizmos de spawn points y límites de arena (PROMPT 09/13).
    /// </summary>
    [ExecuteAlways]
    public class Script_SpawnPointGizmos : MonoBehaviour
    {
        [SerializeField] private Color _arenaBoundsColor = new(0.2f, 0.8f, 1f, 0.5f);
        [SerializeField] private float _arenaHalfWidth = 9.5f;
        [SerializeField] private float _arenaHalfHeight = 4.5f;

        private void OnDrawGizmos()
        {
            Gizmos.color = _arenaBoundsColor;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(_arenaHalfWidth * 2f, _arenaHalfHeight * 2f, 0f));

            foreach (Transform child in transform)
            {
                bool isPlayer = child.name.StartsWith("p");
                Gizmos.color = isPlayer ? Color.green : Color.red;
                Gizmos.DrawSphere(child.position, isPlayer ? 0.25f : 0.35f);
            }
        }
    }
}
#endif
