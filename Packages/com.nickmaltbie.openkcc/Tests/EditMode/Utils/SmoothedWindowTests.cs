﻿// Copyright (C) 2023 Nicholas Maltbie
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

using System.Linq;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    /// <summary>
    /// Basic EditMode tests for the <see cref="SmoothedWindow{E}"/> class.
    /// </summary>
    [TestFixture]
    public class SmoothedWindowTests : TestBase
    {
        [Test]
        public void Validate_SmoothedWindowVector3()
        {
            var vector = new SmoothedVector(4);

            Assert.AreEqual(Vector3.zero, vector.Average());

            vector.AddSample(Vector3.up);
            Assert.AreEqual(Vector3.up, vector.Average());
            vector.AddSample(Vector3.up);
            Assert.AreEqual(Vector3.up, vector.Average());

            vector.AddSample(Vector3.down);
            vector.AddSample(Vector3.down);
            Assert.AreEqual(Vector3.zero, vector.Average());

            vector.AddSample(Vector3.down);
            vector.AddSample(Vector3.down);
            Assert.AreEqual(Vector3.down, vector.Average());
        }

        [Test]
        public void Validate_SmoothedWindow_Samples([NUnit.Framework.Range(0, 100, 10)] int size)
        {
            var smoothed = new SmoothedWindow<int>(size);

            for (int i = 0; i < size; i++)
            {
                smoothed.AddSample(i);

                Assert.AreEqual(i + 1, smoothed.Count);
                Assert.IsTrue(smoothed.Values.SequenceEqual(Enumerable.Range(0, i + 1)));
            }

            for (int i = 0; i < size / 2; i++)
            {
                smoothed.AddSample(size + i);
                Assert.AreEqual(size, smoothed.Count);
                Assert.IsTrue(smoothed.Values.SequenceEqual(Enumerable.Range(i + 1, size)));
            }
        }
    }
}
