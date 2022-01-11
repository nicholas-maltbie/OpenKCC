
using UnityEngine;
using UnityEngine.Rendering;

namespace nickmaltbie.OpenKCC.Utils
{
    public static class MaterialUtils
    {
        public static void RecursiveSetShadowCasingMode(GameObject original, ShadowCastingMode shadowCastingMode)
        {
            foreach (Renderer renderer in original.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = shadowCastingMode;
            }
        }

        public static void RecursiveSetFloatProperty(GameObject original, string property, float value)
        {
            foreach (Renderer renderer in original.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    mat.SetFloat(property, value);
                }
            }
        }

        public static void RecursiveSetColorProperty(GameObject original, string property, Color value)
        {
            foreach (Renderer renderer in original.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in renderer.materials)
                {
                    mat.SetColor(property, value);
                }
            }
        }
    }
}
