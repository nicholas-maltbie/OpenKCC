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
using System.Linq;
using System.Reflection;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    /// <summary>
    /// Tests to validate physics wrappers.
    /// </summary>
    [TestFixture]
    public class PhysicsWrapperTests : TestBase
    {
        [Test]
        public void Validate_RaycastWrapper()
        {
            RaycastHit raycastHitData = default;
            IRaycastHit raycastHitWrapper = new RaycastHitWrapper(raycastHitData);

            Assert.AreEqual(raycastHitData.collider, raycastHitWrapper.collider);
            Assert.AreEqual(raycastHitData.triangleIndex, raycastHitWrapper.triangleIndex);
            Assert.AreEqual(raycastHitData.textureCoord, raycastHitWrapper.textureCoord);
            Assert.AreEqual(raycastHitData.textureCoord2, raycastHitWrapper.textureCoord2);
            Assert.AreEqual(raycastHitData.transform, raycastHitWrapper.transform);
            Assert.AreEqual(raycastHitData.rigidbody, raycastHitWrapper.rigidbody);
            Assert.AreEqual(raycastHitData.articulationBody, raycastHitWrapper.articulationBody);

            Vector3 testVector3 = Vector3.forward;
            raycastHitWrapper.point = Vector3.forward;
            raycastHitWrapper.normal = Vector3.forward;
            raycastHitWrapper.barycentricCoordinate = Vector3.forward;
            Assert.AreEqual(testVector3, raycastHitWrapper.point);
            Assert.AreEqual(testVector3, raycastHitWrapper.normal);

            float testFloat = 3.14f;
            raycastHitWrapper.distance = testFloat;
            Assert.AreEqual(testFloat, raycastHitWrapper.distance);
        }

        [Test]
        public void Validate_KinematicCharacterControllerHit()
        {
            Collider hit = default;
            Rigidbody rigidbody = default;
            GameObject gameObject = default;
            Transform transform = default;
            Vector3 point = default;
            Vector3 normal = default;
            Vector3 moveDirection = default;
            float moveLength = default;

            IControllerColliderHit controllerColliderHit = new KinematicCharacterControllerHit(
                hit, rigidbody, gameObject, transform, point, normal, moveDirection, moveLength);

            Assert.AreEqual(controllerColliderHit.collider, hit);
            Assert.AreEqual(controllerColliderHit.gameObject, gameObject);
            Assert.AreEqual(controllerColliderHit.transform, transform);
            Assert.AreEqual(controllerColliderHit.point, point);
            Assert.AreEqual(controllerColliderHit.normal, normal);
            Assert.AreEqual(controllerColliderHit.moveDirection, moveDirection);
            Assert.AreEqual(controllerColliderHit.moveLength, moveLength);
        }

        [Test]
        public void Validate_ControllerColliderHitWrapper()
        {
            Type colliderHitType = typeof(ControllerColliderHit);
            ConstructorInfo colliderHitConstructor = colliderHitType.GetConstructors().First(c => c.GetParameters().Length == 0);
            var colliderHit = (ControllerColliderHit)colliderHitConstructor.Invoke(new object[0]);

            GameObject go = CreateGameObject();
            Collider collider = go.AddComponent<BoxCollider>();
            Rigidbody rigidbody = go.AddComponent<Rigidbody>();

            FieldInfo colliderField = colliderHitType.GetRuntimeFields()
                .Union(colliderHitType.GetFields())
                .First(field => field.FieldType == typeof(Collider));
            colliderField.SetValue(colliderHit, collider);

            Vector3 moveVector = Vector3.forward;
            IControllerColliderHit controllerColliderHit = new ControllerColliderHitWrapper(
                colliderHit, moveVector);

            Assert.AreEqual(collider, controllerColliderHit.collider);
            Assert.AreEqual(collider.attachedRigidbody, controllerColliderHit.rigidbody);
            Assert.AreEqual(go, controllerColliderHit.gameObject);
            Assert.AreEqual(go.transform, controllerColliderHit.transform);
            Assert.AreEqual(Vector3.zero, controllerColliderHit.point);
            Assert.AreEqual(Vector3.zero, controllerColliderHit.normal);
            Assert.AreEqual(moveVector.normalized, controllerColliderHit.moveDirection);
            Assert.AreEqual(moveVector.magnitude, controllerColliderHit.moveLength);
        }
    }
}
