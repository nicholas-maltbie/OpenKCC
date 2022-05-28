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

using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.TestCommon
{
    /// <summary>
    /// Unity test utilities.
    /// </summary>
    public static class TestUtils
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
        /// Assert that a float value is within a specific bound.
        /// </summary>
        /// <param name="actual">Actual value.</param>
        /// <param name="expected">Expected value.</param>
        /// <param name="range">Acceptable range of error.</param>
        /// <param name="errorMsg">Error message to log if failure, one will be generated if none is provided.</param>
        /// <param name="bound">Allow the actual value to be lesser, grater, or either.</param>
        public static void AssertInBounds(float actual, float expected, float range, string errorMsg = null, BoundRange bound = BoundRange.GraterThanOrLessThan)
        {
            float delta = Mathf.Abs(expected - actual);

            switch (bound)
            {
                case BoundRange.LessThan:
                    Assert.IsTrue(delta <= range && actual <= expected, errorMsg ?? $"Actual value {actual} is not less than expected value {expected} within range {range}");
                    break;
                case BoundRange.GraterThan:
                    Assert.IsTrue(delta <= range && actual >= expected, errorMsg ?? $"Actual value {actual} is not grater than expected value {expected} within range {range}");
                    break;
                case BoundRange.GraterThanOrLessThan:
                    Assert.IsTrue(delta <= range, errorMsg ?? $"Actual value {actual} is not close enough to expected value {expected} within range {range}");
                    break;
                default:
                    Assert.Fail($"Found invalid bound:{bound} for {nameof(AssertInBounds)}");
                    break;
            }
        }

        /// <summary>
        /// Assert that two vectors are within a specific bound of each other.
        /// </summary>
        /// <param name="actual">Actual vector found.</param>
        /// <param name="expected">Expected vector to find.</param>
        /// <param name="range">Range in units of acceptable error.</param>
        /// <param name="errorMsg">Error message to log if failure, one will be generated if none is provided.</param>
        /// <param name="bound">Allow the actual vector to be shorter, longer, or either.</param>
        public static void AssertInBounds(Vector3 actual, Vector3 expected, float range, string errorMsg = null, BoundRange bound = BoundRange.GraterThanOrLessThan)
        {
            float delta = (expected - actual).magnitude;

            switch (bound)
            {
                case BoundRange.LessThan:
                    Assert.IsTrue(delta <= range && actual.magnitude <= expected.magnitude, errorMsg ?? $"Actual value {actual.ToString("F3")} is not less than expected value {expected.ToString("F3")} within range {range}");
                    break;
                case BoundRange.GraterThan:
                    Assert.IsTrue(delta <= range && actual.magnitude >= expected.magnitude, errorMsg ?? $"Actual value {actual.ToString("F3")} is not grater than expected value {expected.ToString("F3")} within range {range}");
                    break;
                case BoundRange.GraterThanOrLessThan:
                    Assert.IsTrue(delta <= range, errorMsg ?? $"Actual value {actual.ToString("F3")} is not close enough to expected value {expected.ToString("F3")} within range {range}, actual delta: {delta}");
                    break;
                default:
                    Assert.Fail($"Found invalid bound:{bound} for {nameof(AssertInBounds)}");
                    break;
            }
        }
    }
}
