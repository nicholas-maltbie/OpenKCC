
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

        int taskId = -1;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Bake Animation Curves"))
            {
                var footIK = target as HumanoidFootIK;
                EditorCoroutineUtility.StartCoroutine(BakeAnimations(footIK.gameObject, footIK.GetComponent<Animator>(), footIK), this);
            }
        }

        public bool IsFootGrounded(GameObject go, Animator animator, HumanBodyBones bone, float footGroundedHeight)
        {
            Transform transform = animator.GetBoneTransform(bone);
            return transform.position.y <= footGroundedHeight;
        }

        public IEnumerator BakeAnimations(GameObject go, Animator animator, HumanoidFootIK footIK)
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

                ClipAnimationInfoCurve leftInfoCurve = new ClipAnimationInfoCurve();
                ClipAnimationInfoCurve rightInfoCurve = new ClipAnimationInfoCurve();
                leftInfoCurve.name = LeftFootIKWeight;
                rightInfoCurve.name = RightFootIKWeight;

                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                
                // Sample for each frame and check if foot is grounded
                int frames = Mathf.CeilToInt(clip.length * clip.frameRate);

                Keyframe[] leftFootKeys = new Keyframe[frames];
                Keyframe[] rightFootKeys = new Keyframe[frames];

                yield return null;
                float time = 0.0f;
                clip.SampleAnimation(go, time);
                bool leftGrounded = IsFootGrounded(go, animator, HumanBodyBones.LeftFoot, footIK.footGroundedHeight);
                bool rightGrounded = IsFootGrounded(go, animator, HumanBodyBones.RightFoot, footIK.footGroundedHeight);
                leftFootKeys[0] = new Keyframe(time, leftGrounded ? 1.0f : 0.0f);
                rightFootKeys[0] = new Keyframe(time, rightGrounded ? 1.0f : 0.0f);
                yield return null;

                for (int i = 1; i < frames - 1; i++)
                {
                    time = i / clip.frameRate;
                    clip.SampleAnimation(go, time);
                    leftGrounded = IsFootGrounded(go, animator, HumanBodyBones.LeftFoot, footIK.footGroundedHeight);
                    rightGrounded = IsFootGrounded(go, animator, HumanBodyBones.RightFoot, footIK.footGroundedHeight);
                    leftFootKeys[i] = new Keyframe(time, leftGrounded ? 1.0f : 0.0f);
                    rightFootKeys[i] = new Keyframe(time, rightGrounded ? 1.0f : 0.0f);
                    yield return null;
                }

                time = clip.length;
                clip.SampleAnimation(go, time);
                leftGrounded = IsFootGrounded(go, animator, HumanBodyBones.LeftFoot, footIK.footGroundedHeight);
                rightGrounded = IsFootGrounded(go, animator, HumanBodyBones.RightFoot, footIK.footGroundedHeight);

                leftFootKeys[frames - 1] = new Keyframe(clip.length, leftGrounded ? 1.0f : 0.0f);
                rightFootKeys[frames - 1] = new Keyframe(clip.length, rightGrounded ? 1.0f : 0.0f);
                yield return null;

                leftFootKeys = Enumerable.Range(0, leftFootKeys.Length).Where(
                    i =>
                    {
                        int before = i - 1;
                        int after = i + 1;
                        bool sameBefore = before >= 0 && leftFootKeys[before].value == leftFootKeys[i].value;
                        bool sameAfter = after < frames && leftFootKeys[after].value == leftFootKeys[i].value;
                        return sameBefore && sameAfter;
                    }
                ).Select(i => leftFootKeys[i]).ToArray();
                rightFootKeys = Enumerable.Range(0, rightFootKeys.Length).Where(
                    i =>
                    {
                        int before = i - 1;
                        int after = i + 1;
                        bool sameBefore = before >= 0 && rightFootKeys[before].value == rightFootKeys[i].value;
                        bool sameAfter = after < frames && rightFootKeys[after].value == rightFootKeys[i].value;
                        return sameBefore && sameAfter;
                    }
                ).Select(i => rightFootKeys[i]).ToArray();

                leftInfoCurve.curve = new AnimationCurve(leftFootKeys);
                rightInfoCurve.curve = new AnimationCurve(rightFootKeys);

                ModelImporterClipAnimation[] anims = importer.clipAnimations;
                anims[0].curves = Enumerable.Concat(
                    anims[0].curves.Where(c => c.name != LeftFootIKWeight && c.name != RightFootIKWeight),
                    new [] { leftInfoCurve, rightInfoCurve }).ToArray();
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
