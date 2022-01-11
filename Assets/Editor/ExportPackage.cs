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
using UnityEngine;

public class ExportPackage : MonoBehaviour
{
    public static readonly string[] AssetPaths = {
        Path.Combine(ScriptBatch.AssetDirectory, "OpenKCC"),
    };

    public static readonly string[] ScriptsAssetPaths = {
        Path.Combine(ScriptBatch.AssetDirectory, "OpenKCC", "Scripts"),
    };

    public static string PackagePath => Path.Combine(
        ScriptBatch.BuildDirectory, $"OpenKCC-Examples-{ScriptBatch.VersionNumber}.unitypackage");

    public static string ScriptPackagePath => Path.Combine(
        ScriptBatch.BuildDirectory, $"OpenKCC-{ScriptBatch.VersionNumber}.unitypackage");

    [MenuItem("Build/Package/Export All Packages")]
    public static void ExportAllPackages()
    {
        ExportExampleAssetPackage();
        ExportScriptsAssetPackage();
    }

    [MenuItem("Build/Package/Export Example Package")]
    public static void ExportExampleAssetPackage()
    {
        AssetDatabase.ExportPackage(
            AssetPaths,
            PackagePath,
            ExportPackageOptions.Recurse |
                ExportPackageOptions.Interactive |
                ExportPackageOptions.IncludeDependencies);
    }

    [MenuItem("Build/Package/Export Scripts Package")]
    public static void ExportScriptsAssetPackage()
    {
        AssetDatabase.ExportPackage(
            ScriptsAssetPaths,
            ScriptPackagePath,
            ExportPackageOptions.Recurse |
                ExportPackageOptions.Interactive |
                ExportPackageOptions.IncludeDependencies);
    }
}
