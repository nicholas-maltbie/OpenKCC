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
using Moq;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.OpenKCC.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Character.KCCStateMachine"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class KCCStateMachineTests : TestBase
    {
        private Mock<IUnityService> unityServiceMock;
        private Mock<IColliderCast> colliderCastMock;
        private Mock<IKCCConfig> kccConfigMock;
        private Mock<ICharacterPush> characterPushMock;
        private Mock<ICameraControls> cameraControlsMock;

        private StickControl moveStick;
        private ButtonControl jumpButton;

        private Gamepad gamepad;
        private InputAction moveInputAction;
        private InputAction jumpInputAction;
        private JumpAction jumpAction;
        private KCCGroundedState kccGroundedState;
        private KCCStateMachine kccStateMachine;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            unityServiceMock = new Mock<IUnityService>();
            colliderCastMock = new Mock<IColliderCast>();
            cameraControlsMock = new Mock<ICameraControls>();
            kccConfigMock = new Mock<IKCCConfig>();
            characterPushMock = new Mock<ICharacterPush>();

            unityServiceMock.Setup(e => e.deltaTime).Returns(1.0f);

            kccGroundedState = new KCCGroundedState();
            kccConfigMock.Setup(e => e.ColliderCast).Returns(colliderCastMock.Object);
            kccConfigMock.Setup(e => e.Up).Returns(Vector3.up);
            kccGroundedState.groundedDistance = 0.01f;
            kccGroundedState.maxWalkAngle = 60.0f;

            InputActionMap actionMap;
            var jumpInput = new BufferedInput();
            (gamepad, _, actionMap) = base.SetupInputDevice<Gamepad>();
            jumpButton = gamepad.aButton;
            moveStick = gamepad.leftStick;
            jumpInputAction = actionMap.AddAction("jumpAction", InputActionType.Button, jumpButton.path);
            moveInputAction = actionMap.AddAction("moveAction", InputActionType.Value, moveStick.path);

            jumpInputAction.Enable();
            moveInputAction.Enable();

            jumpInput.unityService = unityServiceMock.Object;
            jumpInput.inputAction = InputActionReference.Create(jumpInputAction);
            jumpInput.cooldown = 1.0f;
            jumpInput.bufferTime = 3.0f;

            jumpAction = new JumpAction();
            jumpAction.jumpInput = jumpInput;
            jumpAction.unityService = unityServiceMock.Object;

            GameObject go = base.CreateGameObject();
            kccStateMachine = go.AddComponent<KCCStateMachine>();
            kccStateMachine.jumpAction = jumpAction;
            kccStateMachine.moveAction = InputActionReference.Create(moveInputAction);

            kccStateMachine.Awake();
            kccStateMachine._cameraControls = cameraControlsMock.Object;
            kccStateMachine._characterPush = characterPushMock.Object;
            kccStateMachine._colliderCast = colliderCastMock.Object;
        }

        [Test]
        public void Validate_KCCStateMachine_Idle_FixedUpdate()
        {
            TestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));

            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));
            Assert.IsTrue(kccStateMachine.groundedState.StandingOnGround);
            Assert.IsFalse(kccStateMachine.groundedState.Sliding);
            Assert.IsFalse(kccStateMachine.groundedState.Falling);
        }

        [Test]
        public void Validate_KCCStateMachine_Falling_Transition()
        {
            TestUtils.SetupCastSelf(colliderCastMock, didHit: false);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));

            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.FallingState));
            Assert.IsFalse(kccStateMachine.groundedState.StandingOnGround);
            Assert.IsFalse(kccStateMachine.groundedState.Sliding);
            Assert.IsTrue(kccStateMachine.groundedState.Falling);
        }

        [Test]
        public void Validate_KCCStateMachine_Sliding_Transition()
        {
            TestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.Lerp(Vector3.up, Vector3.forward, 0.9f), didHit: true);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));

            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.SlidingState));
            Assert.IsTrue(kccStateMachine.groundedState.StandingOnGround);
            Assert.IsTrue(kccStateMachine.groundedState.Sliding);
            Assert.IsFalse(kccStateMachine.groundedState.Falling);
        }

        [UnityTest]
        public IEnumerator Validate_KCCStateMachine_Move_Transition()
        {
            TestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true);
            Set(moveStick, Vector2.up);
            Debug.Log("Move input action value: " + moveInputAction.ReadValue<Vector2>());
            yield return null;

            kccStateMachine.Update();
            TestUtils.AssertInBounds(kccStateMachine.InputMovement, Vector3.forward);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.WalkingState));

            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.WalkingState));
            Assert.IsTrue(kccStateMachine.groundedState.StandingOnGround);
            Assert.IsFalse(kccStateMachine.groundedState.Sliding);
            Assert.IsFalse(kccStateMachine.groundedState.Falling);
        }

        [Test]
        public void Validate_KCCStateMachine_ApplyJump([NUnit.Framework.Range(0.0f, 5.0f, 1.0f)] float strength)
        {
            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));
            kccStateMachine.ApplyJump(Vector3.up * strength);
            TestUtils.AssertInBounds(kccStateMachine.Velocity, Vector3.up * strength);
            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.FallingState));
        }

        [Test]
        public void Validate_KCCStateMachine_Values(
            [Random(2)] int maxBounces,
            [Random(2)] float pushDecay,
            [Random(2)] float verticalSnapUp,
            [Random(2)] float stepUpDepth,
            [Random(2)] float anglePower
        )
        {
            kccStateMachine.maxBounces = maxBounces;
            kccStateMachine.pushDecay = pushDecay;
            kccStateMachine.verticalSnapUp = verticalSnapUp;
            kccStateMachine.stepUpDepth = stepUpDepth;
            kccStateMachine.anglePower = anglePower;

            Assert.AreEqual(kccStateMachine.MaxBounces, kccStateMachine.maxBounces);
            Assert.AreEqual(kccStateMachine.PushDecay, kccStateMachine.pushDecay);
            Assert.AreEqual(kccStateMachine.VerticalSnapUp, kccStateMachine.verticalSnapUp);
            Assert.AreEqual(kccStateMachine.StepUpDepth, kccStateMachine.stepUpDepth);
            Assert.AreEqual(kccStateMachine.AnglePower, kccStateMachine.anglePower);
        }
    }
}
