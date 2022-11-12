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

using System.Collections;
using System.Collections.Generic;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.PlayMode.Character
{
    /// <summary>
    /// Test basic movement scenarios for KCCStateMachine with moving ground.
    /// </summary>
    public class KCCStateMachineMovingGroundTests : TestBase
    {
        private KCCStateMachine kccStateMachine;
        private StickControl moveStick;
        private ButtonControl jumpButton;

        private Gamepad gamepad;
        private InputAction moveInputAction;
        private InputAction jumpInputAction;
        private JumpAction jumpAction;
        private ParentConstraint constraint;

        private GameObject floor;

        public static IEnumerable<Vector3> DirectionsOnFlatPlane()
        {
            return new [] {
                Vector3.forward,
                Vector3.left,
                Vector3.forward + Vector3.left,
                Vector3.forward + Vector3.right,
            };
        }

        public static IEnumerable<Vector3> RotationAxis()
        {
            return new [] {
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
            };
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            GameObject go = new GameObject();
            CapsuleCollider capsuleCollider = go.AddComponent<CapsuleCollider>();
            go.AddComponent<CapsuleColliderCast>();
            capsuleCollider.center = new Vector3(0, 1, 0);
            capsuleCollider.height = 2.0f;
            capsuleCollider.radius = 0.5f;

            var controller = new AnimatorController();
            controller.AddLayer("base");
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            rootStateMachine.AddState(KCCStateMachine.IdleAnimState);
            rootStateMachine.AddState(KCCStateMachine.JumpAnimState);
            rootStateMachine.AddState(KCCStateMachine.LandingAnimState);
            rootStateMachine.AddState(KCCStateMachine.WalkingAnimState);
            rootStateMachine.AddState(KCCStateMachine.SlidingAnimState);
            rootStateMachine.AddState(KCCStateMachine.FallingAnimState);
            rootStateMachine.AddState(KCCStateMachine.LongFallingAnimState);

            Animator anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            var jumpInput = new BufferedInput();
            PlayerInput playerInput = go.AddComponent<PlayerInput>();
            gamepad = InputSystem.AddDevice<Gamepad>();
            InputActionAsset inputActionAsset = CreateScriptableObject<InputActionAsset>();
            InputActionMap actionMap = inputActionAsset.AddActionMap("testMap");
            playerInput.actions = inputActionAsset;
            playerInput.currentActionMap = actionMap;
            jumpButton = gamepad.aButton;
            moveStick = gamepad.leftStick;
            jumpInputAction = actionMap.AddAction("jumpAction", InputActionType.Button, jumpButton.path);
            moveInputAction = actionMap.AddAction("moveAction", InputActionType.Value, moveStick.path);

            jumpInputAction.Enable();
            moveInputAction.Enable();

            jumpInput.inputAction = InputActionReference.Create(jumpInputAction);
            jumpInput.cooldown = 1.0f;
            jumpInput.bufferTime = 3.0f;
            jumpAction = new JumpAction();
            jumpAction.jumpInput = jumpInput;

            kccStateMachine = go.AddComponent<KCCStateMachine>();
            kccStateMachine.jumpAction = jumpAction;
            kccStateMachine.moveAction = InputActionReference.Create(moveInputAction);
            kccStateMachine.Start();

            constraint = kccStateMachine.GetComponent<ParentConstraint>();

            floor = CreateGameObject();
            BoxCollider box = floor.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = new Vector3(10, 1, 10);
            box.transform.position = Vector3.zero;
            box.transform.rotation = Quaternion.identity;
            floor.transform.position = new Vector3(0, -0.5f, 0);
        }
        
        [TearDown]
        public override void TearDown()
        {
            InputSystem.RemoveDevice(gamepad);
            GameObject.Destroy(kccStateMachine.gameObject);
            base.TearDown();
        }

        [UnityTest]
        public IEnumerator FollowLinearMovement(
            [ValueSource(nameof(TestDirections))] Vector3 dir,
            [ValueSource(nameof(DirectionsOnFlatPlane))] Vector3 relativePos
        )
        {
            kccStateMachine.TeleportPlayer(relativePos);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Move the box forward and wait a fixed update, the player should
            // move along with the box
            for (int i = 0; i < 10; i++)
            {
                // Account for drift in player overlap with platform.
                relativePos = floor.transform.InverseTransformPoint(kccStateMachine.transform.position);

                // Translate the current floor
                floor.transform.position += dir * 0.1f;

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                Assert.AreEqual(floor, kccStateMachine.groundedState.Floor);
                Assert.IsTrue(kccStateMachine.groundedState.StandingOnGroundOrOverlap);
                Assert.AreEqual(1, constraint.sourceCount);
                Assert.AreEqual(floor.transform, constraint.GetSource(0).sourceTransform);
                yield return new WaitForFixedUpdate();

                Vector3 expectedPosition = floor.transform.position + relativePos;
                TestUtils.AssertInBounds(kccStateMachine.transform.position, expectedPosition, range: 0.25f);
            }
        }

        [UnityTest]
        public IEnumerator FollowAngularMovement(
            [ValueSource(nameof(RotationAxis))] Vector3 axis,
            [ValueSource(nameof(DirectionsOnFlatPlane))] Vector3 relativePos
        )
        {
            kccStateMachine.TeleportPlayer(relativePos);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Move the box forward and wait a fixed update, the player should
            // move along with the box
            for (int i = 0; i < 10; i++)
            {
                // Account for drift in player overlap with platform.
                relativePos = floor.transform.InverseTransformPoint(kccStateMachine.transform.position);

                // Rotated by the current floor
                floor.transform.rotation *= Quaternion.Euler(axis.normalized * 5);

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(floor, kccStateMachine.groundedState.Floor);
                Assert.IsTrue(kccStateMachine.groundedState.StandingOnGroundOrOverlap);
                Assert.AreEqual(1, constraint.sourceCount);
                Assert.AreEqual(floor.transform, constraint.GetSource(0).sourceTransform);
                
                // Expected position should be relative position
                Vector3 expectedPos = floor.transform.rotation * relativePos;
                TestUtils.AssertInBounds(kccStateMachine.transform.position, expectedPos, 1.0f);
            }
        }
    }
}
