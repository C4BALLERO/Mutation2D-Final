using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Panel de Control Centralizado — Todos los setup en un solo lugar.
    /// Reemplaza los 23 menús individuales en UN SOLO MENU.
    /// Acceso: Tools > Mutation Swarm
    /// </summary>
    public class MutationSwarmControlPanel : EditorWindow
    {
        private Vector2 _scrollPosition;
        private int _selectedTab = 0;
        private readonly string[] _tabs = { "🚀 Setup", "🏗️ Build", "🎬 Escenas", "🎨 Arte" };

        [MenuItem("Tools/Mutation Swarm")]
        public static void ShowWindow()
        {
            var window = GetWindow<MutationSwarmControlPanel>("Mutation Swarm Control");
            window.minSize = new Vector2(400, 600);
        }

        private void OnGUI()
        {
            GUILayout.Label("Mutation Swarm 2D — Control Panel", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs);
            EditorGUILayout.Space();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            switch (_selectedTab)
            {
                case 0:
                    DrawSetupTab();
                    break;
                case 1:
                    DrawBuildTab();
                    break;
                case 2:
                    DrawScenesTab();
                    break;
                case 3:
                    DrawArtTab();
                    break;
            }

            GUILayout.EndScrollView();
        }

        private void DrawSetupTab()
        {
            GUILayout.Label("🚀 Setup Inicial", EditorStyles.boldLabel);
            DrawButton("Setup Completo del Proyecto", () => CallMethod("MutationSwarmProjectSetup", "SetupCompleteProject"), 40);
            DrawButton("Full Game Setup (Playable)", () => CallMethod("MutationSwarmFullSetup", "ExecuteFullSetup"), 40);

            EditorGUILayout.Space(10);
            GUILayout.Label("🔧 Fixes (Ejecutar primero)", EditorStyles.boldLabel);

            DrawButton("Fix UI (Black Screen) ← RUN THIS FIRST", () => 
                CallMethod("MutationSwarmUIFix", "FixAll"), 40);

            DrawButton("Fix UI Current Scene Only", () => 
                CallMethod("MutationSwarmUIFix", "FixCurrentSceneOnly"), 35);

            DrawButton("Fix _Player Prefab", () => 
                CallMethod("MutationSwarmPlayerPrefabFixer", "FixPlayerPrefab"), 35);

            EditorGUILayout.Space(10);
            GUILayout.Label("🎮 Spawn Points", EditorStyles.boldLabel);

            DrawButton("Setup Spawn Points in All Rooms", () => 
                CallMethod("MutationSwarmRoomSpawnPointSetup", "SetupAllRoomSpawnPoints"), 35);

            DrawButton("Setup Spawn Points in Current Room", () => 
                CallMethod("MutationSwarmRoomSpawnPointSetup", "SetupCurrentRoomSpawnPoints"), 35);
        }

        private void DrawBuildTab()
        {
            GUILayout.Label("🏗️ Build & Export", EditorStyles.boldLabel);
            DrawButton("Build All Content (Kenney + Scenes)", () => 
                CallMethod("BuildPipeline", "BuildAllContent"), 40);
            
            DrawButton("Build All Rooms", () => 
                CallMethod("MutationSwarmRoomBuilder", "BuildAllRooms"), 35);
            
            DrawButton("Build Art Level (Scene_02)", () => 
                CallMethod("MutationSwarmArtLevelBuilder", "BuildArtLevel"), 35);

            EditorGUILayout.Space(10);
            GUILayout.Label("📦 Windows Executable", EditorStyles.boldLabel);

            DrawButton("Build Windows (Debug)", () => 
                CallMethod("BuildPipeline", "BuildWindows"), 35);

            DrawButton("Build Windows (Release)", () => 
                CallMethod("BuildPipeline", "BuildWindowsRelease"), 35);
        }

        private void DrawScenesTab()
        {
            GUILayout.Label("🎬 Setup Escenas", EditorStyles.boldLabel);

            DrawButton("Setup All Scenes", () => 
                CallMethod("MutationSwarmSceneSetup", "SetupAllScenes"), 40);

            EditorGUILayout.Space(10);
            GUILayout.Label("Escenas Individuales", EditorStyles.boldLabel);

            DrawButton("Setup Scene_00_Boot", () => 
                CallMethod("MutationSwarmSceneSetup", "SetupBootScene"), 35);

            DrawButton("Setup Scene_01_MainMenu", () => 
                CallMethod("MutationSwarmSceneSetup", "SetupMainMenuScene"), 35);

            DrawButton("Setup Scene_02_GameWorld", () => 
                CallMethod("MutationSwarmSceneSetup", "SetupGameWorldScene"), 35);

            DrawButton("Setup Scene_03_UpgradeMenu", () => 
                CallMethod("MutationSwarmSceneSetup", "SetupUpgradeMenuScene"), 35);
        }

        private void DrawArtTab()
        {
            GUILayout.Label("🎨 Arte & Sprites", EditorStyles.boldLabel);

            DrawButton("Import Art Package Settings", () => 
                CallMethod("MutationSwarmArtLevelBuilder", "ImportArtPackageSettings"), 35);

            DrawButton("Build Enemy Sprites (Art Bible)", () => 
                CallMethod("MutationSwarmEnemyArtSetup", "BuildEnemySprites"), 35);

            DrawButton("Build Enemy Animations (Full Pipeline)", () => 
                CallMethod("MutationSwarmEnemyAnimationSetup", "BuildEnemyAnimations"), 35);

            DrawButton("Build Player Sprite (Argos Armor)", () => 
                CallMethod("MutationSwarmPlayerArtSetup", "BuildPlayerSprite"), 35);

            EditorGUILayout.Space(10);
            GUILayout.Label("📥 Imports", EditorStyles.boldLabel);

            DrawButton("Import & Slice Player Sprite Sheets", () => 
                CallMethod("MutationSwarmPlayerSpriteImporter", "ImportAndSlice"), 35);

            DrawButton("Inspect Player Sprite Sheets", () => 
                CallMethod("MutationSwarmPlayerSpriteImporter", "InspectSpriteSheets"), 35);

            DrawButton("Import Guns Pack + Weapon Shop", () => 
                CallMethod("MutationSwarmGunsImport", "ImportGunsPackAndWeaponShop"), 35);

            DrawButton("Build Kenney UI + Playable Level", () => 
                CallMethod("MutationSwarmKenneyGameplaySetup", "BuildKenneyUI"), 35);
        }

        private void DrawButton(string label, System.Action onClick, int height = 35)
        {
            if (GUILayout.Button(label, GUILayout.Height(height)))
            {
                try
                {
                    onClick?.Invoke();
                }
                catch (System.Exception ex)
                {
                    EditorUtility.DisplayDialog("Error", $"Error executing: {ex.Message}", "OK");
                }
            }
        }

        private void CallMethod(string className, string methodName)
        {
            var type = System.Type.GetType($"MutationSwarm.Editor.{className}");
            if (type == null)
            {
                Debug.LogError($"[ControlPanel] No se encontró clase: {className}");
                return;
            }

            var method = type.GetMethod(methodName, 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            if (method == null)
            {
                Debug.LogError($"[ControlPanel] No se encontró método: {className}.{methodName}");
                return;
            }

            method.Invoke(null, null);
        }
    }
}
