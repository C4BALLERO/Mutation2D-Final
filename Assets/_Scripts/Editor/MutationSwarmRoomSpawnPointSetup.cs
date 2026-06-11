using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using MutationSwarm.Core;
using System.IO;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Setup automático de SpawnPointChild en todas las habitaciones.
    /// Agrega el componente a cada spawn point y configura Is Player Spawn.
    /// </summary>
    public class MutationSwarmRoomSpawnPointSetup
    {
        [MenuItem("Tools/Mutation Swarm/Setup Spawn Points in All Rooms")]
        public static void SetupAllRoomSpawnPoints()
        {
            string[] roomScenes = new[]
            {
                "Assets/_Scenes/Rooms/Room_01.unity",
                "Assets/_Scenes/Rooms/Room_02.unity",
                "Assets/_Scenes/Rooms/Room_03.unity",
                "Assets/_Scenes/Rooms/Room_04.unity",
                "Assets/_Scenes/Rooms/Room_05.unity",
                "Assets/_Scenes/Rooms/Room_Boss.unity",
            };

            string currentScene = EditorSceneManager.GetActiveScene().path;
            int totalSetup = 0;

            foreach (string roomPath in roomScenes)
            {
                if (!File.Exists(roomPath))
                {
                    Debug.LogWarning($"[RoomSpawnPointSetup] Escena no encontrada: {roomPath}");
                    continue;
                }

                Scene scene = EditorSceneManager.OpenScene(roomPath, OpenSceneMode.Single);
                int setupCount = SetupSpawnPointsInScene();
                totalSetup += setupCount;

                if (setupCount > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[RoomSpawnPointSetup] ✅ {Path.GetFileNameWithoutExtension(roomPath)}: {setupCount} spawn points configurados");
                }
            }

            // Reabrir escena original
            if (!string.IsNullOrEmpty(currentScene) && File.Exists(currentScene))
                EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);

            Debug.Log($"[RoomSpawnPointSetup] ✅ TOTAL: {totalSetup} spawn points configurados en todas las habitaciones");
            EditorUtility.DisplayDialog("Setup Complete", $"Se configuraron {totalSetup} spawn points en todas las habitaciones.", "OK");
        }

        /// <summary>
        /// Configura spawn points en la escena actual.
        /// </summary>
        [MenuItem("Tools/Mutation Swarm/Setup Spawn Points in Current Room")]
        public static void SetupCurrentRoomSpawnPoints()
        {
            int count = SetupSpawnPointsInScene();
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log($"[RoomSpawnPointSetup] ✅ Se configuraron {count} spawn points en la escena actual");
        }

        private static int SetupSpawnPointsInScene()
        {
            GameObject spawnPointsRoot = GameObject.Find("_SpawnPoints");
            if (spawnPointsRoot == null)
            {
                Debug.LogWarning("[RoomSpawnPointSetup] No se encontró _SpawnPoints en la escena");
                return 0;
            }

            int setupCount = 0;

            foreach (Transform child in spawnPointsRoot.transform)
            {
                // Agregar SpawnPointChild si no existe
                SpawnPointChild spawnPoint = child.GetComponent<SpawnPointChild>();
                if (spawnPoint == null)
                {
                    spawnPoint = child.gameObject.AddComponent<SpawnPointChild>();
                    setupCount++;
                }

                // Configurar Is Player Spawn
                bool isPlayer = child.name.StartsWith("p");
                SerializedObject so = new SerializedObject(spawnPoint);
                SerializedProperty prop = so.FindProperty("_isPlayerSpawn");
                prop.boolValue = isPlayer;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return setupCount;
        }
    }
}
