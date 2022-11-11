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

            Set(gamepad.aButton, 0);
            Set(gamepad.leftStick, Vector3.zero);

            kccStateMachine = go.AddComponent<KCCStateMachine>();
            kccStateMachine.jumpAction = jumpAction;
            kccStateMachine.moveAction = InputActionReference.Create(moveInputAction);
            kccStateMachine.Start();

            constraint = kccStateMachine.GetComponent<ParentConstraint>();
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
            [ValueSource(nameof(TestDirections))] Vector3 dir
        )
        {
            GameObject floor = CreateGameObject();
            BoxCollider box = floor.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = Vector3.one;
            kccStateMachine.transform.position = Vector3.zero;
            floor.transform.position = new Vector3(0, -0.51f, 0);

            yield return null;

            while (!kccStateMachine.groundedState.StandingOnGround)
            {
                yield return new WaitForFixedUpdate();
            }

            UnityEngine.Debug.Log($"OnGround:{kccStateMachine.groundedState.OnGround} DistanceToGround:{kccStateMachine.groundedState.DistanceToGround}");

            // Assert that the player is attached to the floor.
            Assert.AreEqual(floor, kccStateMachine.groundedState.Floor);
            Assert.IsTrue(kccStateMachine.groundedState.StandingOnGround);
            Assert.AreEqual(1, constraint.sourceCount);
            Assert.AreEqual(floor.transform, constraint.GetSource(0).sourceTransform);

            // Move the box forward and wait a fixed update, the player should
            // move along with the box
            for (int i = 0; i < 10; i++)
            {
                box.transform.position += dir * 0.1f;
                yield return new WaitForFixedUpdate();
                yield return null;
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(floor, kccStateMachine.groundedState.Floor);
                Assert.IsTrue(kccStateMachine.groundedState.StandingOnGround);
                Assert.AreEqual(1, constraint.sourceCount);
                Assert.AreEqual(floor.transform, constraint.GetSource(0).sourceTransform);
                yield return new WaitForFixedUpdate();
                yield return null;
                TestUtils.AssertInBounds(kccStateMachine.transform.position, box.transform.position + Vector3.up * 0.5f, range: 0.1f);
            }
        }
    }
}
