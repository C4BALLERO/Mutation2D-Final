#if UNITY_EDITOR
using System.IO;
using MutationSwarm.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Fixes the most common Unity 6 UIToolkit black-screen issue:
    /// UIDocument components require a PanelSettings asset assigned.
    /// Without it, UI renders nothing (black screen, no errors).
    ///
    /// Run: Tools > Mutation Swarm > Fix UI (Black Screen)
    /// </summary>
    public static class MutationSwarmUIFix
    {
        private const string PanelSettingsPath = "Assets/Settings/DefaultPanelSettings.asset";

        private static readonly string[] AllScenes =
        {
            "Assets/_Scenes/Scene_00_Boot.unity",
            "Assets/_Scenes/Scene_01_MainMenu.unity",
            "Assets/_Scenes/Scene_02_GameWorld.unity",
            "Assets/_Scenes/Scene_03_UpgradeMenu.unity",
            "Assets/_Scenes/Rooms/Room_01.unity",
            "Assets/_Scenes/Rooms/Room_02.unity",
            "Assets/_Scenes/Rooms/Room_03.unity",
            "Assets/_Scenes/Rooms/Room_04.unity",
            "Assets/_Scenes/Rooms/Room_05.unity",
            "Assets/_Scenes/Rooms/Room_Boss.unity",
        };

        // ── Main fix ──────────────────────────────────────────────────────

        [MenuItem("Tools/Mutation Swarm/Fix UI (Black Screen) ← Run This First")]
        public static void FixAll()
        {
            PanelSettings ps = GetOrCreatePanelSettings();

            // Fix the currently open scene
            int fixedInCurrent = FixCurrentScene(ps);

            // Fix all other scenes
            int fixedInOther = 0;
            string currentPath = UnityEditor.SceneManagement.EditorSceneManager
                .GetActiveScene().path;

            foreach (string scenePath in AllScenes)
            {
                if (!File.Exists(scenePath)) continue;
                if (scenePath == currentPath) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int count  = FixCurrentScene(ps);
                if (count > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    fixedInOther += count;
                }
            }

            // Reopen the original scene
            if (!string.IsNullOrEmpty(currentPath) && File.Exists(currentPath))
                EditorSceneManager.OpenScene(currentPath, OpenSceneMode.Single);

            AssetDatabase.SaveAssets();

            string msg = $"PanelSettings assigned to {fixedInCurrent + fixedInOther} UIDocument(s).\n" +
                         $"Press Play on Scene_00_Boot to test the full flow.";
            EditorUtility.DisplayDialog("UI Fix Complete", msg, "OK");
            Debug.Log($"[UIFix] Fixed {fixedInCurrent + fixedInOther} UIDocuments across all scenes.");
        }

        // ── Fix current scene only (useful from inspector) ────────────────

        [MenuItem("Tools/Mutation Swarm/Fix UI Current Scene Only")]
        public static void FixCurrentSceneOnly()
        {
            PanelSettings ps = GetOrCreatePanelSettings();
            int count = FixCurrentScene(ps);
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            Debug.Log($"[UIFix] Fixed {count} UIDocument(s) in current scene.");
        }

        private static int FixCurrentScene(PanelSettings ps)
        {
            int count = 0;
#if UNITY_6000_0_OR_NEWER
            UIDocument[] docs = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
#else
            UIDocument[] docs = Object.FindObjectsOfType<UIDocument>();
#endif
            foreach (UIDocument doc in docs)
            {
                if (doc.panelSettings == null)
                {
                    doc.panelSettings = ps;
                    EditorUtility.SetDirty(doc);
                    count++;
                    Debug.Log($"[UIFix] Assigned PanelSettings to {doc.gameObject.name}.");
                }
            }
            return count;
        }

        // ── PanelSettings creation ────────────────────────────────────────

        /// <summary>
        /// Finds an existing PanelSettings in the project, or creates a
        /// game-appropriate default at Assets/Settings/DefaultPanelSettings.asset.
        /// </summary>
        public static PanelSettings GetOrCreatePanelSettings()
        {
            // 1. Look for any existing PanelSettings in the project
            string[] guids = AssetDatabase.FindAssets("t:PanelSettings");
            foreach (string guid in guids)
            {
                PanelSettings found = AssetDatabase.LoadAssetAtPath<PanelSettings>(
                    AssetDatabase.GUIDToAssetPath(guid));
                if (found != null)
                    return found;
            }

            // 2. Try the known path
            PanelSettings existing = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (existing != null) return existing;

            // 3. Create a default PanelSettings
            PanelSettings ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode            = PanelScaleMode.ScaleWithScreenSize;
            ps.referenceResolution  = new Vector2Int(1920, 1080);
            ps.screenMatchMode      = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.match                = 0.5f;
            ps.sortingOrder         = 0;

            if (!Directory.Exists("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");

            AssetDatabase.CreateAsset(ps, PanelSettingsPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[UIFix] Created DefaultPanelSettings at {PanelSettingsPath}.");
            return ps;
        }
    }
}
#endif
