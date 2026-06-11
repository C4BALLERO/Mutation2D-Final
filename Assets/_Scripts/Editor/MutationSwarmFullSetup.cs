#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MutationSwarm.Core;
using MutationSwarm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// One-click full game setup.
    /// Run via: Tools > Mutation Swarm > Full Game Setup (Playable)
    ///
    /// What this does:
    ///  1. Assigns AC_Player.controller to Prefab_Player.prefab's Animator
    ///  2. For each room scene (Room_01…Room_Boss):
    ///     a. Adds Script_37_RoomBootstrap with player prefab + spawn point
    ///     b. Adds HUD (UIDocument + Script_25_HUDController) if missing
    ///     c. Sets WaveManager._upgradeSceneName = "" (no mid-room upgrade menus)
    ///     d. Assigns SO_WaveConfig_Default to WaveManager._config
    ///     e. Ensures scene is in Build Settings
    ///  3. Adds Boot/MainMenu/GameWorld/UpgradeMenu to Build Settings
    ///  4. Logs a full report of what was fixed
    /// </summary>
    public static class MutationSwarmFullSetup
    {
        private const string PlayerPrefabPath   = "Assets/_Prefabs/Player/Prefab_Player.prefab";
        private const string ControllerPath     = "Assets/_Art/Animations/Player/AC_Player.controller";
        private const string WaveConfigPath     = "Assets/_ScriptableObjects/Waves/SO_WaveConfig_Default.asset";
        private const string UiPath             = "Assets/_Scripts/UI";
        private const string RoomScenesRoot     = "Assets/_Scenes/Rooms";
        private const string MainScenesRoot     = "Assets/_Scenes";

        private static readonly string[] RoomNames =
        {
            "Room_01", "Room_02", "Room_03", "Room_04", "Room_05", "Room_Boss"
        };

        private static readonly string[] MainScenes =
        {
            $"{MainScenesRoot}/Scene_00_Boot.unity",
            $"{MainScenesRoot}/Scene_01_MainMenu.unity",
            $"{MainScenesRoot}/Scene_02_GameWorld.unity",
            $"{MainScenesRoot}/Scene_03_UpgradeMenu.unity",
        };

        // ── Entry point ───────────────────────────────────────────────────

        [MenuItem("Tools/Mutation Swarm/Full Game Setup (Playable)")]
        public static void SetupAll()
        {
            int fixCount = 0;

            fixCount += ConfigurePlayerPrefab();
            fixCount += ConfigureEnemyPrefabs();

            foreach (string roomName in RoomNames)
                fixCount += SetupRoomScene(roomName);

            EnsureAllScenesInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Full Game Setup",
                $"Setup complete.\n{fixCount} items fixed or configured.\n\nPress Play to test!",
                "OK");

            Debug.Log($"[FullSetup] Done. {fixCount} items configured.");
        }

        // ── Player prefab ─────────────────────────────────────────────────

        private static int ConfigurePlayerPrefab()
        {
            int fixes = 0;

            RuntimeAnimatorController ctrl =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (ctrl == null)
            {
                Debug.LogWarning($"[FullSetup] AC_Player.controller not found at {ControllerPath}.");
                return 0;
            }

            GameObject prefab = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[FullSetup] Player prefab not found at {PlayerPrefabPath}.");
                return 0;
            }

            Animator anim = prefab.GetComponent<Animator>();
            if (anim == null)
                anim = prefab.AddComponent<Animator>();

            if (anim.runtimeAnimatorController == null)
            {
                anim.runtimeAnimatorController = ctrl;
                PrefabUtility.SaveAsPrefabAsset(prefab, PlayerPrefabPath);
                fixes++;
                Debug.Log("[FullSetup] Assigned AC_Player.controller to Prefab_Player.");
            }

            PrefabUtility.UnloadPrefabContents(prefab);
            return fixes;
        }

        // ── Enemy prefabs ─────────────────────────────────────────────────

        private static int ConfigureEnemyPrefabs()
        {
            int fixes = 0;
            string[] enemyPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Prefabs/Enemies" });

            // Try to find an existing enemy Animator Controller from the Build Animations tool
            string[] controllerGuids = AssetDatabase.FindAssets(
                "AC_Enemy t:AnimatorController", new[] { "Assets/_Art/Animations/Enemies" });

            RuntimeAnimatorController droneCtrl = null;
            if (controllerGuids.Length > 0)
            {
                string ctrlPath = AssetDatabase.GUIDToAssetPath(controllerGuids[0]);
                droneCtrl = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
            }

            foreach (string guid in enemyPrefabs)
            {
                string path   = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = PrefabUtility.LoadPrefabContents(path);
                if (go == null) continue;

                bool changed = false;

                Animator anim = go.GetComponent<Animator>();
                if (anim == null)
                {
                    anim    = go.AddComponent<Animator>();
                    changed = true;
                }

                // Assign a controller if this enemy has a matching one, else use drone's
                if (anim.runtimeAnimatorController == null)
                {
                    string enemyName = Path.GetFileNameWithoutExtension(path); // e.g., Prefab_Enemy_Drone
                    string typeName  = enemyName.Replace("Prefab_Enemy_", ""); // e.g., Drone

                    // Look for AC_Enemy_Drone.controller etc.
                    RuntimeAnimatorController typeCtrl = FindEnemyController(typeName);
                    anim.runtimeAnimatorController = typeCtrl ?? droneCtrl;

                    if (anim.runtimeAnimatorController != null)
                    {
                        changed = true;
                        Debug.Log($"[FullSetup] Assigned controller to {enemyName}.");
                    }
                }

                // Ensure Animator culling mode is set for performance
                if (anim.cullingMode != AnimatorCullingMode.CullUpdateTransforms)
                {
                    anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    changed = true;
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(go, path);
                    fixes++;
                }

                PrefabUtility.UnloadPrefabContents(go);
            }

            return fixes;
        }

        private static RuntimeAnimatorController FindEnemyController(string typeName)
        {
            string[] guids = AssetDatabase.FindAssets(
                $"AC_Enemy_{typeName} t:AnimatorController", new[] { "Assets/_Art/Animations/Enemies" });
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        // ── Room scene setup ──────────────────────────────────────────────

        private static int SetupRoomScene(string roomName)
        {
            string scenePath = $"{RoomScenesRoot}/{roomName}.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"[FullSetup] Room scene not found: {scenePath}. Run 'Build All Rooms' first.");
                return 0;
            }

            int fixes = 0;
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            fixes += FixWaveManager(roomName);
            fixes += AddRoomBootstrap();
            fixes += AddHUDIfMissing();
            fixes += AddLevelExitIfMissing(roomName);

            EditorSceneManager.SaveScene(scene);
            EnsureSceneInBuildSettings(scenePath);

            Debug.Log($"[FullSetup] Room {roomName}: {fixes} items fixed.");
            return fixes;
        }

        private static int FixWaveManager(string roomName)
        {
            int fixes = 0;
            Script_02_WaveManager wm = Object.FindFirstObjectByType<Script_02_WaveManager>();
            if (wm == null) return 0;

            SerializedObject so = new(wm);

            // Disable upgrade menu in rooms — exit handles room transitions
            SerializedProperty upgradeProp = so.FindProperty("_upgradeSceneName");
            if (upgradeProp != null && upgradeProp.stringValue != "")
            {
                upgradeProp.stringValue = "";
                fixes++;
            }

            // Assign SO_WaveConfig_Default
            SerializedProperty configProp = so.FindProperty("_config");
            if (configProp != null && configProp.objectReferenceValue == null)
            {
                Object waveConfig = AssetDatabase.LoadAssetAtPath<Object>(WaveConfigPath);
                if (waveConfig != null)
                {
                    configProp.objectReferenceValue = waveConfig;
                    fixes++;
                }
            }

            // Ensure enemy prefab is set (fall back to Drone)
            SerializedProperty prefabProp = so.FindProperty("_enemyPrefab");
            if (prefabProp != null && prefabProp.objectReferenceValue == null)
            {
                GameObject drone = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Prefabs/Enemies/Prefab_Enemy_Drone.prefab");
                if (drone != null)
                {
                    prefabProp.objectReferenceValue = drone;
                    fixes++;
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            return fixes;
        }

        private static int AddRoomBootstrap()
        {
            if (Object.FindFirstObjectByType<Script_37_RoomBootstrap>() != null)
                return 0; // already present

            GameObject go = new("_RoomBootstrap");
            Script_37_RoomBootstrap bootstrap = go.AddComponent<Script_37_RoomBootstrap>();

            SerializedObject so = new(bootstrap);

            // Assign player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
            SerializedProperty prefabProp = so.FindProperty("_playerPrefab");
            if (prefabProp != null)
                prefabProp.objectReferenceValue = playerPrefab;

            // Find p1 spawn point in current scene
            Transform p1 = FindTransformNamed("p1");
            SerializedProperty spawnProp = so.FindProperty("_spawnPoint");
            if (spawnProp != null && p1 != null)
                spawnProp.objectReferenceValue = p1;

            so.ApplyModifiedPropertiesWithoutUndo();

            Debug.Log($"[FullSetup] Added RoomBootstrap to scene.");
            return 1;
        }

        private static int AddHUDIfMissing()
        {
            if (Object.FindFirstObjectByType<Script_25_HUDController>() != null)
                return 0;

            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UiPath}/HUD_Main.uxml");
            if (uxml == null)
            {
                Debug.LogWarning("[FullSetup] HUD_Main.uxml not found — skipping HUD.");
                return 0;
            }

            GameObject hudGo  = new("_HUD");
            UIDocument uiDoc  = hudGo.AddComponent<UIDocument>();
            uiDoc.visualTreeAsset = uxml;
            uiDoc.sortingOrder    = 100;
            uiDoc.panelSettings   = MutationSwarmUIFix.GetOrCreatePanelSettings();

            Script_25_HUDController hud = hudGo.AddComponent<Script_25_HUDController>();

            // Wire the UIDocument reference
            SerializedObject soHud = new(hud);
            SerializedProperty docProp = soHud.FindProperty("_uiDocument");
            if (docProp != null)
            {
                docProp.objectReferenceValue = uiDoc;
                soHud.ApplyModifiedPropertiesWithoutUndo();
            }

            Debug.Log("[FullSetup] Added HUD to scene.");
            return 1;
        }

        private static int AddLevelExitIfMissing(string roomName)
        {
            MutationSwarm.Rooms.LevelExit existing = Object.FindFirstObjectByType<MutationSwarm.Rooms.LevelExit>();
            if (existing != null)
                return 0;

            // Determine next scene
            string[] chain = Script_36_SceneLoader.GetRoomChain();
            int idx = System.Array.IndexOf(chain, roomName);
            string nextScene = (idx >= 0 && idx < chain.Length - 1)
                ? chain[idx + 1]
                : "Scene_01_MainMenu";

            // Load prefab or create from scratch
            GameObject exitPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/_Prefabs/Rooms/LevelExit.prefab");

            GameObject exitGo;
            if (exitPrefab != null)
            {
                exitGo = PrefabUtility.InstantiatePrefab(exitPrefab) as GameObject;
            }
            else
            {
                exitGo = new("LevelExit");
                exitGo.AddComponent<MutationSwarm.Rooms.LevelExit>();
                var col = exitGo.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1.5f, 2.5f);
            }

            if (exitGo != null)
            {
                exitGo.transform.position = new Vector3(7f, -2.8f, 0f);

                MutationSwarm.Rooms.LevelExit exit = exitGo.GetComponent<MutationSwarm.Rooms.LevelExit>();
                if (exit != null)
                {
                    SerializedObject so = new(exit);
                    so.FindProperty("_nextScene").stringValue       = nextScene;
                    so.FindProperty("_requireWaveClear").boolValue  = false; // open by default
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            Debug.Log($"[FullSetup] Added LevelExit ({nextScene}) to {roomName}.");
            return 1;
        }

        // ── Build Settings ────────────────────────────────────────────────

        private static void EnsureAllScenesInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            // Main scenes first
            foreach (string path in MainScenes)
                AddSceneIfMissing(scenes, path);

            // Room scenes
            foreach (string roomName in RoomNames)
            {
                string path = $"{RoomScenesRoot}/{roomName}.unity";
                AddSceneIfMissing(scenes, path);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("[FullSetup] Build Settings updated.");
        }

        private static void AddSceneIfMissing(List<EditorBuildSettingsScene> list, string path)
        {
            if (!File.Exists(path)) return;
            foreach (var s in list)
                if (s.path == path) return;
            list.Add(new EditorBuildSettingsScene(path, true));
        }

        private static void EnsureSceneInBuildSettings(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            AddSceneIfMissing(scenes, path);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ── Scene utilities ───────────────────────────────────────────────

        private static Transform FindTransformNamed(string goName)
        {
            // Search in _SpawnPoints first
            GameObject spawnRoot = GameObject.Find("_SpawnPoints");
            if (spawnRoot != null)
            {
                foreach (Transform child in spawnRoot.transform)
                    if (child.name == goName) return child;
            }
            GameObject found = GameObject.Find(goName);
            return found != null ? found.transform : null;
        }
    }
}
#endif
