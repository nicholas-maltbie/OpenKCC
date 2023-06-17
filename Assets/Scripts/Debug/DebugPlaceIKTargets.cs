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

using System.Collections.Generic;
using UnityEngine;

public class DebugPlaceIKTargets : MonoBehaviour
{
    private static readonly (AvatarIKGoal, HumanBodyBones)[] targets = new[]
    {
        (AvatarIKGoal.LeftHand, HumanBodyBones.LeftHand),
        (AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot),
        (AvatarIKGoal.RightHand, HumanBodyBones.RightHand),
        (AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot),
    };

    public bool enableIk = true;
    private Dictionary<AvatarIKGoal, Transform> debugTargets;
    private Animator animator;

    public void Start()
    {
        animator = GetComponent<Animator>();
        debugTargets = new Dictionary<AvatarIKGoal, Transform>();
        
        foreach ((AvatarIKGoal goal, HumanBodyBones bone) in targets)
        {
            Transform boneTr = animator.GetBoneTransform(bone);
            Transform debugTr = new GameObject(bone.ToString() + "Target").transform;
            debugTr.SetParent(transform);
            debugTr.position = boneTr.position;
            debugTr.rotation = transform.rotation;
            debugTargets[goal] = debugTr;
        }
    }

    public void OnAnimatorIK()
    {
        foreach ((AvatarIKGoal goal, HumanBodyBones bone) in targets)
        {
            float weight = enableIk ? 1.0f : 0.0f;
            Transform target = debugTargets[goal];
            animator.SetIKPosition(goal, target.position);
            animator.SetIKRotation(goal, target.rotation);
            animator.SetIKPositionWeight(goal, weight);
            animator.SetIKRotationWeight(goal, weight);
        }
    }
}
