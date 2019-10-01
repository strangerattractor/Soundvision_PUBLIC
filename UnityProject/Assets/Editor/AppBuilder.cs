using UnityEditor;

public class AppBuilder
{
    public static void Build()
    {
        string[] scenes = { "Assets/Scenes/examples/PdBackendDemo/PdBackendDemo.unity" };

        var args = System.Environment.GetCommandLineArgs();
        BuildPipeline.BuildPlayer(scenes, "../bin/soundvision.exe" , BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
