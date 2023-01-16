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

using Moq;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.TestCommon
{
    /// <summary>
    /// Unity test utilities.
    /// </summary>
    public static class KCCTestUtils
    {
        /// <summary>
        /// Bound range for test utils validation of number in range.
        /// </summary>
        public enum BoundRange
        {
            LessThan,
            GraterThan,
            GraterThanOrLessThan,
        }

        /// <summary>
        /// Setup a raycast hit mock.
        /// </summary>
        /// <param name="collider">Collider to return from the mock.</param>
        /// <param name="point">Point of collision for the mock.</param>
        /// <param name="distance">Distance from source from the mock.</param>
        /// <param name="normal">Normal vector for the collision from the mock.</param>
        /// <param name="fraction">Fraction of movement.</param>
        /// <returns>Mock raycast hit object with the specified properties.</returns>
        public static IRaycastHit SetupRaycastHitMock(Collider collider = null, Vector3 point = default, Vector3 normal = default, float distance = 0.0f)
        {
            var raycastHitMock = new Mock<IRaycastHit>();

            raycastHitMock.Setup(hit => hit.collider).Returns(collider);
            raycastHitMock.Setup(hit => hit.point).Returns(point);
            raycastHitMock.Setup(hit => hit.distance).Returns(distance);
            raycastHitMock.Setup(hit => hit.normal).Returns(normal);

            return raycastHitMock.Object;
        }

        public static IRaycastHit SetupCastSelf(Mock<IColliderCast> colliderCastMock, Collider collider = null, Vector3 point = default, Vector3 normal = default, float distance = 0.0f, bool didHit = false)
        {
            IRaycastHit raycastHit = KCCTestUtils.SetupRaycastHitMock(
                collider,
                point,
                normal,
                distance);

            colliderCastMock.Setup(e => e.CastSelf(
                It.IsAny<Vector3>(),
                It.IsAny<Quaternion>(),
                It.IsAny<Vector3>(),
                It.IsAny<float>(),
                out raycastHit
            )).Returns(didHit);

            return raycastHit;
        }
    }
}
