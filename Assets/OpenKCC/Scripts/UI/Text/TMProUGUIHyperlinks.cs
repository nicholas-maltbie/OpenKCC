using System.Collections;
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
    /// </summary>
    [DisallowMultipleComponent()]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMProUGUIHyperlinks : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private Color32 hoveredColor = new Color32(0x00, 0x59, 0xFF, 0xFF);
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
        private Camera mainCamera;

        void Awake()
        {
            textMeshPro = GetComponent<TextMeshProUGUI>();

            mainCamera = Camera.main;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            int linkIndex = GetLinkIndex();
            if (linkIndex != -1) // Was pointer intersecting a link?
            {
                pressedLinkIndex = linkIndex;
                if (usedLinks.TryGetValue(linkIndex, out bool isUsed) && isUsed) // Has the link been already used?
                {
                    // Have we hovered before we pressed? Touch input will first press, then hover
                    if (pressedLinkIndex != hoveredLinkIndex) startColors = SetLinkColor(linkIndex, usedPressedColor);
                    else SetLinkColor(linkIndex, usedPressedColor);
                }
                else
                {
                    // Have we hovered before we pressed? Touch input will first press, then hover
                    if (pressedLinkIndex != hoveredLinkIndex) startColors = SetLinkColor(linkIndex, pressedColor);
                    else SetLinkColor(linkIndex, pressedColor);
                }
                hoveredLinkIndex = pressedLinkIndex; // Changes flow in LateUpdate
            }
            else pressedLinkIndex = -1;
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

        private void LateUpdate()
        {
            int linkIndex = GetLinkIndex();
            if (linkIndex != -1) // Was pointer intersecting a link?
            {
                if (linkIndex != hoveredLinkIndex) // We started hovering above link (hover can be set from OnPointerDown!)
                {
                    if (hoveredLinkIndex != -1) ResetLinkColor(hoveredLinkIndex, startColors); // If we hovered above other link before
                    hoveredLinkIndex = linkIndex;
                    if (usedLinks.TryGetValue(linkIndex, out bool isUsed) && isUsed) // Has the link been already used?
                    {
                        // If we have pressed on link, wandered away and came back, set the pressed color
                        if (pressedLinkIndex == linkIndex) startColors = SetLinkColor(hoveredLinkIndex, usedPressedColor);
                        else startColors = SetLinkColor(hoveredLinkIndex, usedHoveredColor);
                    }
                    else
                    {
                        // If we have pressed on link, wandered away and came back, set the pressed color
                        if (pressedLinkIndex == linkIndex) startColors = SetLinkColor(hoveredLinkIndex, pressedColor);
                        else startColors = SetLinkColor(hoveredLinkIndex, hoveredColor);
                    }
                }
            }
            else if (hoveredLinkIndex != -1) // If we hovered above other link before
            {
                ResetLinkColor(hoveredLinkIndex, startColors);
                hoveredLinkIndex = -1;
            }
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
                var charInfo = textMeshPro.textInfo.characterInfo[characterIndex];
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

        private void ResetLinkColor(int linkIndex, List<Color32[]> startColors)
        {
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            int underlineIndex = -1;
            for (int i = 0; i < linkInfo.linkTextLength; i++)
            {
                int characterIndex = linkInfo.linkTextfirstCharacterIndex + i;
                var charInfo = textMeshPro.textInfo.characterInfo[characterIndex];
                int meshIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Color32[] vertexColors = textMeshPro.textInfo.meshInfo[meshIndex].colors32;
                if (charInfo.isVisible)
                {
                    vertexColors[vertexIndex + 0] = startColors[i][0];
                    vertexColors[vertexIndex + 1] = startColors[i][1];
                    vertexColors[vertexIndex + 2] = startColors[i][2];
                    vertexColors[vertexIndex + 3] = startColors[i][3];
                }
                if (charInfo.isVisible && charInfo.underlineVertexIndex > 0 && charInfo.underlineVertexIndex != underlineIndex && charInfo.underlineVertexIndex < vertexColors.Length)
                {
                    underlineIndex = charInfo.underlineVertexIndex;
                    for (int j = 0; j < 12; j++)
                    {
                        vertexColors[underlineIndex + j] = startColors[i][0];
                    }
                }
            }

            textMeshPro.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }
}
