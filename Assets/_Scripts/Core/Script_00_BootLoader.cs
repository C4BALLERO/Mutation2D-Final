using UnityEngine;
using UnityEngine.SceneManagement;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Escena de arranque: asegura singletons y carga el menú principal.
    /// </summary>
    public class Script_00_BootLoader : MonoBehaviour
    {
        [SerializeField] private string _mainMenuSceneName = "Scene_01_MainMenu";
        [SerializeField] private float _delayBeforeLoad = 0.1f;

        private void Start()
        {
            Invoke(nameof(LoadMainMenu), _delayBeforeLoad);
        }

        private void LoadMainMenu()
        {
            if (SceneManager.GetActiveScene().name == _mainMenuSceneName)
                return;

            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}
