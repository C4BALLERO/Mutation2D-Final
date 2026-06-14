using MutationSwarm.Combat;
using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.UI
{
    /// <summary>
    /// HUD en tiempo real: HP, oleada, enemigos, dash, recursos, evolución.
    /// </summary>
    public class Script_25_HUDController : MonoBehaviour
    {
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private int _maxPlayersUi = 4;

        private Label _waveLabel;
        private Label _enemyCounterLabel;
        private Label _materialsLabel;
        private Label _weaponLabel;
        private Label _dominantGeneLabel;
        private Label _generationLabel;
        private VisualElement _dominantGeneColor;
        private VisualElement _superAdaptationAlert;
        private VisualElement _mutationToastContainer;
        private ProgressBar[] _playerHpBars;

        private void OnEnable()
        {
            CacheUi();
            Script_03_EventBus.Subscribe<WaveStartedEvent>(OnWaveStarted);
            Script_03_EventBus.Subscribe<WaveEndedEvent>(OnWaveEnded);
            Script_03_EventBus.Subscribe<EvolutionPhaseEvent>(OnEvolutionPhase);
            Script_03_EventBus.Subscribe<EnemyCountChangedEvent>(OnEnemyCountChanged);
            Script_03_EventBus.Subscribe<CoinChangedEvent>(OnCoinChanged);
            Script_03_EventBus.Subscribe<MutationToastEvent>(OnMutationToast);
            Script_03_EventBus.Subscribe<WeaponEquippedEvent>(OnWeaponEquipped);
        }

        private void OnDisable()
        {
            Script_03_EventBus.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
            Script_03_EventBus.Unsubscribe<WaveEndedEvent>(OnWaveEnded);
            Script_03_EventBus.Unsubscribe<EvolutionPhaseEvent>(OnEvolutionPhase);
            Script_03_EventBus.Unsubscribe<EnemyCountChangedEvent>(OnEnemyCountChanged);
            Script_03_EventBus.Unsubscribe<CoinChangedEvent>(OnCoinChanged);
            Script_03_EventBus.Unsubscribe<MutationToastEvent>(OnMutationToast);
            Script_03_EventBus.Unsubscribe<WeaponEquippedEvent>(OnWeaponEquipped);
        }

        private void OnWeaponEquipped(WeaponEquippedEvent e)
        {
            if (e.weapon != null)
                UpdateWeaponAndAmmo(e.weapon.displayName, -1);
        }

        private void CacheUi()
        {
            if (_uiDocument == null)
                return;

            VisualElement root = _uiDocument.rootVisualElement;
            _waveLabel = root.Q<Label>("WaveLabel");
            _enemyCounterLabel = root.Q<Label>("EnemyCounterLabel");
            _materialsLabel = root.Q<Label>("MaterialsValue");
            _weaponLabel = root.Q<Label>("WeaponValue");
            _dominantGeneLabel = root.Q<Label>("DominantGeneLabel");
            _generationLabel = root.Q<Label>("GenerationLabel");
            _dominantGeneColor = root.Q<VisualElement>("DominantGeneColor");
            _superAdaptationAlert = root.Q<VisualElement>("SuperAdaptationAlert");
            _mutationToastContainer = root.Q<VisualElement>("MutationToastContainer");

            _playerHpBars = new ProgressBar[_maxPlayersUi];
            for (int i = 0; i < _maxPlayersUi; i++)
                _playerHpBars[i] = root.Q<ProgressBar>($"PlayerHpBar_{i + 1}");
        }

        private void OnWaveStarted(WaveStartedEvent e)
        {
            if (_waveLabel != null)
                _waveLabel.text = $"Oleada {e.waveNumber}";
        }

        private void OnWaveEnded(WaveEndedEvent e)
        {
            ShowToast($"Oleada {e.waveNumber} completada");
        }

        private void OnEvolutionPhase(EvolutionPhaseEvent e)
        {
            if (_dominantGeneLabel != null)
                _dominantGeneLabel.text = e.summary.dominantGene;
            if (_generationLabel != null)
                _generationLabel.text = $"Gen. {e.summary.generationNumber}";

            if (_superAdaptationAlert != null)
            {
                _superAdaptationAlert.style.display = DisplayStyle.Flex;
                CancelInvoke(nameof(HideSuperAdaptationAlert));
                Invoke(nameof(HideSuperAdaptationAlert), 3f);
            }
        }

        private void HideSuperAdaptationAlert()
        {
            if (_superAdaptationAlert != null)
                _superAdaptationAlert.style.display = DisplayStyle.None;
        }

        private void OnEnemyCountChanged(EnemyCountChangedEvent e)
        {
            if (_enemyCounterLabel != null)
                _enemyCounterLabel.text = $"{e.alive} / {e.total}";
        }

        private void OnCoinChanged(CoinChangedEvent e)
        {
            if (_materialsLabel != null)
                _materialsLabel.text = $"🪙 {e.coins}";
        }

        private void OnMutationToast(MutationToastEvent e)
        {
            ShowToast(e.message, e.color);
        }

        public void UpdatePlayerHp(int playerIndex, float current, float max)
        {
            if (_playerHpBars == null || playerIndex < 0 || playerIndex >= _playerHpBars.Length || _playerHpBars[playerIndex] == null)
                return;

            _playerHpBars[playerIndex].value = Mathf.Clamp01(max <= 0f ? 0f : current / max) * 100f;
            _playerHpBars[playerIndex].title = $"P{playerIndex + 1} HP";
        }

        public void UpdateDashCooldown(float normalized)
        {
            VisualElement ring = _uiDocument?.rootVisualElement.Q<VisualElement>("DashCooldownRingFill");
            if (ring != null)
                ring.style.scale = new Scale(new Vector2(1f, Mathf.Clamp01(normalized)));
        }

        public void UpdateWeaponAndAmmo(string weaponName, int ammo)
        {
            if (_weaponLabel != null)
                _weaponLabel.text = ammo >= 0 ? $"{weaponName} ({ammo})" : weaponName;
        }

        private void ShowToast(string message, Color? color = null)
        {
            if (_mutationToastContainer == null)
                return;

            Label toast = new(message)
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 4,
                    paddingBottom = 4,
                    marginBottom = 4,
                    backgroundColor = new Color(0f, 0f, 0f, 0.7f),
                    color = color ?? Color.white
                }
            };

            _mutationToastContainer.Add(toast);
            StartCoroutine(RemoveToastAfterDelay(toast, 3f));
        }

        private System.Collections.IEnumerator RemoveToastAfterDelay(VisualElement toast, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (toast.parent != null)
                toast.parent.Remove(toast);
        }
    }
}
