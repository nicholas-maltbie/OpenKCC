using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ScriptBatch : IPostprocessBuildWithReport, IPreprocessBuildWithReport
{
    public const string BuildDirectory = "Builds";

    public const string AssetDirectory = "Assets";

    public static string VersionNumber => $"v{Application.version}";

    public static string AppName => $"{Application.productName}";

    public int callbackOrder { get { return 0; } }

    public static string[] GetScenes()
    {
        return new string[]
        {
            System.IO.Path.Combine(AssetDirectory, "OpenKCC/Scenes/SampleScene.unity")
        };
    }

    public void OnPreprocessBuild(BuildReport report)
    {
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
        {
            // Copy the web.config to the output path
            File.Copy(
                System.IO.Path.Combine(Application.dataPath, "Config", "web.config"),
                System.IO.Path.Combine(report.summary.outputPath, "Build", "web.config"));
        }

        // Restore default settings
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
    }

    [MenuItem("Build/Demo/Build All")]
    public static void BuildAll()
    {
        WebGLBuild();
        MacOSBuild();
        LinuxBuild();
        WindowsBuild();
    }

    [MenuItem("Build/Demo/WebGL Build")]
    public static void WebGLBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.WebGL.template = "PROJECT:Better2020";

        // Get filename.
        string[] levels = GetScenes();
        string appFolder = Path.Combine(BuildDirectory, $"OpenKCC-WebGL-{VersionNumber}/OpenKCC");

        // Build player.
        BuildPipeline.BuildPlayer(levels, appFolder, BuildTarget.WebGL, BuildOptions.Development);
    }

    [MenuItem("Build/Demo/MacOS Build")]
    public static void MacOSBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

        // Get filename.
        string path = Path.Combine(BuildDirectory, $"OpenKCC-MacOS-{VersionNumber}");
        string[] levels = GetScenes();

        string appFolder = path + $"/{AppName}.app";

        // Build player.
        BuildPipeline.BuildPlayer(levels, appFolder, BuildTarget.StandaloneOSX, BuildOptions.Development);
    }

    [MenuItem("Build/Demo/Linux Build")]
    public static void LinuxBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        // Get filename.
        string path = Path.Combine(BuildDirectory, $"OpenKCC-Linux-{VersionNumber}");
        string[] levels = GetScenes();

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + $"/{AppName}.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.Development);
    }

    [MenuItem("Build/Demo/Windows64 Build")]
    public static void WindowsBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = Path.Combine(BuildDirectory, $"OpenKCC-Win64-{VersionNumber}/{AppName}.exe"),
            targetGroup = BuildTargetGroup.Standalone,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }

    [MenuItem("Build/Demo/Official WebGL Build")]
    public static void OfficialBuild_WebGL()
    {
        PlayerSettings.WebGL.template = "PROJECT:Better2020";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = Path.Combine(BuildDirectory, $"OpenKCC-WebGL"),
            target = BuildTarget.WebGL,
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
            locationPathName = Path.Combine(BuildDirectory, $"OpenKCC-Test-Win64-{VersionNumber}/{AppName}.exe"),
            targetGroup = BuildTargetGroup.Standalone,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }
}
