using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ScriptBatch : IPostprocessBuildWithReport
{
    public static string VersionNumber => $"v{Application.version}";

    public static string AppName => $"{Application.productName}";

    public int callbackOrder { get { return 0; } }

    public static string[] GetScenes()
    {
        return new string[]
        {
            "Assets/OpenKCC/Scenes/SampleScene.unity"
        };
    }

    public void OnPostprocessBuild(BuildReport report)
    {
    }

    [MenuItem("Build/Build All")]
    public static void BuildAll()
    {
        WebGLBuild();
        MacOSBuild();
        LinuxBuild();
        WindowsBuild();
    }

    [MenuItem("Build/WebGL Build")]
    public static void WebGLBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.template = "APPLICATION:NickTemplate";

        // Get filename.
        string[] levels = GetScenes();
        string appFolder = $"Builds/OpenKCC-WebGL-{VersionNumber}/OpenKCC";

        // Build player.
        BuildPipeline.BuildPlayer(levels, appFolder, BuildTarget.WebGL, BuildOptions.Development);
    }

    [MenuItem("Build/MacOS Build")]
    public static void MacOSBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

        // Get filename.
        string path = $"Builds/OpenKCC-MacOS-{VersionNumber}";
        string[] levels = GetScenes();

        string appFolder = path + $"/{AppName}.app";

        // Build player.
        BuildPipeline.BuildPlayer(levels, appFolder, BuildTarget.StandaloneOSX, BuildOptions.Development);
    }

    [MenuItem("Build/Linux Build")]
    public static void LinuxBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        // Get filename.
        string path = $"Builds/OpenKCC-Linux-{VersionNumber}";
        string[] levels = GetScenes();

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + $"/{AppName}.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.Development);
    }

    [MenuItem("Build/Windows64 Build")]
    public static void WindowsBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = $"Builds/OpenKCC-Win64-{VersionNumber}/{AppName}.exe",
            targetGroup = BuildTargetGroup.Standalone,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }

    [MenuItem("Build/Official WebGL Build")]
    public static void OfficialBuild_WebGL()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.template = "PROJECT:NickTemplate";
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = $"Builds/OpenKCC-WebGL",
            targetGroup = BuildTargetGroup.WebGL,
            target = BuildTarget.WebGL,
            options = BuildOptions.Development
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }

    public static void TestBuild_WebGL()
    {
        WebGLBuild();
    }

    public static void TestBuild_Win64()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = $"Builds/OpenKCC-Test-Win64-{VersionNumber}/{AppName}.exe",
            targetGroup = BuildTargetGroup.Standalone,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }
}
