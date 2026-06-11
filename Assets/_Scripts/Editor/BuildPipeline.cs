#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MutationSwarm.Editor
{
    /// <summary>
    /// Build de jugador Windows + setup previo del contenido.
    /// </summary>
    public static class MutationSwarmBuildPipeline
    {
        private const string BuildFolder = "Builds/Windows";
        private const string ProductName = "MutationSwarm";

        [MenuItem("Tools/Mutation Swarm/Build All Content (Kenney + Scenes)")]
        public static void BuildAllContent()
        {
            MutationSwarmKenneyGameplaySetup.BuildAll();
            if (!File.Exists("Assets/_Scenes/Scene_00_Boot.unity"))
                MutationSwarmProjectSetup.SetupCompleteProject();
            else
                ConfigureBuildScenes();

            AssetDatabase.SaveAssets();
            Debug.Log("[BuildPipeline] Contenido generado.");
        }

        [MenuItem("Tools/Mutation Swarm/Build Windows")]
        public static void BuildWindows()
        {
            BuildAllContent();
            BuildWindowsPlayerOnly();
        }

        public static void ConfigureBuildScenesForBatch() => ConfigureBuildScenes();

        public static void BuildWindowsPlayerOnly()
        {
            ConfigureBuildScenes();

            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", BuildFolder));
            string exePath = Path.Combine(outputDir, $"{ProductName}.exe");

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            BuildPlayerOptions options = new()
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.AllowDebugging,
                targetGroup = BuildTargetGroup.Standalone
            };

            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                string steamId = Path.Combine(outputDir, "steam_appid.txt");
                File.WriteAllText(steamId, "480");
                Debug.Log($"[BuildPipeline] Build OK: {exePath}");
            }
            else
            {
                Debug.LogError($"[BuildPipeline] Build falló: {summary.result}");
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/Mutation Swarm/Build Windows (Release)")]
        public static void BuildWindowsRelease()
        {
            BuildAllContent();

            string outputDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", BuildFolder, "Release"));
            string exePath = Path.Combine(outputDir, $"{ProductName}.exe");
            Directory.CreateDirectory(outputDir);

            BuildPlayerOptions options = new()
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
                targetGroup = BuildTargetGroup.Standalone
            };

            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[BuildPipeline] Release OK: {exePath}");
            else
                Debug.LogError($"[BuildPipeline] Release falló.");
        }

        private static string[] GetEnabledScenePaths()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            System.Collections.Generic.List<string> paths = new();
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.enabled)
                    paths.Add(scene.path);
            }

            return paths.ToArray();
        }

        private static void ConfigureBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/_Scenes/Scene_00_Boot.unity", true),
                new EditorBuildSettingsScene("Assets/_Scenes/Scene_01_MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/_Scenes/Scene_02_GameWorld.unity", true),
                new EditorBuildSettingsScene("Assets/_Scenes/Scene_03_UpgradeMenu.unity", true)
            };
        }
    }
}
#endif
