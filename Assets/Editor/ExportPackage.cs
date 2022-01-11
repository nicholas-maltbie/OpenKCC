using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Unity.VisualScripting.IonicZip;
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
