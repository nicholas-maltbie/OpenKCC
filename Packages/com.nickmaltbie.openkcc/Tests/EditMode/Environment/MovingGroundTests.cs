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

using System;
using System.Collections.Generic;
using Moq;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Environment
{
    /// <summary>
    /// Class to validate all types of IMovingGround implementations.
    /// </summary>
    [TestFixture]
    public class MovingGroundTests : TestBase
    {
        /// <summary>
        /// Mock untiy service for managing unity inputs.
        /// </summary>
        private Mock<IUnityService> mockUnityService;

        /// <summary>
        /// Enumerate list of supported moving ground types for testing.
        /// </summary>
        /// <returns>Enumerable list of moving ground types.</returns>
        public static IEnumerable<Type> MovingGroundTypes()
        {
            return new[]
            {
                typeof(MovementTracking),
                typeof(RigidbodyMovingGround),
                typeof(RelativeVelocityMovementTracking),
                typeof(RelativeVelocityRigidbodyTracking),
            };
        }

        /// <summary>
        /// Validate the expected movement for moving ground displacement.
        /// </summary>
        /// <param name="move">Distance to move ground.</param>
        /// <param name="rotation">Amount to rotate the given object.</param>
        /// <param name="type">type fo moving ground being tested.</param>
        [Test]
        public void Validate_MovingGround_DisplacementAndRotation(
            [ValueSource(nameof(TestDirections))] Vector3 move,
            [ValueSource(nameof(TestDirections))] Vector3 rotation,
            [ValueSource(nameof(MovingGroundTypes))] Type type)
        {
            Validate_MovingGroundDisplacement_Helper(
                move,
                rotation * 30, type,
                Vector3.zero);
        }

        /// <summary>
        /// Validate the NoMovementTracking object.
        /// </summary>
        [Test]
        public void Validate_NoMovementTracking(
            [ValueSource(nameof(TestDirections))] Vector3 move,
            [ValueSource(nameof(TestDirections))] Vector3 rotation)
        {
            Validate_MovingGroundDisplacement_Helper(
                move,
                rotation * 30,
                typeof(NoMovementTracking),
                Vector3.zero,
                true,
                Vector3.zero
            );

            NoMovementTracking mt = CreateMovingGround<NoMovementTracking>();
            Assert.IsFalse(mt.ShouldAttach());
            Assert.AreEqual(mt.AvoidTransferMomentum(), true);
            Assert.AreEqual(mt.GetTransferMomentumWeight(Vector3.zero, Vector3.zero), 0.0f);
            Assert.AreEqual(mt.GetMovementWeight(Vector3.zero, Vector3.zero), 0.0f);
        }

        /// <summary>
        /// Validate the expected movement for moving ground displacement.
        /// </summary>
        /// <param name="relativePosition">Relative position to parent object.</param>
        /// <param name="type">type fo moving ground being tested.</param>
        [Test]
        public void Validate_MovingGround_RelativePosition(
            [ValueSource(nameof(TestDirections))] Vector3 relativePosition,
            [ValueSource(nameof(MovingGroundTypes))] Type type)
        {
            foreach (Vector3 translation in TestDirections())
            {
                foreach (Vector3 rotation in TestDirections())
                {
                    Validate_MovingGroundDisplacement_Helper(
                        translation,
                        rotation * 30,
                        type,
                        relativePosition,
                        verifyVelocity: false);
                }
            }
        }

        /// <summary>
        /// Validate movement tracking parameters 
        /// </summary>
        /// <param name="transferMomentum">Value of transfer momentum to be used.</param>
        /// <param name="avoidTransferMomentum">Shoudl avoid transfer momenum be enabled.</param>
        [Test]
        public void Validate_MovementTrackingParams(
            [NUnit.Framework.Range(0.0f, 2.5f, 0.5f)] float transferMomentum,
            [Values] bool avoidTransferMomentum)
        {
            MovementTracking movementTracking = CreateMovingGround<MovementTracking>();
            movementTracking.avoidTransferMomentum = avoidTransferMomentum;
            movementTracking.transferMomentumWeight = transferMomentum;

            Assert.IsTrue(movementTracking.ShouldAttach());
            Assert.AreEqual(movementTracking.AvoidTransferMomentum(), avoidTransferMomentum);
            Assert.AreEqual(movementTracking.GetTransferMomentumWeight(Vector3.zero, Vector3.zero), transferMomentum);
            Assert.AreEqual(movementTracking.GetMovementWeight(Vector3.zero, Vector3.zero), 1.0f);
        }

        /// <summary>
        /// Validate the relative velocity for movement tracking object.
        /// </summary>
        [Test]
        public void Validate_RelativeVelocityMovementTracking()
        {
            RelativeVelocityMovementTracking rvtrack = CreateMovingGround<RelativeVelocityMovementTracking>();
            MoveGround(rvtrack, Vector3.forward, Vector3.zero);

            rvtrack.minimumVelocityThreshold = 1.0f;
            rvtrack.maximumVelocityThreshold = 4.0f;

            Debug.Log($"velAtPoint: {rvtrack.GetVelocityAtPoint(Vector3.zero)}");

            TestUtils.AssertInBounds(rvtrack.GetMovementWeight(Vector3.zero, Vector3.forward * 1), 1, 0.001f);
            TestUtils.AssertInBounds(rvtrack.GetMovementWeight(Vector3.zero, Vector3.forward * 2), 1, 0.001f);
            TestUtils.AssertInBounds(rvtrack.GetMovementWeight(Vector3.zero, Vector3.forward * 3), 0.5f, 0.25f);
            TestUtils.AssertInBounds(rvtrack.GetMovementWeight(Vector3.zero, Vector3.forward * 5), 0, 0.001f);
        }

        /// <summary>
        /// Validate the relative velocity for movement tracking object.
        /// </summary>
        [Test]
        public void Validate_RelativeVelocityRigidbodyTracking()
        {
            RelativeVelocityRigidbodyTracking rvtrack = CreateMovingGround<RelativeVelocityRigidbodyTracking>();
            MoveGround(rvtrack, Vector3.forward, Vector3.zero);

            rvtrack.minimumVelocityThreshold = 1.0f;
            rvtrack.maximumVelocityThreshold = 4.0f;

            Debug.Log($"velAtPoint: {rvtrack.GetVelocityAtPoint(Vector3.zero)}");

            TestUtils.AssertInBounds(rvtrack.GetMovementWeight(Vector3.zero, Vector3.forward * 1), 1, 0.001f);
            TestUtils.AssertInBounds(rvtrack.GetMovementWeight(Vector3.zero, Vector3.forward * 2), 1, 0.001f);
            TestUtils.AssertInBounds(rvtrack.GetMovementWeight(Vector3.zero, Vector3.forward * 3), 0.5f, 0.25f);
            TestUtils.AssertInBounds(rvtrack.GetMovementWeight(Vector3.zero, Vector3.forward * 5), 0, 0.001f);
        }

        /// <summary>
        /// Create a moving ground object of a given type and attach it to a new game object.
        /// </summary>
        /// <typeparam name="E">Type of moving ground to create, must
        /// extend MonoBehaviour and impelment IMovingGround.</typeparam>
        /// <returns>Created moving ground behaviour.</returns>
        protected E CreateMovingGround<E>() where E : MonoBehaviour, IMovingGround
        {
            return CreateMovingGround(typeof(E)) as E;
        }

        /// <summary>
        /// Create a moving ground object of a given type and attach it to a new game object.
        /// </summary>
        /// <param name="componentType">Type of moving ground to create, must
        /// extend MonoBehaviour and impelment IMovingGround.</param>
        /// <returns>Created moving ground behaviour.</returns>
        protected IMovingGround CreateMovingGround(Type componentType)
        {
            GameObject go;
            RegisterGameObject(go = GameObject.CreatePrimitive(PrimitiveType.Sphere));
            go.AddComponent<Rigidbody>();

            var movingGround = go.AddComponent(componentType) as IMovingGround;
            (movingGround as RigidbodyMovingGround)?.Start();

            var movementTracking = movingGround as MovementTracking;
            if (movementTracking != null)
            {
                mockUnityService = new Mock<IUnityService>();
                movementTracking.unityService = mockUnityService.Object;

                mockUnityService.Setup(e => e.deltaTime).Returns(1);
                mockUnityService.Setup(e => e.fixedDeltaTime).Returns(1);
            }

            return movingGround;
        }

        /// <summary>
        /// Helper function for validating moving ground displacement.
        /// </summary>
        /// <param name="move">Displacement of moving ground.</param>
        /// <param name="rotation">Rotation of moving ground.</param>
        /// <param name="type">Type of moving ground to create.</param>
        /// <param name="relativePosition">Relative position of object to moving ground.</param>
        /// <param name="deltaTime">Delta time for processing moving ground.</param>
        protected void Validate_MovingGroundDisplacement_Helper(
            Vector3 move,
            Vector3 rotation,
            Type type,
            Vector3 relativePosition,
            bool verifyVelocity = true,
            Vector3? expectedDisp = null)
        {
            IMovingGround movingGround = CreateMovingGround(type);
            var behaviour = movingGround as MonoBehaviour;

            float deltaTime = 1.0f;

            mockUnityService.Setup(e => e.deltaTime).Returns(deltaTime);
            mockUnityService.Setup(e => e.fixedDeltaTime).Returns(deltaTime);

            MoveGround(behaviour, move, rotation, deltaTime: deltaTime);
            var changeAttitude = Quaternion.Euler(rotation);

            Vector3 deltaRotation = (changeAttitude * relativePosition) - relativePosition;
            Vector3 expectedDisplacement = expectedDisp ?? move + deltaRotation;

            if (verifyVelocity)
            {
                TestUtils.AssertInBounds(
                    movingGround.GetVelocityAtPoint(behaviour.transform.position + relativePosition),
                    expectedDisplacement,
                    0.001f);
            }
        }

        /// <summary>
        /// Move a moving ground a given distance.
        /// </summary>
        /// <param name="monoBehaviour">Mono behaviour to update.</param>
        /// <param name="move">Distance to move.</param>
        /// <param name="rotate">Distance to rotate.</param>
        /// <param name="deltaTime">Expected time to pass, by default is 1 second.</param>
        protected static void MoveGround(MonoBehaviour monoBehaviour, Vector3 move, Vector3 rotate, float deltaTime = 1.0f)
        {
            var movementTracking = monoBehaviour as MovementTracking;
            movementTracking?.FixedUpdate();
            movementTracking?.FixedUpdate();

            monoBehaviour.transform.position += move;
            monoBehaviour.transform.eulerAngles += rotate;

            Rigidbody rb = monoBehaviour.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = monoBehaviour.transform.position;
                rb.rotation = monoBehaviour.transform.rotation;
                rb.velocity = move / deltaTime;
                rb.angularVelocity = rotate * Mathf.Deg2Rad / deltaTime;
            }

            movementTracking?.FixedUpdate();
        }
    }
}
