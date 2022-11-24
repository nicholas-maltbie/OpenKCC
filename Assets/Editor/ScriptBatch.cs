// Copyright (C) 2022 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Batched scripts for building the project more easily.
/// </summary>
public class ScriptBatch : IPostprocessBuildWithReport, IPreprocessBuildWithReport
{
    /// <summary>
    /// Directory to put builds for the project relative to project root.
    /// </summary>
    public const string BuildDirectory = "Builds";

    /// <summary>
    /// Directory of the assets folder relative to project root.
    /// </summary>
    public const string AssetDirectory = "Assets";

    /// <summary>
    /// Descriptive string of the application version.
    /// </summary>
    public static string VersionNumber => $"v{Application.version}";

    /// <summary>
    /// Name of the application"
    /// </summary>
    public static string AppName => $"{Application.productName}";

    /// <summary>
    /// Callback order for resolving this script during build.
    /// </summary>
    public int callbackOrder => 0;

    /// <summary>
    /// Gets the list of scenes to use in the build.
    /// </summary>
    public static string[] GameScenes => new[]
    {
        System.IO.Path.Combine(ScriptBatch.AssetDirectory, "Scenes", "SampleScene.unity")
    };

    /// <summary>
    /// Gets the list of scenes to use in the netcode build.
    /// </summary>
    public static string[] NetcodeGameScenes => new[]
    {
        System.IO.Path.Combine(ScriptBatch.AssetDirectory, "Samples", "NetcodeExample", "ExampleScene.unity")
    };

    /// <summary>
    /// Called before build is completed.
    /// </summary>
    /// <param name="report">Report of the build results and configuration.</param>
    public void OnPreprocessBuild(BuildReport report)
    {
    }

    /// <summary>
    /// Called upon build completion for any final steps.
    /// </summary>
    /// <param name="report">Report of the build results and configuration.</param>
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

    /// <summary>
    /// Build the WebGL, MacOS, and Windows versions of the project.
    /// </summary>
    [MenuItem("Build/Main/Demo/Build All")]
    public static void BuildAll()
    {
        WebGLBuild();
        MacOSBuild();
        LinuxBuild();
        WindowsBuild();
    }

    /// <summary>
    /// Build the WebGL, MacOS, and Windows versions of the project for the netcode example.
    /// </summary>
    [MenuItem("Build/Netcode/Demo/Build All")]
    public static void BuildAll_Netcode()
    {
        WebGLBuild_Netcode();
        MacOSBuild_Netcode();
        LinuxBuild_Netcode();
        WindowsBuild_Netcode();
    }

    /// <summary>
    /// Create a demo build for the WebGL platform.
    /// </summary>
    [MenuItem("Build/Netcode/Demo/WebGL Build")]
    public static void WebGLBuild_Netcode()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.WebGL.template = "PROJECT:Better2020";

        // Get file path of build.
        string appFolder = Path.Combine(
            BuildDirectory,
            $"{Constants.ProjectName}-WebGL-Netcode-{VersionNumber}",
            Constants.ProjectName);

