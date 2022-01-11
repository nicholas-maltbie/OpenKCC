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

using System;
using System.IO;
using System.Reflection;
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

    public int callbackOrder => 0;

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
        var levels = GetScenes();
        var appFolder = Path.Combine(BuildDirectory, $"OpenKCC-WebGL-{VersionNumber}/OpenKCC");

        // Build player.
        BuildPipeline.BuildPlayer(levels, appFolder, BuildTarget.WebGL, BuildOptions.Development);
    }

    [MenuItem("Build/Demo/MacOS Build")]
    public static void MacOSBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);

        // Get filename.
        var path = Path.Combine(BuildDirectory, $"OpenKCC-MacOS-{VersionNumber}");
        var levels = GetScenes();

        var appFolder = path + $"/{AppName}.app";

        // Build player.
        BuildPipeline.BuildPlayer(levels, appFolder, BuildTarget.StandaloneOSX, BuildOptions.Development);
    }

    [MenuItem("Build/Demo/Linux Build")]
    public static void LinuxBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        // Get filename.
        var path = Path.Combine(BuildDirectory, $"OpenKCC-Linux-{VersionNumber}");
        var levels = GetScenes();

        // Build player.
        BuildPipeline.BuildPlayer(levels, path + $"/{AppName}.x86_64", BuildTarget.StandaloneLinux64, BuildOptions.Development);
    }

    [MenuItem("Build/Demo/Windows64 Build")]
    public static void WindowsBuild()
    {
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);

        var options = new BuildPlayerOptions
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
        var options = new BuildPlayerOptions
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

        var options = new BuildPlayerOptions
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

    public static void PrepareSonarFiles()
    {
        Debug.Log("### ScriptBatch:PrepareSonarFiles - Started...");
        // We actually ask Unity to create the CSPROJ and SLN files.
        bool success = EditorApplication.ExecuteMenuItem("Assets/Open C# Project");
        Debug.Log("### ScriptBatch:PrepareSonarFiles - " + (success ? "Done" : "FAILED") + ".");

        // Unsupported Version
        Debug.Log("### ScriptBatch:PrepareSonarFiles - Started V2...");
        System.Type T = System.Type.GetType("UnityEditor.SyncVS,UnityEditor");
        System.Reflection.MethodInfo SyncSolution = T.GetMethod("SyncSolution", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        SyncSolution.Invoke(null, null);
        Debug.Log("### ScriptBatch:PrepareSonarFiles - Ended V2...");
        // ---
    }
}
