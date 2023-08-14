using System;
using System.Collections;
using System.IO;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public class FixTexturePlatformSettings : AssetPostprocessor
{
    private static readonly string[] Settings =
    {
        "  - serializedVersion: 3",
        "    buildTarget: {platform}",
        "    maxTextureSize: 2048",
        "    resizeAlgorithm: 0",
        "    textureFormat: -1",
        "    textureCompression: 1",
        "    compressionQuality: 50",
        "    crunchedCompression: 0",
        "    allowsAlphaSplitting: 0",
        "    overridden: 0",
        "    androidETC2FallbackOverride: 0",
        "    forceMaximumCompressionQuality_BC6H_BC7: 0"
    };

    private static readonly string[] Platforms =
    {
        "DefaultTexturePlatform",
        "Standalone",
        "Server",
        "WebGL"
    };

    public void OnPostprocessTexture(Texture2D texture)
    {
        Fix($"{assetPath}.meta");
    }

    private void Fix(string metafile)
    {
        if (!File.ReadAllText(metafile).Contains("platformSettings:"))
        {
            return;
        }

        // Read .meta file
        string original = File.ReadAllText(metafile);
        StringBuilder meta = new(original);

        Debug.Log("[FixTPS] Checking platform settings...");
        foreach (string platform in Platforms)
        {
            if (meta.ToString().Contains(platform))
            {
                // Add missing setting to existing platforms
                if (!meta.ToString().Contains(Settings[^1]))
                {
                    int insertAt = meta.ToString().LastIndexOf(Settings[^2]) + Settings[^2].Length + 1;
                    meta.Insert(insertAt, Settings[^1] + '\n');

                    Debug.Log($"[FixTPS] Added missing setting to platform '{platform}'");
                }
            }
            else
            {
                // Add missing platforms
                string insertAfter = Settings[^1];

                if (meta.ToString().Contains("platformSettings: []"))
                {
                    meta.Replace("platformSettings: []", "platformSettings:");
                    insertAfter = "platformSettings:";

                    Debug.Log($"[FixTPS] Created platform settings array");
                }

                int insertAt = meta.ToString().LastIndexOf(insertAfter) + insertAfter.Length + 1;
                meta.Insert(insertAt, string.Join('\n', Settings).Replace("{platform}", platform) + '\n');

                Debug.Log($"[FixTPS] Added missing platform '{platform}'");
            }
        }

        // Save .meta file
        if (meta.ToString() != original)
        {
            File.WriteAllText(metafile, meta.ToString());
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
