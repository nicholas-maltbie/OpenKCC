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
using nickmaltbie.OpenKCC.Character.Config;
using UnityEditor;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Editor
{
    [CustomEditor(typeof(KCCMovementEngine))]
    public class KCCMovementEngineEditor : UnityEditor.Editor
    {
        private bool state = false;

        SerializedProperty stepHeightProp;
        SerializedProperty maxWalkAngleProp;

        void OnEnable()
        {
            // Fetch the objects from the GameObject script to display in the inspector
            stepHeightProp = serializedObject.FindProperty("stepHeight");
            maxWalkAngleProp = serializedObject.FindProperty("maxWalkAngle");
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            
            var engine = target as KCCMovementEngine;

            EditorGUILayout.PropertyField(stepHeightProp);
            EditorGUILayout.PropertyField(maxWalkAngleProp);

            // Display grounded state data
            KCCGroundedState grounded = engine.GroundedState;
            state = EditorGUILayout.BeginFoldoutHeaderGroup(state, "Grounded State");
            GUI.enabled = false;

            if (state)
            {
                EditorGUILayout.FloatField("Distance to Ground", grounded.DistanceToGround);
                EditorGUILayout.Toggle("On Ground", grounded.OnGround);
                EditorGUILayout.FloatField("Angle", grounded.Angle);
                EditorGUILayout.Vector3Field("Surface Normal", grounded.SurfaceNormal);
                EditorGUILayout.Vector3Field("Ground Hit Pos", grounded.GroundHitPosition);
                EditorGUILayout.ObjectField("Floor", grounded.Floor, typeof(Object), true);
                EditorGUILayout.Toggle("Standing on Ground", grounded.StandingOnGround);
                EditorGUILayout.Toggle("Standing on Ground or Overlap", grounded.StandingOnGroundOrOverlap);
                EditorGUILayout.Toggle("Falling", grounded.Falling);
                EditorGUILayout.Toggle("Sliding", grounded.Sliding);

                EditorGUILayout.Vector3Field("Ground Velocity", engine.GetGroundVelocity());
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            GUI.enabled = true;

            // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }

    }
}
