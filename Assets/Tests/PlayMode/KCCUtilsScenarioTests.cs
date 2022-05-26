﻿// Copyright (C) 2022 Nicholas Maltbie
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static nickmaltbie.OpenKCC.TestCommon.TestUtils;

namespace nickmaltbie.OpenKCC.Tests.PlayMode
{
    /// <summary>
    /// Test basic movement scenarios for KCC Scenarios.
    /// </summary>
    public class KCCUtilsScenarioTests : TestBase
    {
        /// <summary>
        /// Position fo the player.
        /// </summary>
        public Transform playerPosition;

        /// <summary>
        /// Collider cast associated with the player.
        /// </summary>
        public IColliderCast playerColliderCast;

        /// <summary>
        /// Character push associated with the player.
        /// </summary>
        public ICharacterPush characterPush;

        /// <summary>
        /// Setup and create game objects for the test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            GameObject character = this.CreateGameObject();
            this.playerPosition = character.transform;

            CapsuleCollider collider = character.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, 1, 0);
            collider.radius = 0.5f;
            collider.height = 2.0f;

            this.playerColliderCast = character.AddComponent<CapsuleColliderCast>();
            this.characterPush = character.AddComponent<CharacterPush>();

            var capsuleMesh = CapsuleMaker.CapsuleData(depth: 1.0f);
            capsuleMesh.vertices = capsuleMesh.vertices.Select(vert => vert + Vector3.up).ToArray();
            var meshFilter = character.AddComponent<MeshFilter>();
            meshFilter.mesh = capsuleMesh;

            var mr = character.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        /// <summary>
        /// Get the bounces for character movement from current position.
        /// </summary>
        /// <param name="movement">movement of teh player</param>
        /// <param name="kccConfig">Configuration for the movement.</param>
        /// <returns>Character bounces for player given current movement.</returns>
        public IEnumerable<KCCBounce> GetBounces(Vector3 movement, IKCCConfig kccConfig = null)
        {
            kccConfig ??= new KCCConfig
            {
                maxBounces = 5,
                pushDecay = 0.9f,
                verticalSnapUp = 0.3f,
                stepUpDepth = 0.1f,
                anglePower = 1,
                canSnapUp = true,
                up = Vector3.up,
                colliderCast = this.playerColliderCast,
                push = this.characterPush,
            };
            
            return KCCUtils.GetBounces(
                this.playerPosition.position,
                movement,
                this.playerPosition.rotation,
                kccConfig);
        }

        /// <summary>
        /// Generate basic set of movements.
        /// </summary>
        /// <returns>Set of 9 possible movements.</returns>
        public static IEnumerable<Vector3> MovementGenerator()
        {
            return new[]
            {
                Vector3.right,
                Vector3.left,
                Vector3.back,
                Vector3.forward,
                Vector3.up,
                Vector3.down,
            };
        }

        /// <summary>
        /// Basic test of player walking forward.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(MovementGenerator))]
        public void TestWalkForward(Vector3 movement)
        {
            // Get the bounces from moving forward.
            List<KCCBounce> bounces = GetBounces(movement).ToList();

            // Assert that there are two bounces, one from moving forward and one from stepping down.
            Assert.IsTrue(bounces.Count == 2, $"Expected to find 2 bounces, but instead found {bounces.Count}");

            // Validate bounce states
            KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Move, movement, playerPosition.position, Vector3.zero, movement);
            KCCValidation.ValidateKCCBounce(bounces[1], KCCUtils.MovementAction.Stop);
        }

        /// <summary>
        /// Basic test of player walking into an object.
        /// </summary>
        [UnityTest]
        public IEnumerator TestWalkIntoWall([NUnit.Framework.Range(5, 20, 5)] float distance)
        {
            // Setup object to walk into
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = Vector3.forward * (distance - 2) + Vector3.up * 0.5f;
            RegisterGameObject(wall);
            yield return new WaitForFixedUpdate();

            // Have character walk into wall
            List<KCCBounce> bounces = GetBounces(Vector3.forward * distance).ToList();

            // Validate bounce actions
            Assert.IsTrue(bounces.Count >= 2, $"Expected to find at least {2} bounces, but instead found {bounces.Count}");
            var lastBounce = bounces[bounces.Count - 1];

            KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Bounce);
            KCCValidation.ValidateKCCBounce(lastBounce, KCCUtils.MovementAction.Stop);

            Debug.Log($"initialPosition: {bounces[0].initialPosition}, finalPosition: {bounces[0].finalPosition}, movement: {bounces[0].Movement}");

            TestUtils.AssertInBounds(bounces[0].Movement, Vector3.forward * (distance - 3), KCCUtils.Epsilon * 5);
            TestUtils.AssertInBounds(lastBounce.finalPosition, bounces[0].finalPosition, KCCUtils.Epsilon * 5);
        }

        /// <summary>
        /// Basic test of player sliding off wall.
        /// </summary>
        [UnityTest]
        public IEnumerator TestSlideOffWall([NUnit.Framework.Range(15, 60, 15)] float yaw, [NUnit.Framework.Range(15, 60, 15)] float pitch)
        {
            // Setup object to walk into
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.position = Vector3.forward * 3 + Vector3.up * 0.5f;
            wall.transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            RegisterGameObject(wall);
            yield return new WaitForFixedUpdate();

            // Have character walk into wall
            Vector3 movement = Vector3.forward * 5;
            List<KCCBounce> bounces = GetBounces(movement).ToList();

            // Validate bounce actions
            Assert.IsTrue(bounces.Count == 3, $"Expected to find {3} bounces, but instead found {bounces.Count}");
            KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Bounce);
            KCCValidation.ValidateKCCBounce(bounces[1], KCCUtils.MovementAction.Move);
            KCCValidation.ValidateKCCBounce(bounces[2], KCCUtils.MovementAction.Stop);

            TestUtils.AssertInBounds(bounces[0].Movement, Vector3.forward * 2, 1.0f);
            TestUtils.AssertInBounds(bounces[1].Movement.magnitude, 0.5f, movement.magnitude - bounces[0].Movement.magnitude, bound:BoundRange.GraterThan);
        }
    }
}
