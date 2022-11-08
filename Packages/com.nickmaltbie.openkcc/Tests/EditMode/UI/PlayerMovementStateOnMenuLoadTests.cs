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
using nickmaltbie.OpenKCC.UI.Events;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.UI
{
    /// <summary>
    /// Basic tests for PlayerMovementStateOnMenuLoad in edit mode.
    /// </summary>
    [TestFixture]
    public class PlayerMovementStateOnMenuLoadTests : TestBase
    {
        [Test]
        public void Verify_PlayerMovementStateOnMenuLoad()
        {
            PlayerMovementStateOnMenuLoad playerMovement = CreateGameObject().AddComponent<PlayerMovementStateOnMenuLoad>();

            PlayerInputUtils.playerMovementState = PlayerInputState.Deny;
            playerMovement.playerInputState = PlayerInputState.Allow;

            Assert.AreEqual(PlayerInputUtils.playerMovementState, PlayerInputState.Deny);

            playerMovement.OnScreenLoaded();
            Assert.AreEqual(PlayerInputUtils.playerMovementState, PlayerInputState.Allow);

            playerMovement.OnScreenUnloaded();
            Assert.AreEqual(PlayerInputUtils.playerMovementState, PlayerInputState.Allow);

            playerMovement.playerInputState = PlayerInputState.Deny;
            playerMovement.OnScreenLoaded();
            Assert.AreEqual(PlayerInputUtils.playerMovementState, PlayerInputState.Deny);
        }
    }
}
