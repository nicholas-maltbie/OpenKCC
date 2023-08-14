// Copyright (C) 2023 Nicholas Maltbie
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

using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
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
        // Ignore library assets
        if (assetPath.StartsWith("Packages"))
        {
            return;
        }

        Fix($"{assetPath}.meta");
    }

    private IEnumerator FixRoutine(string metafile)
    {
        if (!File.ReadAllText(metafile).Contains("platformSettings:"))
        {
            yield return null;
        }

        Fix(metafile);
    }

    private void Fix(string metafile)
    {
        if (!File.ReadAllText(metafile).Contains("platformSettings:"))
        {
            EditorCoroutineUtility.StartCoroutine(FixRoutine(metafile), this);
            return;
        }

        // Read .meta file
        string original = File.ReadAllText(metafile);
        StringBuilder meta = new(original);

        Debug.Log($"[FixTPS] Checking platform settings for metafile:{metafile}...");
        foreach (string platform in Platforms)
        {
            if (meta.ToString().Contains(platform))
            {
                // Add missing setting to existing platforms
                if (!meta.ToString().Contains(Settings[^1]))
                {
                    int insertAt = meta.ToString().LastIndexOf(Settings[^2]) + Settings[^2].Length + 1;
                    meta.Insert(insertAt, Settings[^1] + '\n');

                    Debug.Log($"[FixTPS] Added missing setting to platform '{platform}' for metafile:{metafile}");
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

                Debug.Log($"[FixTPS] Added missing platform '{platform}' for metafile:{metafile}");
            }
        }

        // Save .meta file
        if (!Regex.Replace(meta.ToString(), @"\s+", "").Equals(
            Regex.Replace(original, @"\s+", "")))
        {
            File.WriteAllText(metafile, meta.ToString());
        }
    }
}
