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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Tests for <see cref="nickmaltbie.OpenKCC.Character.KCCStateMachine"/> in edit mode
    /// to verify serializing from previous versions to current version.
    /// </summary>
    [TestFixture]
    public class KCCStateMachineSerializationTests : KCCStateMachineTestBase
    {
        [Test]
        public void Verify_DefaultOrUnknownSerialization_ToCurrentVersion()
        {
            KCCStateMachine kcc = CreateGameObject().AddComponent<KCCStateMachine>();
            kcc.OnBeforeSerialize();

            // Assume we had some previous configuration
            // before versioning started
#pragma warning disable CS0618 // Type or member is obsolete
            kcc.moveAction = InputActionReference.Create(base.moveInputAction);
            kcc.sprintAction = InputActionReference.Create(base.sprintInputAction);
            kcc.jumpAction = jumpAction;
            kcc.groundedState = base.kccGroundedState;
            kcc.gravity = Vector3.down * 9.801f;
            kcc.walkingSpeed = 5.0f;
            kcc.sprintSpeed = 10.0f;
            kcc.maxBounces = 8;
            kcc.pushDecay = 0.8f;
            kcc.anglePower = 0.2f;
            kcc.maxPushSpeed = 100;
            kcc.verticalSnapDown = 0.45f;
            kcc.stepUpDepth = 0.25f;
            kcc.verticalSnapUp = 0.25f;
            kcc.snapBufferTime = 0.25f;
            kcc.maxDefaultLaunchVelocity = 1000;
#pragma warning restore CS0618 // Type or member is obsolete

            kcc.serializationVersion = "";

            // Assert that the kcc's current config is the default
            Assert.IsTrue(new HumanoidKCCConfig().Equals(kcc.config));

            // After de-serializing, assert that the fields are
            // restored properly.
            kcc.OnAfterDeserialize();
            Assert.IsFalse(new HumanoidKCCConfig().Equals(kcc.config));

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.AreEqual(kcc.moveAction, kcc.config.moveActionReference);
            Assert.AreEqual(kcc.sprintAction, kcc.config.sprintActionReference);
            Assert.AreEqual(kcc.jumpAction, kcc.config.jumpAction);
            Assert.AreEqual(kcc.groundedState, kcc.config.groundedState);
            Assert.AreEqual(kcc.gravity, kcc.config.gravity);
            Assert.AreEqual(kcc.walkingSpeed, kcc.config.walkingSpeed);
            Assert.AreEqual(kcc.sprintSpeed, kcc.config.sprintSpeed);
            Assert.AreEqual(kcc.maxBounces, kcc.config.maxBounces);
            Assert.AreEqual(kcc.pushDecay, kcc.config.pushDecay);
            Assert.AreEqual(kcc.anglePower, kcc.config.anglePower);
            Assert.AreEqual(kcc.maxPushSpeed, kcc.config.maxPushSpeed);
            Assert.AreEqual(kcc.verticalSnapDown, kcc.config.verticalSnapDown);
            Assert.AreEqual(kcc.stepUpDepth, kcc.config.stepUpDepth);
            Assert.AreEqual(kcc.verticalSnapUp, kcc.config.verticalSnapUp);
            Assert.AreEqual(kcc.snapBufferTime, kcc.config.snapBufferTime);
            Assert.AreEqual(kcc.maxDefaultLaunchVelocity, kcc.config.maxDefaultLaunchVelocity);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
