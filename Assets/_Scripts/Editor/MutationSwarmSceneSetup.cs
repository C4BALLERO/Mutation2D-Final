#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using MutationSwarm.Core;
using MutationSwarm.Entities;
using MutationSwarm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Rebuilds all 4 main scenes from scratch with correct components and references.
    ///
    /// Run via: Tools > Mutation Swarm > Setup All Scenes
    ///
    /// Each scene gets: Camera, Managers, UIDocument (with correct UXML+USS), Controllers.
    /// Scene_02_GameWorld delegates to MutationSwarmArtLevelBuilder for environment.
    /// </summary>
    public static class MutationSwarmSceneSetup
    {
        private const string ScenesPath = "Assets/_Scenes";
        private const string UiPath     = "Assets/_Scripts/UI";

        // ── Entry points ──────────────────────────────────────────────────

        [MenuItem("Tools/Mutation Swarm/Setup All Scenes")]
        public static void SetupAll()
        {
            SetupBoot();
            SetupMainMenu();
            SetupGameWorld();
            SetupUpgradeMenu();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MutationSwarm] All 4 scenes set up successfully.");
        }

        [MenuItem("Tools/Mutation Swarm/Setup Scene_00_Boot")]
        public static void SetupBoot()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera (orthographic, black background for splash)
            CreateCamera(new Color(0.016f, 0.02f, 0.039f), 5.4f);

            // Managers that need to exist in boot
            GameObject managersRoot = new("_Managers");
            CreateChild(managersRoot, "GameManager", typeof(Script_01_GameManager));
            CreateChild(managersRoot, "SaveManager",  typeof(Script_05_SaveManager));
            CreateChild(managersRoot, "AudioManager", typeof(Script_AudioManager));

            // Boot UI — UIDocument with BootSplash.uxml
            GameObject bootUi = new("_BootUI");
            UIDocument uiDoc  = bootUi.AddComponent<UIDocument>();
            AssignUxml(uiDoc, "BootSplash.uxml", "BootSplash.uss");

            Script_34_BootSplashUI splashUI = bootUi.AddComponent<Script_34_BootSplashUI>();

            // Boot loader — references the splash component
            GameObject bootGo  = new("_Boot");
            Script_00_BootLoader loader = bootGo.AddComponent<Script_00_BootLoader>();
            SerializedObject so = new(loader);
            so.FindProperty("_mainMenuSceneName").stringValue = "Scene_01_MainMenu";
            so.FindProperty("_splashUI").objectReferenceValue = splashUI;
            so.ApplyModifiedPropertiesWithoutUndo();

            string path = $"{ScenesPath}/Scene_00_Boot.unity";
            EditorSceneManager.SaveScene(scene, path);
            EnsureInBuildSettings(path);
            Debug.Log("[MutationSwarm] Scene_00_Boot rebuilt.");
        }

        [MenuItem("Tools/Mutation Swarm/Setup Scene_01_MainMenu")]
        public static void SetupMainMenu()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera — very dark biopunk background
            CreateCamera(new Color(0.016f, 0.02f, 0.039f), 5.4f);

            // GameManager must persist into this scene from Boot
            // Add a lightweight reference in case Boot wasn't played
            GameObject managersRoot = new("_Managers");
            CreateChild(managersRoot, "GameManager", typeof(Script_01_GameManager));
            CreateChild(managersRoot, "SaveManager",  typeof(Script_05_SaveManager));
            CreateChild(managersRoot, "AudioManager", typeof(Script_AudioManager));

            // UI GameObject
            GameObject menuUi = new("_MainMenuUI");
            UIDocument uiDoc  = menuUi.AddComponent<UIDocument>();
            AssignUxml(uiDoc, "MainMenu.uxml", "MainMenu.uss");

            // Controllers
            Script_30_MainMenuController controller = menuUi.AddComponent<Script_30_MainMenuController>();
            SerializedObject soCtr = new(controller);
            soCtr.FindProperty("_menuDocument").objectReferenceValue = uiDoc;
            soCtr.ApplyModifiedPropertiesWithoutUndo();

            menuUi.AddComponent<Script_35_MainMenuAnimator>();

            // Background particle system placeholder
            GameObject bgGo = new("_Background");
            Camera cam = Object.FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
                bgSr.sortingOrder   = -100;
                bgGo.transform.position = new Vector3(0f, 0f, 1f);
            }

            string path = $"{ScenesPath}/Scene_01_MainMenu.unity";
            EditorSceneManager.SaveScene(scene, path);
            EnsureInBuildSettings(path);
            Debug.Log("[MutationSwarm] Scene_01_MainMenu rebuilt.");
        }

        [MenuItem("Tools/Mutation Swarm/Setup Scene_02_GameWorld")]
        public static void SetupGameWorld()
        {
            // Delegate entirely to the existing art-level builder which handles
            // managers, platforms, spawns, HUD, and all environment.
            MutationSwarmArtLevelBuilder.BuildArtLevel();
            Debug.Log("[MutationSwarm] Scene_02_GameWorld rebuilt via ArtLevelBuilder.");
        }

        [MenuItem("Tools/Mutation Swarm/Setup Scene_03_UpgradeMenu")]
        public static void SetupUpgradeMenu()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Very dark camera (almost black — upgrade screen covers everything)
            CreateCamera(new Color(0.016f, 0.02f, 0.039f), 5.4f);

            // Minimal managers (GameManager already persists via DontDestroyOnLoad)
            GameObject managersRoot = new("_Managers");
            CreateChild(managersRoot, "GameManager", typeof(Script_01_GameManager));
            CreateChild(managersRoot, "AudioManager", typeof(Script_AudioManager));

            // Upgrade UI
            GameObject upgradeUi = new("_UpgradeUI");
            UIDocument uiDoc     = upgradeUi.AddComponent<UIDocument>();
            AssignUxml(uiDoc, "UpgradeMenu.uxml", "UpgradeMenu.uss");

            Script_26_UpgradeMenuUI upgradeController = upgradeUi.AddComponent<Script_26_UpgradeMenuUI>();
            SerializedObject soUpg = new(upgradeController);
            soUpg.FindProperty("_doc").objectReferenceValue = uiDoc;

            // Wire upgrade pool from ScriptableObjects
            WireUpgradePool(soUpg);
            soUpg.ApplyModifiedPropertiesWithoutUndo();

            string path = $"{ScenesPath}/Scene_03_UpgradeMenu.unity";
            EditorSceneManager.SaveScene(scene, path);
            EnsureInBuildSettings(path);
            Debug.Log("[MutationSwarm] Scene_03_UpgradeMenu rebuilt.");
        }

        // ── Helper: upgrade pool wiring ──────────────────────────────────

        private static void WireUpgradePool(SerializedObject soUpg)
        {
            string[] soGuids = AssetDatabase.FindAssets(
                "t:ScriptableObject", new[] { "Assets/_ScriptableObjects/Upgrades" });

            SerializedProperty poolProp = soUpg.FindProperty("_upgradePool");
            if (poolProp == null) return;

            poolProp.arraySize = soGuids.Length;
            for (int i = 0; i < soGuids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(soGuids[i]);
                Object so = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                poolProp.GetArrayElementAtIndex(i).objectReferenceValue = so;
            }
        }

        // ── Scene helpers ─────────────────────────────────────────────────

        private static void CreateCamera(Color bg, float ortho)
        {
            GameObject camGo = new("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = ortho;
            cam.backgroundColor  = bg;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            camGo.AddComponent<AudioListener>();
        }

        private static void CreateChild(GameObject parent, string name, System.Type type)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent.transform);
            child.AddComponent(type);
        }

        private static void AssignUxml(UIDocument doc, string uxmlFile, string ussFile)
        {
            VisualTreeAsset uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{UiPath}/{uxmlFile}");

            if (uxml == null)
            {
                Debug.LogWarning($"[SceneSetup] {uxmlFile} not found at {UiPath}/");
                return;
            }

            doc.visualTreeAsset = uxml;

            // PanelSettings is required in Unity 6 for UIDocument to render.
            // Without it the screen stays black with no errors.
            if (doc.panelSettings == null)
                doc.panelSettings = MutationSwarmUIFix.GetOrCreatePanelSettings();
        }

        private static void EnsureInBuildSettings(string scenePath)
        {
            if (!File.Exists(scenePath)) return;
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
                if (s.path == scenePath) return;
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
