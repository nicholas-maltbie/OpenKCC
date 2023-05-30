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

using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Character.Events;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{

    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Character.KCCStateMachine"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class KCCStateMachineTests : KCCStateMachineTestBase
    {
        private MockUnityService unityServiceMock;
        private MockColliderCast colliderCastMock;
        private MockCameraControls cameraControlsMock;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            unityServiceMock = new MockUnityService();
            colliderCastMock = new MockColliderCast();
            cameraControlsMock = new MockCameraControls();

            unityServiceMock.deltaTime = 1.0f;
            unityServiceMock.fixedDeltaTime = 1.0f;

            kccStateMachine.Awake();
            kccStateMachine.CameraControls = cameraControlsMock;

            moveEngine._colliderCast = colliderCastMock;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            PlayerInputUtils.playerMovementState = PlayerInputState.Allow;

        }

        [Test]
        public void Validate_KCCStateMachine_Falling_Transition()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, didHit: false);

            Assert.AreEqual(typeof(KCCStateMachine.IdleState), kccStateMachine.CurrentState);
            unityServiceMock.deltaTime = 1.0f;

            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();
            kccStateMachine.FixedUpdate();

            Assert.AreEqual(typeof(KCCStateMachine.FallingState), kccStateMachine.CurrentState);
            Assert.IsFalse(KCCGroundedState.StandingOnGround);
            Assert.IsFalse(KCCGroundedState.Sliding);
            Assert.IsTrue(KCCGroundedState.Falling);
        }

        [Test]
        public void Validate_KCCStateMachine_SprintingInput()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true);
            Set(moveStick, Vector2.up);
            Set(sprintButton, 1);
            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();
            Assert.AreEqual(typeof(KCCStateMachine.SprintingState), kccStateMachine.CurrentState);
            kccStateMachine.FixedUpdate();
        }

        [Test]
        public void Validate_KCCStateMachine_Sliding_Transition()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.Lerp(Vector3.up, Vector3.forward, 0.9f), didHit: true);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));

            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.SlidingState));
            Assert.IsTrue(KCCGroundedState.StandingOnGround);
            Assert.IsTrue(KCCGroundedState.Sliding);
            Assert.IsFalse(KCCGroundedState.Falling);
        }

        [Test]
        public void Validate_KCCStateMachine_MovingGround_Teleport()
        {
            GameObject movingGround = CreateGameObject();
            BoxCollider collider = movingGround.AddComponent<BoxCollider>();
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true, collider: collider);

            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();

            kccStateMachine.transform.position = Vector3.forward * 10;
            Assert.AreEqual(kccStateMachine.transform.position, Vector3.forward * 10);

            KCCTestUtils.SetupCastSelf(colliderCastMock, didHit: false);
            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();

            kccStateMachine.transform.position = Vector3.back * 10;
            Assert.AreEqual(kccStateMachine.transform.position, Vector3.back * 10);
        }

        [Test]
        public void Validate_KCCStateMachine_MovingGround_Update(
            [ValueSource(nameof(TestDirections))] Vector3 direction,
            [Values] bool shouldAttach
        )
        {
            GameObject movingGround = CreateGameObject();
            MovementTracking tracking = movingGround.AddComponent<MovementTracking>();
            BoxCollider collider = movingGround.AddComponent<BoxCollider>();
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true, collider: collider);

            var unityServiceMock = new MockUnityService();
            tracking.shouldAttach = shouldAttach;
            tracking.unityService = unityServiceMock;
            unityServiceMock.fixedDeltaTime = 1.0f;
            unityServiceMock.deltaTime = 1.0f;

            tracking.FixedUpdate();
            tracking.transform.position += direction;
            tracking.FixedUpdate();

            // Update the moving ground object and assert
            // that the player is attached to the moving ground
            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();

            tracking.FixedUpdate();
            tracking.transform.position += direction;
            tracking.FixedUpdate();

            Assert.AreEqual(movingGround, KCCGroundedState.Floor);
        }

        [Test]
        public void Validate_KCCStateMachine_Move_Transition()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true);
            Set(moveStick, Vector2.up);

            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();

            Assert.AreEqual(typeof(KCCStateMachine.WalkingState), kccStateMachine.CurrentState);
            TestUtils.AssertInBounds(kccStateMachine.InputMovement, Vector3.forward);

            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();

            Assert.AreEqual(typeof(KCCStateMachine.WalkingState), kccStateMachine.CurrentState);
            Assert.IsTrue(KCCGroundedState.StandingOnGround);
            Assert.IsFalse(KCCGroundedState.Sliding);
            Assert.IsFalse(KCCGroundedState.Falling);
        }

        [Test]
        public void Validate_KCCStateMachine_ApplyJump([NUnit.Framework.Range(0.0f, 5.0f, 1.0f)] float strength)
        {
            Assert.AreEqual(typeof(KCCStateMachine.IdleState), kccStateMachine.CurrentState);
            kccStateMachine.ApplyJump(Vector3.up * strength);
            TestUtils.AssertInBounds(kccStateMachine.Velocity, Vector3.up * strength);
            Assert.AreEqual(typeof(KCCStateMachine.JumpState), kccStateMachine.CurrentState);
        }

        [Test]
        public void Validate_KCCStateMachine_PlayerInputUtilsFlags()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true);
            Set(moveStick, Vector2.up);
            PlayerInputUtils.playerMovementState = PlayerInputState.Deny;

            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();
            TestUtils.AssertInBounds(kccStateMachine.InputMovement, Vector3.zero);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));

            kccStateMachine.Update();
            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));
            Assert.IsTrue(KCCGroundedState.StandingOnGround);
            Assert.IsFalse(KCCGroundedState.Sliding);
            Assert.IsFalse(KCCGroundedState.Falling);

            PlayerInputUtils.playerMovementState = PlayerInputState.Allow;
        }

        [Test]
        public void Validate_KCCStateMachine_SprintSpeedValue()
        {
            kccStateMachine.RaiseEvent(StartMoveInput.Instance);
            kccStateMachine.RaiseEvent(StartSprintEvent.Instance);
        }

        [Test]
        public void Validate_KCCStateMachine_TeleportPlayer()
        {
            kccStateMachine.TeleportPlayer(Vector3.forward);

            Assert.AreEqual(Vector3.forward, kccStateMachine.transform.position);
        }
    }
}
