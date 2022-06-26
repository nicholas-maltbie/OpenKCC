using nickmaltbie.OpenKCC.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    /// <summary>
    /// Basic tests for CapsuleColliderCast in edit mode.
    /// </summary>
    [TestFixture]
    public class CapsuleColliderCastTests : TestBase
    {
        private CapsuleColliderCastTests colliderCast;

        [SetUp]
        public void SetUp()
        {
            colliderCast = this.CreateGameObject().AddComponent<CapsuleColliderCast>();
        }
    }
}