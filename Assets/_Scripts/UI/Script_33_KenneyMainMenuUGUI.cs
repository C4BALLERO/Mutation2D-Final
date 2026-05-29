using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MutationSwarm.UI
{
    /// <summary>
    /// Menú principal con botones uGUI estilo Kenney (colores claros).
    /// </summary>
    public class Script_33_KenneyMainMenuUGUI : MonoBehaviour
    {
        [SerializeField] private Button _btnPlay;
        [SerializeField] private Button _btnQuit;
        [SerializeField] private Button _btnPlus;
        [SerializeField] private Button _btnMinus;
        [SerializeField] private Text _lblPlayers;
        [SerializeField] private int _selectedPlayers = 1;

        private void Awake()
        {
            if (_btnPlay != null) _btnPlay.onClick.AddListener(OnPlay);
            if (_btnQuit != null) _btnQuit.onClick.AddListener(OnQuit);
            if (_btnPlus != null) _btnPlus.onClick.AddListener(() => ChangePlayers(1));
            if (_btnMinus != null) _btnMinus.onClick.AddListener(() => ChangePlayers(-1));
            RefreshLabel();
        }

        private void OnPlay()
        {
            if (Script_01_GameManager.Instance != null)
            {
                Script_01_GameManager.Instance.StartGameSession(_selectedPlayers);
                return;
            }

            SceneManager.LoadScene("Scene_02_GameWorld");
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ChangePlayers(int delta)
        {
            _selectedPlayers = Mathf.Clamp(_selectedPlayers + delta, 1, 4);
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (_lblPlayers != null)
                _lblPlayers.text = $"{_selectedPlayers} Jugador(es)";
        }
    }
}
