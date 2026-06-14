using MutationSwarm.Core;
using UnityEngine;

namespace MutationSwarm.Core
{
    public class Script_42_CoinManager : MonoBehaviour
    {
        public static Script_42_CoinManager Instance { get; private set; }

        [SerializeField] private int _coinsPerKill = 5;
        [SerializeField] private int _bonusCoinsPerWave = 1;

        private int _coins;
        private int _currentWave;

        public int Coins => _coins;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            Script_03_EventBus.Subscribe<EnemyDiedEvent>(OnEnemyDied);
            Script_03_EventBus.Subscribe<WaveStartedEvent>(OnWaveStarted);
        }

        private void OnDisable()
        {
            Script_03_EventBus.Unsubscribe<EnemyDiedEvent>(OnEnemyDied);
            Script_03_EventBus.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
        }

        private void OnWaveStarted(WaveStartedEvent e) => _currentWave = e.waveNumber;

        private void OnEnemyDied(EnemyDiedEvent e)
        {
            AddCoins(_coinsPerKill + _bonusCoinsPerWave * (_currentWave - 1));
        }

        public void AddCoins(int amount)
        {
            _coins += Mathf.Max(0, amount);
            Script_03_EventBus.Publish(new CoinChangedEvent { coins = _coins });
        }

        public bool TrySpend(int amount)
        {
            if (amount > _coins) return false;
            _coins -= amount;
            Script_03_EventBus.Publish(new CoinChangedEvent { coins = _coins });
            return true;
        }

        public void ResetForNewSession()
        {
            _coins = 0;
            _currentWave = 0;
            Script_03_EventBus.Publish(new CoinChangedEvent { coins = _coins });
        }
    }
}
