using System.Collections;
using System.Collections.Generic;
using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] private LayerMask _structureMask;
        [SerializeField] private int _buildMaterials;
        [SerializeField] private Transform _structuresRoot;
        [SerializeField] private Transform[] _enemySpawnPoints;
        [SerializeField] private List<SO_StructureData> _availableStructures = new();
        [SerializeField] private Material _previewValidMaterial;
        [SerializeField] private Material _previewInvalidMaterial;
        [SerializeField] private Vector2 _gridStep = new(0.5f, 0.5f);

        public int BuildMaterials => _buildMaterials;
        public float BuildTimeRemaining { get; private set; }
        public bool IsBuildPhaseActive { get; private set; }

        private Coroutine _buildPhaseRoutine;
        private int _selectedStructureIndex;
        private GameObject _previewInstance;
        private SpriteRenderer _previewSprite;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!IsBuildPhaseActive)
                return;

            HandleSelectionInput();
            UpdatePreview();
            HandlePlacementInput();
        }

        public void StartBuildPhase()
        {
            if (_buildPhaseRoutine != null)
                StopCoroutine(_buildPhaseRoutine);

            IsBuildPhaseActive = true;
            BuildTimeRemaining = _buildPhaseDuration;
            _buildPhaseRoutine = StartCoroutine(BuildPhaseRoutine());
            EnsurePreview();
        }

        public void AddMaterials(int amount) => _buildMaterials += amount;

        public void ResetMaterialsForNewSession()
        {
            _buildMaterials = 0;
            Script_03_EventBus.Publish(new BuildMaterialsChangedEvent { materials = _buildMaterials });
        }

        public bool TryPlaceStructure(SO_StructureData data, Vector2 position)
        {
            if (!IsBuildPhaseActive || data == null)
                return false;

            if (_buildMaterials < data.materialCost || !IsValidBuildPosition(data, position))
                return false;

            GameObject placed = Instantiate(data.prefab, position, Quaternion.identity, _structuresRoot);
            if (placed.TryGetComponent(out Script_24_StructureBase structure))
                structure.Initialize(data, position);

            _buildMaterials -= data.materialCost;
            Script_03_EventBus.Publish(new BuildMaterialsChangedEvent { materials = _buildMaterials });
            return true;
        }

        private IEnumerator BuildPhaseRoutine()
        {
            while (BuildTimeRemaining > 0f)
            {
                BuildTimeRemaining -= Time.deltaTime;
                Script_03_EventBus.Publish(new BuildPhaseTimerEvent { timeRemaining = BuildTimeRemaining });
                yield return null;
            }

            IsBuildPhaseActive = false;
            if (_previewInstance != null)
                _previewInstance.SetActive(false);
        }

        private void HandleSelectionInput()
        {
            if (_availableStructures.Count == 0)
                return;

            bool next = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            bool prev = Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame;

            if (Gamepad.current != null && Gamepad.current.leftShoulder.isPressed)
            {
                Vector2 stick = Gamepad.current.leftStick.ReadValue();
                if (stick.x > 0.6f) next = true;
                if (stick.x < -0.6f) prev = true;
            }

            if (next)
                _selectedStructureIndex = (_selectedStructureIndex + 1) % _availableStructures.Count;
            else if (prev)
                _selectedStructureIndex = (_selectedStructureIndex - 1 + _availableStructures.Count) % _availableStructures.Count;
        }

        private void HandlePlacementInput()
        {
            if (_availableStructures.Count == 0)
                return;

            bool confirm = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                           || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
            if (!confirm)
                return;

            SO_StructureData selected = _availableStructures[_selectedStructureIndex];
            Vector2 targetPosition = GetBuildCursorWorldPosition();
            TryPlaceStructure(selected, targetPosition);
        }

        private void UpdatePreview()
        {
            if (_previewInstance == null || _availableStructures.Count == 0)
                return;

            SO_StructureData selected = _availableStructures[_selectedStructureIndex];
            Vector2 targetPosition = GetBuildCursorWorldPosition();
            _previewInstance.transform.position = targetPosition;

            bool isValid = IsValidBuildPosition(selected, targetPosition) && _buildMaterials >= selected.materialCost;
            if (_previewSprite != null)
            {
                _previewSprite.color = isValid ? new Color(0.2f, 1f, 0.2f, 0.5f) : new Color(1f, 0.2f, 0.2f, 0.5f);
            }
        }

        private void EnsurePreview()
        {
            if (_availableStructures.Count == 0)
                return;

            if (_previewInstance == null)
            {
                _previewInstance = new GameObject("BuildPreview");
                _previewSprite = _previewInstance.AddComponent<SpriteRenderer>();
            }

            _previewInstance.SetActive(true);
            _previewSprite.sprite = GetStructureSprite(_availableStructures[_selectedStructureIndex]);
            _previewSprite.sortingOrder = 100;
        }

        private Sprite GetStructureSprite(SO_StructureData data)
        {
            if (data?.prefab == null)
                return null;
            return data.prefab.GetComponentInChildren<SpriteRenderer>()?.sprite;
        }

        private Vector2 GetBuildCursorWorldPosition()
        {
            Vector2 raw;
            if (Mouse.current != null)
            {
                raw = Camera.main != null
                    ? Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue())
                    : Vector2.zero;
            }
            else
            {
                raw = transform.position;
            }

            return new Vector2(
                Mathf.Round(raw.x / _gridStep.x) * _gridStep.x,
                Mathf.Round(raw.y / _gridStep.y) * _gridStep.y);
        }

        private bool IsValidBuildPosition(SO_StructureData data, Vector2 position)
        {
            Collider2D surface = Physics2D.OverlapCircle(position, 0.2f, _buildSurfaceMask);
            if (surface == null)
                return false;

            Collider2D overlap = Physics2D.OverlapBox(position, data.footprint, 0f, _structureMask);
            if (overlap != null)
                return false;

            foreach (Transform spawn in _enemySpawnPoints)
            {
                if (spawn == null)
                    continue;
                if (Vector2.Distance(position, spawn.position) < 1.5f)
                    return false;
            }

            return true;
        }
    }
}
