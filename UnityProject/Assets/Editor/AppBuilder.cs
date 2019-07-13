using UnityEditor;

public class AppBuilder
{
    public static void Build()
    {
        string[] scenes = { "Assets/Scenes/MainScene.unity" };

        var args = System.Environment.GetCommandLineArgs();
        BuildPipeline.BuildPlayer(scenes, "../bin/soundvision.exe" , BuildTarget.StandaloneWindows64, BuildOptions.None);
    }
}
