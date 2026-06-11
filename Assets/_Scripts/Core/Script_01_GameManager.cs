using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Estados globales de la partida.
    /// </summary>
    public enum GameState
    {
        Boot,
        MainMenu,
        Playing,
        Paused,
        UpgradePhase,
        EvolutionPhase,
        GameOver
    }

    /// <summary>
    /// Singleton central: estado de partida, jugadores activos y transiciones de escena.
    /// Persiste entre escenas con DontDestroyOnLoad.
    /// </summary>
    public class Script_01_GameManager : MonoBehaviour
    {
        public static Script_01_GameManager Instance { get; private set; }

        [Header("Estado de partida")]
        [SerializeField] private GameState _currentState = GameState.Boot;
        [SerializeField] private int _playerCount = 1;
        [SerializeField] private int _maxPlayers = 4;

        [Header("Escenas")]
        [SerializeField] private string _mainMenuSceneName = "Scene_01_MainMenu";
        [SerializeField] private string _gameWorldSceneName = "Scene_02_GameWorld"; // legacy fallback
        [SerializeField] private bool   _useRoomChain       = true;                  // use Room_01..Boss chain

        [Header("Coop")]
        [SerializeField] private bool[] _playersAlive = new bool[4];

        /// <summary>Oleada actual (sincronizada con WaveManager).</summary>
        public int CurrentWave { get; private set; }

        public GameState CurrentState => _currentState;
        public int PlayerCount => _playerCount;
        public int MaxPlayers => _maxPlayers;

        /// <summary>True while input and gameplay should be blocked (paused, shop, upgrade/evolution phase).</summary>
        public bool IsGameplayFrozen => _currentState == GameState.Paused ||
                                        _currentState == GameState.UpgradePhase ||
                                        _currentState == GameState.EvolutionPhase;

        /// <summary>Evento al cambiar el estado global.</summary>
        public event Action<GameState, GameState> OnGameStateChanged;

        /// <summary>Evento cuando un jugador muere sin posibilidad de revive.</summary>
        public event Action<int> OnPlayerDied;

        /// <summary>Evento cuando todos los jugadores han caído.</summary>
        public event Action OnAllPlayersDead;

        /// <summary>Evento al iniciar una sesión de juego nueva.</summary>
        public event Action<int> OnGameSessionStarted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            transform.SetParent(null); // Must be root for DontDestroyOnLoad
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePlayersAlive();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start()
        {
            if (_currentState == GameState.Boot)
                SetState(GameState.MainMenu);
        }

        /// <summary>
        /// Inicia una partida con el número de jugadores indicado (1–4).
        /// </summary>
        public void StartGameSession(int playerCount)
        {
            _playerCount = Mathf.Clamp(playerCount, 1, _maxPlayers);
            CurrentWave = 0;
            InitializePlayersAlive();
            SetState(GameState.Playing);
            OnGameSessionStarted?.Invoke(_playerCount);
            LoadGameWorld();
        }

        /// <summary>
        /// Carga el menú principal y resetea el estado.
        /// </summary>
        public void ReturnToMainMenu()
        {
            SetState(GameState.MainMenu);
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState)
                return;

            GameState previous = _currentState;
            _currentState = newState;
            OnGameStateChanged?.Invoke(previous, newState);
        }

        public void SetWave(int waveNumber)
        {
            CurrentWave = waveNumber;
        }

        public void SetPlayerCount(int count)
        {
            _playerCount = Mathf.Clamp(count, 1, _maxPlayers);
        }

        /// <summary>
        /// Factor de escala de enemigos según jugadores (coop).
        /// </summary>
        public float GetCoopEnemyMultiplier()
        {
            return _playerCount switch
            {
                1 => 1.0f,
                2 => 1.6f,
                3 => 2.2f,
                4 => 3.0f,
                _ => 1.0f
            };
        }

        /// <summary>
        /// Tipos especiales extra por oleada según jugadores.
        /// </summary>
        public int GetCoopSpecialEnemyBonus()
        {
            return _playerCount switch
            {
                2 => 1,
                3 => 2,
                4 => 3,
                _ => 0
            };
        }

        public bool IsPlayerAlive(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= _maxPlayers)
                return false;
            return _playersAlive[playerIndex];
        }

        /// <summary>
        /// Llamado cuando un jugador muere definitivamente (sin revive).
        /// </summary>
        public void OnPlayerDiedHandler(int playerIndex)
        {
            if (playerIndex < 0 || playerIndex >= _maxPlayers)
                return;

            _playersAlive[playerIndex] = false;
            OnPlayerDied?.Invoke(playerIndex);

            if (!AnyPlayerAlive())
            {
                SetState(GameState.GameOver);
                OnAllPlayersDead?.Invoke();
            }
        }

        /// <summary>
        /// Revive a un jugador en coop.
        /// </summary>
        public void RevivePlayer(int playerIndex)
        {
            if (playerIndex >= 0 && playerIndex < _maxPlayers)
                _playersAlive[playerIndex] = true;
        }

        public void PauseGame()
        {
            if (_currentState != GameState.Playing)
                return;

            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (_currentState != GameState.Paused)
                return;

            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        private void InitializePlayersAlive()
        {
            for (int i = 0; i < _maxPlayers; i++)
                _playersAlive[i] = i < _playerCount;
        }

        private bool AnyPlayerAlive()
        {
            for (int i = 0; i < _playerCount; i++)
            {
                if (_playersAlive[i])
                    return true;
            }
            return false;
        }

        private void LoadGameWorld()
        {
            if (_useRoomChain)
            {
                Script_36_SceneLoader.LoadFirstRoom();
            }
            else
            {
                if (SceneManager.GetActiveScene().name != _gameWorldSceneName)
                    SceneManager.LoadScene(_gameWorldSceneName);
            }
        }
    }
}
