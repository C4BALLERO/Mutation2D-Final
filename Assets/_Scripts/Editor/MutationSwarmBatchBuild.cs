#if UNITY_EDITOR
namespace MutationSwarm.Editor
{
    /// <summary>
    /// Punto de entrada para Unity -batchmode -executeMethod
    /// </summary>
    public static class MutationSwarmBatchBuild
    {
        public static void RunFullBuild()
        {
            MutationSwarmBuildPipeline.BuildAllContent();
            MutationSwarmBuildPipeline.BuildWindows();
        }

        public static void RunContentOnly()
        {
            MutationSwarmGunsImport.ImportAll();
            MutationSwarmEnemyArtSetup.BuildAll();
            MutationSwarmPlayerArtSetup.BuildAll();
            MutationSwarmKenneyGameplaySetup.BuildAll();
        }

        public static void RunWindowsOnly()
        {
            if (!System.IO.File.Exists("Assets/_Scenes/Scene_00_Boot.unity"))
                MutationSwarmProjectSetup.SetupCompleteProject();
            else
                MutationSwarmBuildPipeline.ConfigureBuildScenesForBatch();

            MutationSwarmBuildPipeline.BuildWindowsPlayerOnly();
        }
    }
}
#endif
