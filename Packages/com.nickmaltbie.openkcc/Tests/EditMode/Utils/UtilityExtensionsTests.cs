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

using System;
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    public class TestClass
    {
        public int intField;
        public int IntProperty => intField;
        public int IntMethod()
        {
            return intField;
        }
    }

    [TestFixture]
    public class UtilityExtensionsTests
    {
        [Test]
        public void Validate_UtilityExtensionGetMember()
        {
            var test = new TestClass();

            for (int i = 0; i < 100; i++)
            {
                test.intField = i;
                Assert.AreEqual(i, test.EvaluateMember(nameof(TestClass.intField)));
                Assert.AreEqual(i, test.EvaluateMember(nameof(TestClass.IntProperty)));
                Assert.Throws<NotImplementedException>(() => test.EvaluateMember(nameof(TestClass.IntMethod)));
            }
        }
    }
}
