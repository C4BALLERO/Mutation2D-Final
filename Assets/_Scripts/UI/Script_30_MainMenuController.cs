using MutationSwarm.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.UI
{
    /// <summary>
    /// Controla la UI de la escena de inicio (Scene_01_MainMenu).
    /// </summary>
    public class Script_30_MainMenuController : MonoBehaviour
    {
        [SerializeField] private UIDocument _menuDocument;
        [SerializeField] private int _selectedPlayerCount = 1;

        private Label _playerCountLabel;

        private void OnEnable()
        {
            if (_menuDocument == null)
                _menuDocument = GetComponent<UIDocument>();
            if (_menuDocument == null)
                return;

            VisualElement root = _menuDocument.rootVisualElement;
            Button play    = root.Q<Button>("BtnPlay");
            Button quit    = root.Q<Button>("BtnQuit");
            Button options = root.Q<Button>("BtnOptions");
            Button plus    = root.Q<Button>("BtnPlayersPlus");
            Button minus   = root.Q<Button>("BtnPlayersMinus");
            _playerCountLabel = root.Q<Label>("LblPlayers");

            if (play    != null) play.clicked    += OnPlayClicked;
            if (quit    != null) quit.clicked    += OnQuitClicked;
            if (options != null) options.clicked += OnOptionsClicked;
            if (plus    != null) plus.clicked    += IncreasePlayers;
            if (minus   != null) minus.clicked   += DecreasePlayers;
            RefreshPlayerCountLabel();
        }

        private void OnOptionsClicked()
        {
            // Reserved for future options screen
            Debug.Log("[MainMenu] Opciones — pendiente de implementar.");
        }

        private void OnPlayClicked()
        {
            if (Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.StartGameSession(_selectedPlayerCount);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void IncreasePlayers()
        {
            _selectedPlayerCount = Mathf.Clamp(_selectedPlayerCount + 1, 1, 4);
            RefreshPlayerCountLabel();
        }

        private void DecreasePlayers()
        {
            _selectedPlayerCount = Mathf.Clamp(_selectedPlayerCount - 1, 1, 4);
            RefreshPlayerCountLabel();
        }

        private void RefreshPlayerCountLabel()
        {
            if (_playerCountLabel == null) return;
            string suffix = _selectedPlayerCount == 1 ? "JUGADOR" : "JUGADORES";
            _playerCountLabel.text = $"{_selectedPlayerCount} {suffix}";
        }
    }
}
