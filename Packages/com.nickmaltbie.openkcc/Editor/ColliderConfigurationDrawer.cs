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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace nickmaltbie.OpenKCC.Editor
{
    [CustomPropertyDrawer(typeof(ColliderConfiguration))]
    public class ColliderConfigurationDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels\
            var typeProp = property.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProp, label);
            
            ColliderType col = (ColliderType) typeProp.enumValueIndex;
            EditorGUILayout.PropertyField(property.FindPropertyRelative("center"), new GUIContent("Center"));
            switch (col)
            {
                case ColliderType.Box:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("size"), new GUIContent("size"));
                    break;
                case ColliderType.Sphere:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"), new GUIContent("radius"));
                    break;
                case ColliderType.Capsule:
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("radius"), new GUIContent("radius"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("height"), new GUIContent("height"));
                    EditorGUILayout.PropertyField(property.FindPropertyRelative("capsuleDirection"), new GUIContent("capsuleDirection"));
                    break;
            }

            EditorGUI.EndProperty();
        }
    }
}
