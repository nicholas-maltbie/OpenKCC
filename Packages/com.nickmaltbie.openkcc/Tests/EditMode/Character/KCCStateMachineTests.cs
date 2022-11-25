﻿// Copyright (C) 2022 Nicholas Maltbie
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

using Moq;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Character.Events;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Character.KCCStateMachine"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class KCCStateMachineTests : KCCStateMachineTestBase
    {
        private Mock<IUnityService> unityServiceMock;
        private Mock<IColliderCast> colliderCastMock;
        private Mock<IKCCConfig> kccConfigMock;
        private Mock<ICharacterPush> characterPushMock;
        private Mock<ICameraControls> cameraControlsMock;

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
            unityServiceMock.Setup(e => e.fixedDeltaTime).Returns(1.0f);

            kccGroundedState = new KCCGroundedState();
            kccConfigMock.Setup(e => e.ColliderCast).Returns(colliderCastMock.Object);
            kccConfigMock.Setup(e => e.Up).Returns(Vector3.up);
            kccGroundedState.groundedDistance = 0.01f;
            kccGroundedState.maxWalkAngle = 60.0f;

            kccStateMachine.Awake();
            kccStateMachine.Start();
            kccStateMachine._cameraControls = cameraControlsMock.Object;
            kccStateMachine.config.groundedState = kccGroundedState;
            kccStateMachine.config._characterPush = characterPushMock.Object;
            kccStateMachine.config._colliderCast = colliderCastMock.Object;
        }

        [Test]
        public void Validate_KCCStateMachine_Falling_Transition()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, didHit: false);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));

            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.FallingState));
            Assert.IsFalse(kccStateMachine.config.groundedState.StandingOnGround);
            Assert.IsFalse(kccStateMachine.config.groundedState.Sliding);
            Assert.IsTrue(kccStateMachine.config.groundedState.Falling);
        }

        [Test]
        public void Validate_KCCStateMachine_SprintingInput()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true);
            Set(moveStick, Vector2.up);
            Set(sprintButton, 1);
            Debug.Log($"MoveInput:{moveInputAction.ReadValue<Vector2>()} SprintInput:{sprintInputAction.IsPressed()}");
            kccStateMachine.Update();
            Assert.AreEqual(typeof(KCCStateMachine.SprintingState), kccStateMachine.CurrentState);
            kccStateMachine.FixedUpdate();
        }

        [Test]
        public void Validate_KCCStateMachine_Sliding_Transition()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.Lerp(Vector3.up, Vector3.forward, 0.9f), didHit: true);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));

            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.SlidingState));
            Assert.IsTrue(kccStateMachine.config.groundedState.StandingOnGround);
            Assert.IsTrue(kccStateMachine.config.groundedState.Sliding);
            Assert.IsFalse(kccStateMachine.config.groundedState.Falling);
        }

        [Test]
        public void Validate_KCCStateMachine_MovingGround_Teleport()
        {
            GameObject movingGround = CreateGameObject();
            BoxCollider collider = movingGround.AddComponent<BoxCollider>();
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true, collider: collider);
            ParentConstraint constraint = kccStateMachine.GetComponent<ParentConstraint>();

            Assert.AreEqual(constraint.sourceCount, 0);
            kccStateMachine.FixedUpdate();

            Assert.AreEqual(constraint.sourceCount, 1);

            kccStateMachine.TeleportPlayer(Vector3.forward * 10);
            Assert.AreEqual(kccStateMachine.transform.position, Vector3.forward * 10);

            KCCTestUtils.SetupCastSelf(colliderCastMock, didHit: false);
            kccStateMachine.FixedUpdate();
            Assert.AreEqual(constraint.sourceCount, 0);

            kccStateMachine.TeleportPlayer(Vector3.back * 10);
            Assert.AreEqual(kccStateMachine.transform.position, Vector3.back * 10);
        }

        [Test]
        public void Validate_KCCStateMachine_MovingGround_ResetConstraints()
        {
            GameObject movingGround = CreateGameObject();
            BoxCollider collider = movingGround.AddComponent<BoxCollider>();
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true, collider: collider);
            ParentConstraint constraint = kccStateMachine.GetComponent<ParentConstraint>();

            Assert.AreEqual(constraint.sourceCount, 0);
            kccStateMachine.FixedUpdate();

            Assert.AreEqual(constraint.sourceCount, 1);

            KCCTestUtils.SetupCastSelf(colliderCastMock, didHit: false);
            kccStateMachine.FixedUpdate();
            Assert.AreEqual(constraint.sourceCount, 0);
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
            ParentConstraint constraint = kccStateMachine.GetComponent<ParentConstraint>();

            var unityServiceMock = new Mock<IUnityService>();
            tracking.shouldAttach = shouldAttach;
            tracking.unityService = unityServiceMock.Object;
            unityServiceMock.Setup(e => e.fixedDeltaTime).Returns(1.0f);
            unityServiceMock.Setup(e => e.deltaTime).Returns(1.0f);

            tracking.FixedUpdate();
            tracking.transform.position += direction;
            tracking.FixedUpdate();

            // Update the moving ground object and assert
            // that the player is attached to the moving ground
            Assert.AreEqual(Vector3.zero, kccStateMachine.transform.position);
            kccStateMachine.UpdateMovingGround();

            tracking.FixedUpdate();
            tracking.transform.position += direction;
            tracking.FixedUpdate();

            Assert.AreEqual(movingGround, kccStateMachine.config.groundedState.Floor);
            Assert.AreEqual(shouldAttach ? 1 : 0, constraint.sourceCount);

            if (shouldAttach)
            {
                Assert.AreEqual(movingGround.transform, constraint.GetSource(0).sourceTransform);
                Assert.AreEqual(1, constraint.GetSource(0).weight);
                Assert.AreEqual(-direction, constraint.GetTranslationOffset(0));
            }
            else
            {
                Assert.AreEqual(direction, kccStateMachine.transform.position);
            }
        }

        [Test]
        public void Validate_KCCStateMachine_MovingGround_GetVelocity()
        {
            GameObject movingGround = CreateGameObject();
            MovementTracking tracking = movingGround.AddComponent<MovementTracking>();
            BoxCollider collider = movingGround.AddComponent<BoxCollider>();

            var unityServiceMock = new Mock<IUnityService>();
            tracking.unityService = unityServiceMock.Object;
            unityServiceMock.Setup(e => e.fixedDeltaTime).Returns(1.0f);
            unityServiceMock.Setup(e => e.deltaTime).Returns(1.0f);

            tracking.FixedUpdate();
            tracking.transform.position += Vector3.forward;
            tracking.FixedUpdate();

            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true, collider: collider);
            kccStateMachine.UpdateGroundedState();
            Vector3 velocity = kccStateMachine.GetGroundVelocity();
            Assert.AreEqual(velocity, tracking.GetVelocityAtPoint(Vector3.zero));
        }

        [Test]
        public void Validate_KCCStateMachine_GetVelocity_NoMovementTracking(
            [ValueSource(nameof(TestDirections))] Vector3 dir
        )
        {
            GameObject movingGround = CreateGameObject();
            unityServiceMock.Setup(e => e.fixedDeltaTime).Returns(1.0f);
            kccStateMachine.config.maxDefaultLaunchVelocity = 10.0f;
            BoxCollider collider = movingGround.AddComponent<BoxCollider>();

            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true, collider: collider);

            kccStateMachine.UpdateGroundedState();
            Vector3 velocity = kccStateMachine.GetGroundVelocity();
            Assert.AreEqual(velocity, Vector3.zero);

            // have the player move a bit during the next fixed updates
            KCCTestUtils.SetupCastSelf(colliderCastMock, didHit: false);
            for (int i = 0; i < 10; i++)
            {
                kccStateMachine.transform.position += dir;
                kccStateMachine.FixedUpdate();
            }

            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true, collider: collider);
            kccStateMachine.UpdateGroundedState();
            // Now the velocity should be within the bounds of about direction
            velocity = kccStateMachine.GetGroundVelocity();
        }

        [Test]
        public void Validate_KCCStateMachine_Move_Transition()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: Vector3.up, didHit: true);
            Set(moveStick, Vector2.up);
            Debug.Log("Move input action value: " + moveInputAction.ReadValue<Vector2>());

            kccStateMachine.Update();

            Assert.AreEqual(typeof(KCCStateMachine.WalkingState), kccStateMachine.CurrentState);
            TestUtils.AssertInBounds(kccStateMachine.InputMovement, Vector3.forward);

            kccStateMachine.FixedUpdate();

            Assert.AreEqual(typeof(KCCStateMachine.WalkingState), kccStateMachine.CurrentState);
            Assert.IsTrue(kccStateMachine.config.groundedState.StandingOnGround);
            Assert.IsFalse(kccStateMachine.config.groundedState.Sliding);
            Assert.IsFalse(kccStateMachine.config.groundedState.Falling);
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
            TestUtils.AssertInBounds(kccStateMachine.InputMovement, Vector3.zero);

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));

            kccStateMachine.FixedUpdate();

            Assert.AreEqual(kccStateMachine.CurrentState, typeof(KCCStateMachine.IdleState));
            Assert.IsTrue(kccStateMachine.config.groundedState.StandingOnGround);
            Assert.IsFalse(kccStateMachine.config.groundedState.Sliding);
            Assert.IsFalse(kccStateMachine.config.groundedState.Falling);

            PlayerInputUtils.playerMovementState = PlayerInputState.Allow;
        }

        [Test]
        public void Validate_KCCStateMachine_SprintSpeedValue()
        {
            kccStateMachine.RaiseEvent(StartMoveInput.Instance);
            kccStateMachine.RaiseEvent(StartSprintEvent.Instance);
            kccStateMachine.ApplyMovement();
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
            kccStateMachine.config.maxBounces = maxBounces;
            kccStateMachine.config.pushDecay = pushDecay;
            kccStateMachine.config.verticalSnapUp = verticalSnapUp;
            kccStateMachine.config.stepUpDepth = stepUpDepth;
            kccStateMachine.config.anglePower = anglePower;

            Assert.AreEqual(kccStateMachine.config.MaxBounces, maxBounces);
            Assert.AreEqual(kccStateMachine.config.PushDecay, pushDecay);
            Assert.AreEqual(kccStateMachine.config.VerticalSnapUp, verticalSnapUp);
            Assert.AreEqual(kccStateMachine.config.StepUpDepth, stepUpDepth);
            Assert.AreEqual(kccStateMachine.config.AnglePower, anglePower);
        }
    }
}
