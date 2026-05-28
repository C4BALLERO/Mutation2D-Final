using MutationSwarm.Core;
using UnityEngine;

namespace MutationSwarm.Building
{
    /// <summary>
    /// Construcción entre oleadas (UpgradePhase), grid y recursos.
    /// </summary>
    public class Script_23_BuildManager : MonoBehaviour
    {
        public static Script_23_BuildManager Instance { get; private set; }

        [SerializeField] private float _buildPhaseDuration = 20f;
        [SerializeField] private LayerMask _buildSurfaceMask;
        [SerializeField] private int _buildMaterials;

        public int BuildMaterials => _buildMaterials;
        public bool IsBuildPhaseActive { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void StartBuildPhase()
        {
            IsBuildPhaseActive = true;
        }

        public void AddMaterials(int amount) => _buildMaterials += amount;

        public bool TryPlaceStructure(SO_StructureData data, Vector2 position) => false;
    }
}
