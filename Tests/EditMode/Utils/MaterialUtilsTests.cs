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
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    /// <summary>
    /// Tests for the MaterialsUtils class.
    /// </summary>
    [TestFixture]
    public class MaterialUtilsTests : TestBase
    {
        private const string TestValueFloat = "_Glossiness";
        private const string TestValueColor = "_BaseColor";

        private GameObject baseRendererGO;
        private GameObject childRendererGO;
        private GameObject otherRendererGO;

        private Material baseMat;
        private Material childMat;
        private Material otherMat;

        private Renderer baseRenderer;
        private Renderer childRenderer;
        private Renderer otherRenderer;

        [SetUp]
        public void SetUp()
        {
            baseRendererGO = CreateGameObject();
            childRendererGO = CreateGameObject();
            otherRendererGO = CreateGameObject();

            baseRenderer = baseRendererGO.AddComponent<MeshRenderer>();
            childRenderer = childRendererGO.AddComponent<MeshRenderer>();
            otherRenderer = otherRendererGO.AddComponent<MeshRenderer>();

            childRendererGO.transform.SetParent(baseRendererGO.transform);

            baseRenderer.sharedMaterials = new[] { baseMat = new Material(Shader.Find("Universal Render Pipeline/Lit")) };
            childRenderer.sharedMaterials = new[] { childMat = new Material(Shader.Find("Universal Render Pipeline/Lit")) };
            otherRenderer.sharedMaterials = new[] { otherMat = new Material(Shader.Find("Universal Render Pipeline/Lit")) };
        }

        [Test]
        public void Validate_RecursiveSetShadowCastingMode()
        {
            ShadowCastingMode initialOtherRendering = otherRenderer.shadowCastingMode;

            MaterialUtils.RecursiveSetShadowCastingMode(baseRendererGO, ShadowCastingMode.ShadowsOnly);

            Assert.AreEqual(baseRenderer.shadowCastingMode, ShadowCastingMode.ShadowsOnly);
            Assert.AreEqual(childRenderer.shadowCastingMode, ShadowCastingMode.ShadowsOnly);
            Assert.AreEqual(otherRenderer.shadowCastingMode, initialOtherRendering);

            MaterialUtils.RecursiveSetShadowCastingMode(childRendererGO, ShadowCastingMode.On);

            Assert.AreEqual(baseRenderer.shadowCastingMode, ShadowCastingMode.ShadowsOnly);
            Assert.AreEqual(childRenderer.shadowCastingMode, ShadowCastingMode.On);
            Assert.AreEqual(otherRenderer.shadowCastingMode, initialOtherRendering);

            MaterialUtils.RecursiveSetShadowCastingMode(baseRendererGO, ShadowCastingMode.Off);

            Assert.AreEqual(baseRenderer.shadowCastingMode, ShadowCastingMode.Off);
            Assert.AreEqual(childRenderer.shadowCastingMode, ShadowCastingMode.Off);
            Assert.AreEqual(otherRenderer.shadowCastingMode, initialOtherRendering);
        }

        [Test]
        public void Validate_RecursiveSetFloatProperty()
        {
            float initialOtherRendering = otherMat.GetFloat(TestValueFloat);

            MaterialUtils.RecursiveSetFloatProperty(baseRendererGO, TestValueFloat, 0.1f, true);

            TestUtils.AssertInBounds(baseMat.GetFloat(TestValueFloat), 0.1f, 0.001f);
            TestUtils.AssertInBounds(childMat.GetFloat(TestValueFloat), 0.1f, 0.001f);
            TestUtils.AssertInBounds(otherMat.GetFloat(TestValueFloat), initialOtherRendering, 0.001f);

            MaterialUtils.RecursiveSetFloatProperty(childRendererGO, TestValueFloat, 0.2f, true);

            TestUtils.AssertInBounds(baseMat.GetFloat(TestValueFloat), 0.1f, 0.001f);
            TestUtils.AssertInBounds(childMat.GetFloat(TestValueFloat), 0.2f, 0.001f);
            TestUtils.AssertInBounds(otherMat.GetFloat(TestValueFloat), initialOtherRendering, 0.001f);

            MaterialUtils.RecursiveSetFloatProperty(baseRendererGO, TestValueFloat, 0.3f, true);

            TestUtils.AssertInBounds(baseMat.GetFloat(TestValueFloat), 0.3f, 0.001f);
            TestUtils.AssertInBounds(childMat.GetFloat(TestValueFloat), 0.3f, 0.001f);
            TestUtils.AssertInBounds(otherMat.GetFloat(TestValueFloat), initialOtherRendering, 0.001f);
        }

        [Test]
        public void Validate_RecursiveSetColorProperty()
        {
            Color initialOtherRendering = otherMat.GetColor(TestValueColor);

            MaterialUtils.RecursiveSetColorProperty(baseRendererGO, TestValueColor, Color.red, true);

            Assert.AreEqual(baseMat.GetColor(TestValueColor), Color.red);
            Assert.AreEqual(childMat.GetColor(TestValueColor), Color.red);
            Assert.AreEqual(otherMat.GetColor(TestValueColor), initialOtherRendering);

            MaterialUtils.RecursiveSetColorProperty(childRendererGO, TestValueColor, Color.blue, true);

            Assert.AreEqual(baseMat.GetColor(TestValueColor), Color.red);
            Assert.AreEqual(childMat.GetColor(TestValueColor), Color.blue);
            Assert.AreEqual(otherMat.GetColor(TestValueColor), initialOtherRendering);

            MaterialUtils.RecursiveSetColorProperty(baseRendererGO, TestValueColor, Color.green, true);

            Assert.AreEqual(baseMat.GetColor(TestValueColor), Color.green);
            Assert.AreEqual(childMat.GetColor(TestValueColor), Color.green);
            Assert.AreEqual(otherMat.GetColor(TestValueColor), initialOtherRendering);
        }
    }
}
