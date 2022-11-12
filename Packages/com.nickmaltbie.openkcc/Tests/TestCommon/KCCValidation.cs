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

using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.TestCommon
{
    /// <summary>
    /// Utility functions for KCC Validation
    /// </summary>
    public static class KCCValidation
    {
        /// <summary>
        /// Validate a KCC bounce for a specified set of properties.
        /// </summary>
        /// <param name="bounce">Bounce to validate.</param>
        /// <param name="movementAction">Expected movement action, or null if not required to check.</param>
        /// <param name="finalPosition">Expected final position, or null if not required to check.</param>
        /// <param name="initialPosition">Expected initial position, or null if not required to check.</param>
        /// <param name="remainingMomentum">Expected remaining momentum, or null if not required to check.</param>
        /// <param name="initialMomentum">Expected initial momentum, or null if not required to check.</param>
        /// <param name="log">Should the comparison be logged.</param>
        public static void ValidateKCCBounce(
            KCCBounce bounce,
            KCCUtils.MovementAction? movementAction = null,
            Vector3? finalPosition = null,
            Vector3? initialPosition = null,
            Vector3? remainingMomentum = null,
            Vector3? initialMomentum = null,
            bool log = true)
        {
            if (log)
            {
                Debug.Log($"Evaluating bounce {bounce} for properties finalPosition:{finalPosition}, initialPosition:{initialPosition}, remainingMomentum:{remainingMomentum}, initialMomentum:{initialMomentum}");
            }

            Assert.IsTrue(movementAction == null || bounce.action == movementAction, $"Expected {nameof(bounce.action)} to be {movementAction} but instead found {bounce.action}");
            Assert.IsTrue(finalPosition == null || bounce.finalPosition == finalPosition, $"Expected {nameof(bounce.finalPosition)} to be {finalPosition} but instead found {bounce.finalPosition}");
            Assert.IsTrue(initialPosition == null || bounce.initialPosition == initialPosition, $"Expected {nameof(bounce.initialPosition)} to be {initialPosition} but instead found {bounce.initialPosition}");
            Assert.IsTrue(remainingMomentum == null || bounce.remainingMomentum == remainingMomentum, $"Expected {nameof(bounce.remainingMomentum)} to be {remainingMomentum} but instead found {bounce.remainingMomentum}");
            Assert.IsTrue(initialMomentum == null || bounce.initialMomentum == initialMomentum, $"Expected {nameof(bounce.initialMomentum)} to be {initialMomentum} but instead found {bounce.initialMomentum}");
        }
    }
}
