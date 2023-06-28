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

using System;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

[Serializable]
public struct InputMovementCommand
{
    public float time;
    public Vector3 dir;
}

[RequireComponent(typeof(KCCMovementEngine))]
public class DummyMovementEngine : MonoBehaviour
{
    private KCCMovementEngine movementEngine;
    private Animator animator;

    private float elapsed;
    private int commandIdx;

    public InputMovementCommand[] commands;
    public float rotationSpeed = 30.0f;

    public virtual Vector3 Direction => commands[commandIdx].dir;

    private Vector3 velocity = Vector3.zero;

    public void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        movementEngine = GetComponent<KCCMovementEngine>();
    }

    public void FixedUpdate()
    {
        if (movementEngine.GroundedState.StandingOnGround)
        {
            velocity = Vector3.zero;
        }
        else
        {
            velocity += Physics.gravity * Time.fixedDeltaTime;
        }

        // Update command if needed
        if (elapsed > commands[commandIdx].time)
        {
            commandIdx = (commandIdx + 1) % commands.Length;
            elapsed = 0.0f;
        }

        movementEngine.MovePlayer(Direction * Time.fixedDeltaTime, velocity * Time.fixedDeltaTime);
        elapsed += Time.fixedDeltaTime;
    }

    public void Update()
    {
        // rotate player towards movement direction
        if (Direction.magnitude >= 0.001f)
        {
            Quaternion currentRot = animator.transform.rotation;
            var targetRot = Quaternion.LookRotation(Direction, Vector3.up);
            animator.transform.rotation = Quaternion.RotateTowards(currentRot, targetRot, rotationSpeed * Time.deltaTime);
        }

        float moveX = animator.GetFloat("MoveX");
        float moveY = animator.GetFloat("MoveY");

        // Get the relative moveX and moveY to include
        // the delta in rotation between the avatar's current heading
        // and the desired world space input
        Vector3 playerHeading = animator.transform.forward;
        Vector3 movementDir = Direction;

        var relative = Quaternion.FromToRotation(playerHeading, movementDir);
        Vector3 relativeMovement = relative * Vector3.forward;

        if (Direction.magnitude <= KCCUtils.Epsilon)
        {
            relativeMovement = Vector3.zero;
        }

        moveX = Mathf.Lerp(moveX, relativeMovement.x, 4 * Time.deltaTime);
        moveY = Mathf.Lerp(moveY, relativeMovement.z, 4 * Time.deltaTime);
        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveY", moveY);
    }
}
