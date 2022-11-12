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
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Environment.Pushable;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Character.CharacterPush"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class CharacterPushTests : TestBase
    {
        private CharacterPush character;
        private GameObject pushable;

        [SetUp]
        public override void Setup()
        {
            character = CreateGameObject().AddComponent<CharacterPush>();

            pushable = CreateGameObject();
            pushable.AddComponent<BoxCollider>();
            pushable.AddComponent<Rigidbody>();
            pushable.AddComponent<Pushable>().Awake();
        }

        [Test]
        public void Verify_CharacterPush_CanPushObject()
        {
            GameObject noRigidbody = CreateGameObject();
            GameObject kinematic = CreateGameObject();
            GameObject noPushable = CreateGameObject();

            kinematic.AddComponent<Rigidbody>().isKinematic = true;
            noPushable.AddComponent<Rigidbody>();

            Assert.IsFalse(character.CanPushObject(noRigidbody.AddComponent<BoxCollider>()));
            Assert.IsFalse(character.CanPushObject(kinematic.AddComponent<BoxCollider>()));
            Assert.IsFalse(character.CanPushObject(noPushable.AddComponent<BoxCollider>()));
            Assert.IsTrue(character.CanPushObject(pushable.GetComponent<BoxCollider>()));
        }

        [Test]
        public void Verify_CharacterPush_NonPushable_PushObject()
        {
            GameObject noRigidbody = CreateGameObject();
            GameObject kinematic = CreateGameObject();
            GameObject noPushable = CreateGameObject();

            kinematic.AddComponent<Rigidbody>().isKinematic = true;
            noPushable.AddComponent<Rigidbody>();

            var hit = new Mock<IControllerColliderHit>();
            hit.Setup(e => e.gameObject).Returns(noRigidbody);
            character.PushObject(hit.Object);

            hit.Setup(e => e.gameObject).Returns(kinematic);
            hit.Setup(e => e.rigidbody).Returns(kinematic.GetComponent<Rigidbody>());
            character.PushObject(hit.Object);

            hit.Setup(e => e.gameObject).Returns(noPushable);
            hit.Setup(e => e.rigidbody).Returns(noPushable.GetComponent<Rigidbody>());
            character.PushObject(hit.Object);
        }

        [Test]
        public void Verify_CharacterPush_PushObject()
        {
            var hit = new Mock<IControllerColliderHit>();
            hit.Setup(e => e.gameObject).Returns(pushable);
            hit.Setup(e => e.rigidbody).Returns(pushable.GetComponent<Rigidbody>());
            character.PushObject(hit.Object);
        }
    }
}