        // Build player.
        BuildPipeline.BuildPlayer(NetcodeGameScenes, appFolder, BuildTarget.WebGL, BuildOptions.Development);
    }

    /// <summary>
    /// Create a build for the Mac platform with the Mono backend.
    /// </summary>
    [MenuItem("Build/Main/Demo/MacOS Build")]
    public static void MacOSBuild_Netcode()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

        // Get file path of build.
        string path = Path.Combine(BuildDirectory, $"{Constants.ProjectName}-MacOS-Netcode-{VersionNumber}");

        string appFolder = path + $"/{AppName}.app";

        // Build player.
        BuildPipeline.BuildPlayer(NetcodeGameScenes, appFolder, BuildTarget.StandaloneOSX, BuildOptions.Development);
    }

    /// <summary>
    /// Create a build for the Linux platform with the IL2CPP backend.
    /// </summary>
    [MenuItem("Build/Netcode/Demo/Linux Build")]
    public static void LinuxBuild_Netcode()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        // Get file path of build.
        string path = Path.Combine(BuildDirectory, $"{Constants.ProjectName}-Linux-Netcode-{VersionNumber}");

        // Build player.
        BuildPipeline.BuildPlayer(NetcodeGameScenes, path + $"/{AppName}.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.Development);
    }

    /// <summary>
    /// Create a build for the windows 64 version with IL2CPP backend.
    /// </summary>
    [MenuItem("Build/Netcode/Demo/Windows64 Build")]
    public static void WindowsBuild_Netcode()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        var options = new BuildPlayerOptions
        {
            scenes = NetcodeGameScenes,
            locationPathName = Path.Combine(
                BuildDirectory,
                $"{Constants.ProjectName}-Win64-Netcode-{VersionNumber}",
                $"{AppName}.exe"),
            targetGroup = BuildTargetGroup.Standalone,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }

    /// <summary>
    /// Create a demo build for the WebGL platform.
    /// </summary>
    [MenuItem("Build/Main/Demo/WebGL Build")]
    public static void WebGLBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.WebGL.template = "PROJECT:Better2020";

        // Get file path of build.
        string appFolder = Path.Combine(
            BuildDirectory,
            $"{Constants.ProjectName}-WebGL-{VersionNumber}",
            Constants.ProjectName);

        // Build player.
        BuildPipeline.BuildPlayer(GameScenes, appFolder, BuildTarget.WebGL, BuildOptions.Development);
    }

    /// <summary>
    /// Create a build for the Mac platform with the Mono backend.
    /// </summary>
    [MenuItem("Build/Main/Demo/MacOS Build")]
    public static void MacOSBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

        // Get file path of build.
        string path = Path.Combine(BuildDirectory, $"{Constants.ProjectName}-MacOS-{VersionNumber}");

        string appFolder = path + $"/{AppName}.app";

        // Build player.
        BuildPipeline.BuildPlayer(GameScenes, appFolder, BuildTarget.StandaloneOSX, BuildOptions.Development);
    }

    /// <summary>
    /// Create a build for the Linux platform with the IL2CPP backend.
    /// </summary>
    [MenuItem("Build/Main/Demo/Linux Build")]
    public static void LinuxBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        // Get file path of build.
        string path = Path.Combine(BuildDirectory, $"{Constants.ProjectName}-Linux-{VersionNumber}");

        // Build player.
        BuildPipeline.BuildPlayer(GameScenes, path + $"/{AppName}.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.Development);
    }

    /// <summary>
    /// Create a build for the windows 64 version with IL2CPP backend.
    /// </summary>
    [MenuItem("Build/Main/Demo/Windows64 Build")]
    public static void WindowsBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        var options = new BuildPlayerOptions
        {
            scenes = GameScenes,
            locationPathName = Path.Combine(
                BuildDirectory,
                $"{Constants.ProjectName}-Win64-{VersionNumber}",
                $"{AppName}.exe"),
            targetGroup = BuildTargetGroup.Standalone,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }

    /// <summary>
    /// Create all official builds.
    /// </summary>
    [MenuItem("Build/Netcode/Official/Build All")]
    public static void OfficialBuild_All()
    {
        OfficialBuild_WebGL();
        OfficialBuild_WebGL_Netcode();
    }

    /// <summary>
    /// Create an official build for the WebGL Platform.
    /// </summary>
    [MenuItem("Build/Main/Official/WebGL Build")]
    public static void OfficialBuild_WebGL()
    {
        PlayerSettings.WebGL.template = "PROJECT:Better2020";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        var options = new BuildPlayerOptions
        {
            scenes = GameScenes,
            locationPathName = Path.Combine(BuildDirectory, $"{Constants.ProjectName}-WebGL"),
            target = BuildTarget.WebGL,
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }

    /// <summary>
    /// Create an official build for the WebGL Platform with the netcode scenes.
    /// </summary>
    [MenuItem("Build/Netcode/Official/WebGL Build Netcode")]
    public static void OfficialBuild_WebGL_Netcode()
    {
        PlayerSettings.WebGL.template = "PROJECT:Better2020";
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        var options = new BuildPlayerOptions
        {
            scenes = NetcodeGameScenes,
            locationPathName = Path.Combine(BuildDirectory, $"{Constants.ProjectName}-WebGL-Netcode"),
            target = BuildTarget.WebGL,
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }

    /// <summary>
    /// Create a test build for the WebGL platform.
    /// </summary>
    public static void TestBuild_WebGL()
    {
        WebGLBuild();
    }

    /// <summary>
    /// Create a test build for Windows 64 platform with Mono backend.
    /// </summary>
    public static void TestBuild_Win64()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

        var options = new BuildPlayerOptions
        {
            scenes = GameScenes,
            locationPathName = Path.Combine(
                BuildDirectory,
                $"{Constants.ProjectName}-Test-Win64-{VersionNumber}",
                $"{AppName}.exe"),
            targetGroup = BuildTargetGroup.Standalone,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        };

        // Build player.
        BuildPipeline.BuildPlayer(options);
    }
}
