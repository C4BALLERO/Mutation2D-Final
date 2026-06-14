using System.Collections.Generic;
using MutationSwarm.Core;
using MutationSwarm.UI;
using UnityEngine;

namespace MutationSwarm.Combat
{
    /// <summary>
    /// Abre la tienda de armas tras oleadas configuradas.
    /// </summary>
    public class Script_39_WeaponShopManager : MonoBehaviour
    {
        public static Script_39_WeaponShopManager Instance { get; private set; }

        [SerializeField] private List<SO_WeaponData> _catalog = new();
        [SerializeField] private int[] _shopAfterWaves = { 2, 5, 8, 11, 14 };
        [SerializeField] private bool _alsoOnEveryThirdWave = true;
        [SerializeField] private Script_40_WeaponShopUI _shopUi;

        public bool IsShopOpen { get; private set; }
        public IReadOnlyList<SO_WeaponData> Catalog => _catalog;

        private Script_02_WaveManager _waveManager;

        private void Awake()
        {
            Instance = this;
            _waveManager = FindFirstObjectByType<Script_02_WaveManager>();
        }

        private void OnEnable()
        {
            Script_03_EventBus.Subscribe<WaveEndedEvent>(OnWaveEnded);
        }

        private void OnDisable()
        {
            Script_03_EventBus.Unsubscribe<WaveEndedEvent>(OnWaveEnded);
        }

        private void OnWaveEnded(WaveEndedEvent e)
        {
            if (ShouldOpenShop(e.waveNumber))
            {
                OpenShop(e.waveNumber);
                return;
            }

            _waveManager?.StartNextWave();
        }

        public bool ShouldOpenShop(int waveNumber) => true;

        public void OpenShop(int afterWave)
        {
            if (IsShopOpen)
                return;

            IsShopOpen = true;
            Time.timeScale = 0f;
            Script_03_EventBus.Publish(new WeaponShopOpenedEvent { afterWave = afterWave });

            if (_shopUi != null)
                _shopUi.Show(afterWave);
            else
                Debug.LogWarning("[WeaponShop] Falta Script_40_WeaponShopUI en escena.");
        }

        public void CloseShopAndContinue()
        {
            if (!IsShopOpen)
                return;

            IsShopOpen = false;
            Time.timeScale = 1f;

            if (_shopUi != null)
                _shopUi.Hide();

            Script_03_EventBus.Publish(new WeaponShopClosedEvent());

            _waveManager?.StartNextWave();
        }
    }
}
