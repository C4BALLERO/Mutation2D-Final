using MutationSwarm.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Boot scene controller.
    /// Order: Awake initializes singletons → splash animates → LoadMainMenu.
    /// </summary>
    public class Script_00_BootLoader : MonoBehaviour
    {
        [SerializeField] private string _mainMenuSceneName = "Scene_01_MainMenu";
        [SerializeField] private Script_34_BootSplashUI _splashUI;

        private void Awake()
        {
            // Force singletons to initialize in the correct order.
            _ = Script_01_GameManager.Instance;
            _ = Script_05_SaveManager.Instance;
        }

        private void Start()
        {
            if (_splashUI != null)
            {
                _splashUI.OnSplashComplete += LoadMainMenu;
                _splashUI.enabled = true;
            }
            else
            {
                // Fallback: no splash UI assigned — load immediately
                Invoke(nameof(LoadMainMenu), 0.5f);
            }
        }

        private void LoadMainMenu()
        {
            if (SceneManager.GetActiveScene().name == _mainMenuSceneName)
                return;

            SceneManager.LoadScene(_mainMenuSceneName);
        }

        private void OnDestroy()
        {
            if (_splashUI != null)
                _splashUI.OnSplashComplete -= LoadMainMenu;
        }
    }
}
