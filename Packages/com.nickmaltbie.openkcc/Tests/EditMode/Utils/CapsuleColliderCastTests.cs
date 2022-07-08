using nickmaltbie.OpenKCC.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    /// <summary>
    /// Basic tests for CapsuleColliderCast in edit mode.
    /// </summary>
    [TestFixture]
    public class CapsuleColliderCastTests : TestBase
    {
        private CapsuleColliderCast colliderCast;
        
        private CapsuleCollider collider;

        [SetUp]
        public void SetUp()
        {
            GameObject go = CreateGameObject();
            collider = go.AddComponent<CapsuleCollider>();
            collider.radius = 0.5f;
            collider.center = Vector3.zero;
            collider.height = 2;
            colliderCast = go.AddComponent<CapsuleColliderCast>();
        }

        [Test]
        public void Validate_GetParams()
        {
            (Vector3 top, Vector3 bottom, float radius, float height) = colliderCast.GetParams(Vector3.zero, Quaternion.Euler(Vector3.zero));

            Assert.AreEqual(new Vector3(0, 1 - collider.radius, 0), top);
            Assert.AreEqual(new Vector3(0, -1 + collider.radius, 0), bottom);
            Assert.AreEqual(collider.radius, radius);
            Assert.AreEqual(collider.height, height);
        }
    }
}
