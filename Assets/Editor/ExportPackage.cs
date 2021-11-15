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

    public static string PackagePath => Path.Combine(
        ScriptBatch.BuildDirectory, $"OpenKCC-{ScriptBatch.VersionNumber}.unitypackage");

    [MenuItem("Build/Package/Export Package")]
    public static void ExportAssetPackage()
    {
        AssetDatabase.ExportPackage(
            AssetPaths,
            PackagePath,
            ExportPackageOptions.Recurse |
                ExportPackageOptions.Interactive |
                ExportPackageOptions.IncludeDependencies);
        using (var zipFile = new ZipFile(PackagePath + ".zip"))
        {
            zipFile.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
            zipFile.AddFile(PackagePath, "");
            zipFile.Save();
        }
    }

}
