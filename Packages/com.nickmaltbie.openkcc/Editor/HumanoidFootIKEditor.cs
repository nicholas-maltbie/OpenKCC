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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nickmaltbie.OpenKCC.Animation;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Editor
{
    /// <summary>
    /// Custom editor script to manage creating curves for the Humanoid Foot IK.
    /// </summary>
    [CustomEditor(typeof(HumanoidFootIK), true)]
    public class HumanoidFootIKEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Task id for generating foot curves in teh background.
        /// </summary>
        private int taskId = -1;

        /// <summary>
        /// Rate of sampling per second.
        /// </summary>
        private int samplingRate = 30;

        /// <summary>
        /// Current state of the editor dropdown.
        /// </summary>
        private bool state = false;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (state = EditorGUILayout.BeginFoldoutHeaderGroup(state, "Baking Options"))
            {
                samplingRate = EditorGUILayout.IntField("Sampling Rate", samplingRate);
                var footIK = target as HumanoidFootIK;

                if (GUILayout.Button("Bake Animation Curves"))
                {
                    EditorCoroutineUtility.StartCoroutine(BakeAnimations(footIK.gameObject, footIK.GetComponent<Animator>(), footIK), this);
                }
            }
        }

        /// <summary>
        /// Check if the foot is "grounded" for a specific grounded. height.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="bone"></param>
        /// <param name="footGroundedHeight"></param>
        /// <returns></returns>
        public bool IsFootGrounded(Animator animator, HumanBodyBones bone, float footGroundedHeight)
        {
            Transform transform = animator.GetBoneTransform(bone);
            return transform.position.y <= footGroundedHeight;
        }

        public Dictionary<Transform, (Vector3, Quaternion)> GetPoses(Animator animator)
        {
            var state = new Dictionary<Transform, (Vector3, Quaternion)>();
            foreach (Transform transform in animator.GetComponentsInChildren<Transform>())
            {
                state[transform] = (transform.position, transform.rotation);
            }

            return state;
        }

        /// <summary>
        /// Reset the player pose to some saved position.
        /// </summary>
        /// <param name="poses">POsition and rotation of each bone.</param>
        public void ResetPlayerPose(Dictionary<Transform, (Vector3, Quaternion)> poses)
        {
            foreach (KeyValuePair<Transform, (Vector3, Quaternion)> kvp in poses)
            {
                kvp.Key.position = kvp.Value.Item1;
                kvp.Key.rotation = kvp.Value.Item2;
            }
        }

        /// <summary>
        /// Bake the animation for the avatar's foot IK curves.
        /// </summary>
        /// <param name="go">Game object for the avatar.</param>
        /// <param name="animator">Animator for the avatar.</param>
        /// <param name="footIK">Foot IK Controller.</param>
        /// <returns>Enumerable of action for baking action.</returns>
        public IEnumerator BakeAnimations(GameObject go, Animator animator, HumanoidFootIK footIK)
        {
            Dictionary<Transform, (Vector3, Quaternion)> pose = GetPoses(animator);

#if UNITY_2020_1_OR_NEWER
            taskId = Progress.Start("FootIKCurves", "Baking Foot IK Curves", Progress.Options.None, -1);
# endif

            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            int clipCount = clips.Length;
            int current = 0;

            // Setup the curves for foot locking to ground
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                float clipPercent = (float)current / clipCount;
# if UNITY_2020_1_OR_NEWER
                Progress.Report(taskId, clipPercent, $"Working on clip:{clip.name}");
# endif

                string assetPath = AssetDatabase.GetAssetPath(clip);

                var leftInfoCurve = new ClipAnimationInfoCurve();
                var rightInfoCurve = new ClipAnimationInfoCurve();
                leftInfoCurve.name = FootTarget.LeftFootIKWeight;
                rightInfoCurve.name = FootTarget.RightFootIKWeight;

                var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                ModelImporterClipAnimation[] anims = importer.clipAnimations;
                int animIndex = Enumerable.Range(0, anims.Length).FirstOrDefault(i => anims[i].name == clip.name);

                anims[animIndex].curves = anims[animIndex].curves.Where(c => c.name != FootTarget.LeftFootIKWeight && c.name != FootTarget.RightFootIKWeight).ToArray();
                importer.clipAnimations = anims;

                // So you actually need to cleanup the animations first...
                // otherwise it will fail to save.
                yield return null;
# if UNITY_2020_1_OR_NEWER
                Progress.Report(taskId, (float)current / clipCount, $"Cleaning up existing curves for:{clip.name}");
# endif
                importer.SaveAndReimport();
                ResetPlayerPose(pose);
                yield return null;

                // Sample for each frame and check if foot is grounded
                int frames = Mathf.CeilToInt(clip.length * samplingRate);

                var leftFootKeys = new Keyframe[frames];
                var rightFootKeys = new Keyframe[frames];

                yield return null;
                float time = 0.0f;
                clip.SampleAnimation(go, time);
                bool leftGrounded = IsFootGrounded(animator, HumanBodyBones.LeftFoot, footIK.footGroundedHeight);
                bool rightGrounded = IsFootGrounded(animator, HumanBodyBones.RightFoot, footIK.footGroundedHeight);
                leftFootKeys[0] = new Keyframe(time, leftGrounded ? 1.0f : 0.0f);
                rightFootKeys[0] = new Keyframe(time, rightGrounded ? 1.0f : 0.0f);
                yield return null;
# if UNITY_2020_1_OR_NEWER
                Progress.Report(taskId, (float)current / clipCount, $"Processing frame:0 for clip:{clip.name}.");
# endif

                for (int i = 1; i < frames - 1; i++)
                {
                    time = (float)i / samplingRate;
                    clip.SampleAnimation(go, time);
                    leftGrounded = IsFootGrounded(animator, HumanBodyBones.LeftFoot, footIK.footGroundedHeight);
                    rightGrounded = IsFootGrounded(animator, HumanBodyBones.RightFoot, footIK.footGroundedHeight);
                    float keyframeTime = (float)i / frames;
                    leftFootKeys[i] = new Keyframe(keyframeTime, leftGrounded ? 1.0f : 0.0f);
                    rightFootKeys[i] = new Keyframe(keyframeTime, rightGrounded ? 1.0f : 0.0f);
# if UNITY_2020_1_OR_NEWER
                    Progress.Report(taskId, (float)current / clipCount + (float)i / frames * 1 / clipCount, $"Processing frame:{i} for clip:{clip.name}.");
# endif
                    yield return null;
                }

                time = clip.length;
                clip.SampleAnimation(go, time);
                leftGrounded = IsFootGrounded(animator, HumanBodyBones.LeftFoot, footIK.footGroundedHeight);
                rightGrounded = IsFootGrounded(animator, HumanBodyBones.RightFoot, footIK.footGroundedHeight);
                current++;
# if UNITY_2020_1_OR_NEWER
                Progress.Report(taskId, (float)current / clipCount, $"Processing frame:end for clip:{clip.name}.");
# endif

                leftFootKeys[frames - 1] = new Keyframe(1.0f, leftGrounded ? 1.0f : 0.0f);
                rightFootKeys[frames - 1] = new Keyframe(1.0f, rightGrounded ? 1.0f : 0.0f);
                yield return null;

                leftFootKeys = Enumerable.Range(0, leftFootKeys.Length).Where(
                    i =>
                    {
                        int before = i - 1;
                        int after = i + 1;
                        bool sameBefore = before >= 0 && leftFootKeys[before].value == leftFootKeys[i].value;
                        bool sameAfter = after < frames && leftFootKeys[after].value == leftFootKeys[i].value;
                        return !sameBefore || !sameAfter;
                    }
                ).Select(i => leftFootKeys[i]).ToArray();
                rightFootKeys = Enumerable.Range(0, rightFootKeys.Length).Where(
                    i =>
                    {
                        int before = i - 1;
                        int after = i + 1;
                        bool sameBefore = before >= 0 && rightFootKeys[before].value == rightFootKeys[i].value;
                        bool sameAfter = after < frames && rightFootKeys[after].value == rightFootKeys[i].value;
                        return !sameBefore || !sameAfter;
                    }
                ).Select(i => rightFootKeys[i]).ToArray();

                leftInfoCurve.curve = new AnimationCurve(leftFootKeys);
                rightInfoCurve.curve = new AnimationCurve(rightFootKeys);

                anims[animIndex].curves = Enumerable.Concat(
                    anims[animIndex].curves.Where(c => c.name != FootTarget.LeftFootIKWeight && c.name != FootTarget.RightFootIKWeight),
                    new[] { leftInfoCurve, rightInfoCurve }).ToArray();
                importer.clipAnimations = anims;

                yield return null;
# if UNITY_2020_1_OR_NEWER
                Progress.Report(taskId, (float)current / clipCount, $"Saving results for clip:{clip.name}");
# endif
                importer.SaveAndReimport();
                ResetPlayerPose(pose);
                yield return null;
            }

#if UNITY_2020_1_OR_NEWER
            Progress.Remove(taskId);
# endif
        }
    }
}
