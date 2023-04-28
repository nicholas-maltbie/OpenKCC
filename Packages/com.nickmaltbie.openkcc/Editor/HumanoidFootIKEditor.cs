
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nickmaltbie.OpenKCC.Animation;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace nickmaltbie.OpenKCC.Editor
{
    [CustomEditor(typeof(HumanoidFootIK), true)]
    public class HumanoidFootIKEditor : UnityEditor.Editor
    {
        public const string LeftFootIKWeight = "LeftFootIKWeight";
        public const string RightFootIKWeight = "RightFootIKWeight";

        public float groundedHeight = 0.15f;
        int taskId = -1;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            groundedHeight = EditorGUILayout.FloatField("Grounded Height", groundedHeight);

            if (GUILayout.Button("Bake Animation Curves"))
            {
                var footIK = target as HumanoidFootIK;
                EditorCoroutineUtility.StartCoroutine(BakeAnimations(footIK.gameObject, footIK.GetComponent<Animator>()), this);
            }
        }

        public bool IsFootGrounded(GameObject go, Animator animator, HumanBodyBones bone)
        {
            Transform transform = animator.GetBoneTransform(bone);
            UnityEngine.Debug.Log($"localPos:{transform.position.ToString("F3")}");
            return transform.position.y <= 0.15f;
        }

        public IEnumerator BakeAnimations(GameObject go, Animator animator)
        {
            taskId = Progress.Start("FootIKCurves", "Baking Foot IK Curves", Progress.Options.None, -1);

            var clips = animator.runtimeAnimatorController.animationClips;
            int clipCount = clips.Length;
            int current = 0;

            // Setup the curves for foot locking to ground
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                float clipPercent = (float) current / clipCount;
                Progress.Report(taskId, clipPercent, $"Working on clip:{clip.name}");

                current++;
                var assetPath = AssetDatabase.GetAssetPath(clip);

                ClipAnimationInfoCurve infoCurve = new ClipAnimationInfoCurve();
                infoCurve.name = LeftFootIKWeight;
                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;

                List<Keyframe> keys = new List<Keyframe>();
                
                // Sample for each frame and check if foot is grounded
                int frames = Mathf.CeilToInt(clip.length * clip.frameRate);
                float time = 0.0f;
                yield return null;
                clip.SampleAnimation(go, time);
                bool grounded = IsFootGrounded(go, animator, HumanBodyBones.LeftFoot);
                keys.Add(new Keyframe(time, grounded ? 1.0f : 0.0f));

                for (int i = 1; i < frames; i++)
                {
                    time = i / clip.frameRate;
                    clip.SampleAnimation(go, time);
                    bool newGrounded = IsFootGrounded(go, animator, HumanBodyBones.LeftFoot);
                    UnityEngine.Debug.Log($"time:{time} newGrounded:{newGrounded} grounded:{grounded}");
                    if (newGrounded != grounded)
                    {
                        keys.Add(new Keyframe(time, grounded ? 1.0f : 0.0f));
                    }

                    Progress.Report(taskId, clipPercent + (float) i / frames / clipCount);

                    grounded = newGrounded;
                }

                keys.Add(new Keyframe(clip.length, grounded ? 1.0f : 0.0f));
                var curve = new AnimationCurve(keys.ToArray());
                infoCurve.curve = curve;
                ModelImporterClipAnimation[] anims = importer.clipAnimations;
                anims[0].curves = Enumerable.Append(
                    anims[0].curves.Where(c => c.name != LeftFootIKWeight),
                    infoCurve).ToArray();
                importer.clipAnimations = anims;

                yield return null;
                Progress.Report(taskId, (float) current / clipCount, $"Saving results for clip:{clip.name}");
                importer.SaveAndReimport();
                yield return null;
            }

            Progress.Remove(taskId);
        }
    }
}
