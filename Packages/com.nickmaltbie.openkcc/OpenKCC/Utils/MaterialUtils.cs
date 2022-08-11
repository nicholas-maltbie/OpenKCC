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

using UnityEngine;
using UnityEngine.Rendering;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Collection of utility functions for setting material properties.
    /// </summary>
    public static class MaterialUtils
    {
        /// <summary>
        /// Recursively set the ShadowCastingMode for each
        /// Renderer component in the object and its children. 
        /// </summary>
        /// <param name="original">Base game object to start operation from.</param>
        /// <param name="shadowCastingMode">Shadow casting mode to set for Renderer component.</param>
        public static void RecursiveSetShadowCastingMode(GameObject original, ShadowCastingMode shadowCastingMode)
        {
            foreach (Renderer renderer in original.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = shadowCastingMode;
            }
        }

        /// <summary>
        /// Recursively set a float property for each Renderer components'
        /// materials for a given object and its children.
        /// </summary>
        /// <param name="original">Base game object to start operation from.</param>
        /// <param name="property">Name of property to modify.</param>
        /// <param name="value">Value to set for float property.</param>
        /// <param name="sharedMaterial">Should the shared materials be modified.</param>
        public static void RecursiveSetFloatProperty(GameObject original, string property, float value, bool sharedMaterial = false)
        {
            foreach (Renderer renderer in original.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in sharedMaterial ? renderer.sharedMaterials : renderer.materials)
                {
                    mat.SetFloat(property, value);
                }
            }
        }

        /// <summary>
        /// Recursively set a Color property for each
        /// Renderer components' materials for a given object and its children.
        /// </summary>
        /// <param name="original">Base game object to start operation from.</param>
        /// <param name="property">Name of property to modify.</param>
        /// <param name="value">Value to set for color property.</param>
        /// <param name="sharedMaterial">Should the shared materials be modified.</param>
        public static void RecursiveSetColorProperty(GameObject original, string property, Color value, bool sharedMaterial = false)
        {
            foreach (Renderer renderer in original.GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in sharedMaterial ? renderer.sharedMaterials : renderer.materials)
                {
                    mat.SetColor(property, value);
                }
            }
        }
    }
}
