using UnityEngine;
using UnityEngine.SceneManagement;

namespace MutationSwarm.Core
{
    /// <summary>
    /// Centralized scene navigation. All scene transitions go through here.
    /// Room chain: Room_01 → Room_02 → Room_03 → Room_04 → Room_05 → Room_Boss → MainMenu
    /// </summary>
    public static class Script_36_SceneLoader
    {
        private static readonly string[] RoomChain =
        {
            "Room_01",
            "Room_02",
            "Room_03",
            "Room_04",
            "Room_05",
            "Room_Boss",
        };

        public static string[] GetRoomChain() => RoomChain;

        /// <summary>Returns 0-based index of the current scene in the room chain, or -1.</summary>
        public static int CurrentRoomIndex()
        {
            string current = SceneManager.GetActiveScene().name;
            for (int i = 0; i < RoomChain.Length; i++)
                if (RoomChain[i] == current) return i;
            return -1;
        }

        public static bool IsRoomScene() => CurrentRoomIndex() >= 0;

        /// <summary>Loads the first room (Room_01).</summary>
        public static void LoadFirstRoom()
        {
            if (Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.SetState(GameState.Playing);
            SceneManager.LoadScene(RoomChain[0]);
        }

        /// <summary>Loads the next room in the chain. Returns to MainMenu after Room_Boss.</summary>
        public static void LoadNextRoom()
        {
            int idx = CurrentRoomIndex();
            if (idx < 0)
            {
                Debug.LogWarning("[SceneLoader] Not in a room scene — redirecting to Room_01.");
                LoadFirstRoom();
                return;
            }

            if (idx < RoomChain.Length - 1)
                SceneManager.LoadScene(RoomChain[idx + 1]);
            else
                LoadMainMenu(); // After Room_Boss → credits / main menu
        }

        /// <summary>Loads a specific room by index (0-based).</summary>
        public static void LoadRoom(int index)
        {
            int clamped = Mathf.Clamp(index, 0, RoomChain.Length - 1);
            SceneManager.LoadScene(RoomChain[clamped]);
        }

        /// <summary>Reloads the current scene (respawn after death).</summary>
        public static void RestartRoom()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>Loads the main menu and resets game state.</summary>
        public static void LoadMainMenu()
        {
            if (Script_01_GameManager.Instance != null)
                Script_01_GameManager.Instance.ReturnToMainMenu();
            else
                SceneManager.LoadScene("Scene_01_MainMenu");
        }
    }
}
