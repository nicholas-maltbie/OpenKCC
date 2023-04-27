
using System.Collections.Generic;
using nickmaltbie.OpenKCC.Animation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace nickmaltbie.OpenKCC.Editor
{
    [CustomEditor(typeof(HumanoidFootIK), true)]
    public class HumanoidFootIKEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Bake Animation Curves"))
            {
                var footIK = target as HumanoidFootIK;
                BakeAnimations(footIK.gameObject, footIK.GetComponent<Animator>());
            }
        }

        public void BakeAnimations(GameObject go, Animator animator)
        {
            // Setup the curves for foot locking to ground
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                var bindings = AnimationUtility.GetCurveBindings(clip);
                UnityEngine.Debug.Log($"clip:{clip.name} bindings:{bindings.Length}");

                foreach (var b in bindings)
                {
                    if (b.propertyName == "Test")
                    {
                        UnityEngine.Debug.Log($"path:'{b.path}' type:'{b.type}' propertyName:'{b.propertyName}'");
                    }
                }

                var assetPath = AssetDatabase.GetAssetPath(clip);
                List<Keyframe> keys = new List<Keyframe>();
                keys.Add(new Keyframe(0.0f, 0.0f));
                keys.Add(new Keyframe(clip.length, 1.0f));

                var curve = new AnimationCurve(keys.ToArray());
                EditorCurveBinding binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "leftFootIKWeight");
                AnimationUtility.SetEditorCurve(clip, binding, curve);
                AssetDatabase.SaveAssetIfDirty(clip);
            }
        }
    }
}