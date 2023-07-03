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

using nickmaltbie.OpenKCC.Utils.ColliderCast;
using UnityEditor;

namespace nickmaltbie.OpenKCC.Editor
{
    [CustomEditor(typeof(PrimitiveColliderCast), true)]
    public class PrimitiveColliderCastEditor : UnityEditor.Editor
    {
        private SerializedProperty colliderConfig;

        public void OnEnable()
        {
            colliderConfig = serializedObject.FindProperty("config");
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            EditorGUILayout.PropertyField(colliderConfig);

            // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
            if (serializedObject.ApplyModifiedProperties())
            {
                (target as PrimitiveColliderCast).UpdateColliderParameters();
            }
        }
    }
}
