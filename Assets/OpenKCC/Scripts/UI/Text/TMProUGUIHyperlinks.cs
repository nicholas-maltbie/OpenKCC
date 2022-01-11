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

using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.UI.Text
{
    /// <summary>
    /// This class handles basic link color behavior, supports also underline (static only)
    /// Does not support strike-through, but can be easily implemented in the same way as the underline
    /// 
    /// forum post reference - https://forum.unity.com/threads/textmeshpro-hyperlinks.1091296/
    /// 
    /// https://github.com/EpsilonD3lta/UnityUtilities/blob/master/Scripts/Runtime/TMProUGUIHyperlinks.cs
    /// </summary>
    [DisallowMultipleComponent()]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMProUGUIHyperlinks : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private Color32 pressedColor = new Color32(0x00, 0x00, 0xB7, 0xFF);
        [SerializeField]
        private Color32 usedColor = new Color32(0xFF, 0x00, 0xFF, 0xFF);
        [SerializeField]
        private Color32 usedHoveredColor = new Color32(0xFD, 0x5E, 0xFD, 0xFF);
        [SerializeField]
        private Color32 usedPressedColor = new Color32(0xCF, 0x00, 0xCF, 0xFF);

        private List<Color32[]> startColors = new List<Color32[]>();
        private TextMeshProUGUI textMeshPro;
        private Dictionary<int, bool> usedLinks = new Dictionary<int, bool>();
        private int hoveredLinkIndex = -1;
        private int pressedLinkIndex = -1;

        public void OnPointerDown(PointerEventData eventData)
        {
            int linkIndex = GetLinkIndex();
            if (linkIndex != -1) // Was pointer intersecting a link?
            {
                pressedLinkIndex = linkIndex;
                if (usedLinks.TryGetValue(linkIndex, out bool isUsed) && isUsed) // Has the link been already used?
                {
                    // Have we hovered before we pressed? Touch input will first press, then hover
                    if (pressedLinkIndex != hoveredLinkIndex)
                    {
                        startColors = SetLinkColor(linkIndex, usedPressedColor);
                    }
                    else
                    {
                        SetLinkColor(linkIndex, usedPressedColor);
                    }
                }
                else
                {
                    // Have we hovered before we pressed? Touch input will first press, then hover
                    if (pressedLinkIndex != hoveredLinkIndex)
                    {
                        startColors = SetLinkColor(linkIndex, pressedColor);
                    }
                    else
                    {
                        SetLinkColor(linkIndex, pressedColor);
                    }
                }

                hoveredLinkIndex = pressedLinkIndex; // Changes flow in LateUpdate
            }
            else
            {
                pressedLinkIndex = -1;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            int linkIndex = GetLinkIndex();
            if (linkIndex != -1 && linkIndex == pressedLinkIndex) // Was pointer intersecting the same link as OnPointerDown?
            {
                TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
                SetLinkColor(linkIndex, usedHoveredColor);
                startColors.ForEach(c => c[0] = c[1] = c[2] = c[3] = usedColor);
                usedLinks[linkIndex] = true;
                UnityEngine.Debug.Log($"Opening link - {linkInfo.GetLinkID()}");
                Application.OpenURL(linkInfo.GetLinkID());
            }

            pressedLinkIndex = -1;
        }

        private int GetLinkIndex()
        {
            return TMP_TextUtilities.FindIntersectingLink(textMeshPro, Mouse.current.position.ReadValue(), null);
        }

        private List<Color32[]> SetLinkColor(int linkIndex, Color32 color)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];

            var oldVertexColors = new List<Color32[]>(); // Store the old character colors
            int underlineIndex = -1;
            for (int i = 0; i < linkInfo.linkTextLength; i++)
            {
                // For each character in the link string
                int characterIndex = linkInfo.linkTextfirstCharacterIndex + i; // The current character index
                TMP_CharacterInfo charInfo = textMeshPro.textInfo.characterInfo[characterIndex];
                int meshIndex = charInfo.materialReferenceIndex; // Get the index of the material/subtext object used by this character.
                int vertexIndex = charInfo.vertexIndex; // Get the index of the first vertex of this character.

                // This array contains colors for all vertices of the mesh (might be multiple chars)
                Color32[] vertexColors = textMeshPro.textInfo.meshInfo[meshIndex].colors32;
                oldVertexColors.Add(new Color32[] { vertexColors[vertexIndex + 0], vertexColors[vertexIndex + 1], vertexColors[vertexIndex + 2], vertexColors[vertexIndex + 3] });
                if (charInfo.isVisible)
                {
                    vertexColors[vertexIndex + 0] = color;
                    vertexColors[vertexIndex + 1] = color;
                    vertexColors[vertexIndex + 2] = color;
                    vertexColors[vertexIndex + 3] = color;
                }
                // Each line will have its own underline mesh with different index, index == 0 means there is no underline
                if (charInfo.isVisible && charInfo.underlineVertexIndex > 0 && charInfo.underlineVertexIndex != underlineIndex && charInfo.underlineVertexIndex < vertexColors.Length)
                {
                    underlineIndex = charInfo.underlineVertexIndex;
                    for (int j = 0; j < 12; j++) // Underline seems to be always 3 quads == 12 vertices
                    {
                        vertexColors[underlineIndex + j] = color;
                    }
                }
            }

            textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            return oldVertexColors;
        }

        // private void ResetLinkColor(int linkIndex, List<Color32[]> startColors)
        // {
        //     var linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
        //     var underlineIndex = -1;
        //     for (var i = 0; i < linkInfo.linkTextLength; i++)
        //     {
        //         var characterIndex = linkInfo.linkTextfirstCharacterIndex + i;
        //         var charInfo = textMeshPro.textInfo.characterInfo[characterIndex];
        //         var meshIndex = charInfo.materialReferenceIndex;
        //         var vertexIndex = charInfo.vertexIndex;

        //         var vertexColors = textMeshPro.textInfo.meshInfo[meshIndex].colors32;
        //         if (charInfo.isVisible)
        //         {
        //             vertexColors[vertexIndex + 0] = startColors[i][0];
        //             vertexColors[vertexIndex + 1] = startColors[i][1];
        //             vertexColors[vertexIndex + 2] = startColors[i][2];
        //             vertexColors[vertexIndex + 3] = startColors[i][3];
        //         }

        //         if (charInfo.isVisible && charInfo.underlineVertexIndex > 0 && charInfo.underlineVertexIndex != underlineIndex && charInfo.underlineVertexIndex < vertexColors.Length)
        //         {
        //             underlineIndex = charInfo.underlineVertexIndex;
        //             for (var j = 0; j < 12; j++)
        //             {
        //                 vertexColors[underlineIndex + j] = startColors[i][0];
        //             }
        //         }
        //     }

        //     textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        // }
    }
}
