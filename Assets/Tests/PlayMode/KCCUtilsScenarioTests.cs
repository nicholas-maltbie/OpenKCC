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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
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
        /// Create a KCCConfig with default parameters.
        /// </summary>
        /// <value></value>
        public KCCConfig CreateKCCConfig() => new KCCConfig
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
            kccConfig ??= CreateKCCConfig();
            
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
        /// Test that the player will not move when overlapping with objects.
        /// </summary>
        /// <returns>Enumerator of unity events for the test.</returns>
        [UnityTest]
        public IEnumerator PushOutOverlapping()
        {
            // Setup object to overlap with
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            RegisterGameObject(wall);

            // Setup random number generator
            UnityEngine.Random.InitState(42);

            for (int i = 0; i < 30; i++)
            {
                Vector3 offset = UnityEngine.Random.insideUnitCircle * 10;

                // Randomize the cube position and rotation then update state
                wall.transform.position = UnityEngine.Random.insideUnitSphere * 0.75f + Vector3.up + offset;
                wall.transform.rotation = UnityEngine.Random.rotation;
                playerPosition.transform.position = offset;
                yield return new WaitForFixedUpdate();

                // have player attempt to move, they should overlap with object and exit early
                var bounces = GetBounces(Vector3.forward).ToList();
                Assert.IsTrue(bounces.Count == 2, $"Expected to find 2 bounces, but instead found {bounces.Count}");
                KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Invalid, initialPosition:playerPosition.position, finalPosition:playerPosition.position, remainingMomentum:Vector3.zero, initialMomentum:Vector3.forward, log:false);
                KCCValidation.ValidateKCCBounce(bounces[1], KCCUtils.MovementAction.Stop, initialPosition:playerPosition.position, finalPosition:playerPosition.position, remainingMomentum:Vector3.zero, log:false);
            }
        }

        [UnityTest]
        public IEnumerator TestWalkUpStairs(
            [NUnit.Framework.Range(0.1f, 0.65f, 0.25f)] float stepHeight,
            [Values(2)] float width,
            [NUnit.Framework.Range(5, 10, 5)] int numSteps,
            [NUnit.Framework.Range(0.15f, 0.55f, 0.2f)] float stepDepth,
            [Values(new float[]{0.125f, 0.325f, 0.1f})] float[] snapHeightRange,
            [Values(new float[]{0.25f, 0.75f, 0.25f})] float[]  snapDepthRange)
        {
            // Create stair object for player to walk into
            // Position stairs 1 units ahead of the player and centered
            Vector3 size = new Vector3(width, stepHeight * numSteps, stepDepth * numSteps);
            var stairBuilder = ShapeGenerator.GenerateStair(PivotLocation.FirstCorner, size, numSteps, false);
            stairBuilder.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            stairBuilder.transform.position = Vector3.forward + Vector3.left * width / 2;
            stairBuilder.gameObject.AddComponent<MeshCollider>();
            RegisterGameObject(stairBuilder.gameObject);

            // Put a small platform after the stairs
            var platform = ShapeGenerator.GeneratePlane(PivotLocation.FirstCorner, 5, width, 1, 1, Axis.Up);
            platform.transform.position = stairBuilder.transform.position + new Vector3(0, size.y, size.z);
            platform.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            platform.gameObject.AddComponent<MeshCollider>();
            RegisterGameObject(platform.gameObject);

            yield return new WaitForFixedUpdate();

            // Run a test for each permutation of snap height and snap depth
            List<(float, float)> permutations = new List<(float, float)>();
            for (float snapHeight = snapHeightRange[0]; snapHeight <= snapHeightRange[1]; snapHeight += snapHeightRange[2])
            {
                for (float snapDepth = snapDepthRange[0]; snapDepth <= snapDepthRange[1]; snapDepth += snapDepthRange[2])
                {
                    permutations.Add((snapHeight, snapDepth));
                }
            }

            foreach ((float snapHeight, float snapDepth) in permutations)
            {
                // Player should bounce forward and have at least first step be a bounce up
                KCCConfig config = CreateKCCConfig();
                config.MaxBounces = Mathf.Min(5);
                config.stepUpDepth = snapDepth;
                config.verticalSnapUp = snapHeight;
                config.canSnapUp = true;

                // Reset player position
                playerPosition.position = Vector3.zero;

                // Check if the player can climb steps, step height should be <= snap up stair step depth should be >= snap depth
                // Edge case, player can walk up steps if player can take multiple steps in one snap
                int maxStepsAtOnce = (int) Mathf.Floor(snapHeight / stepHeight);
                float maxStepsDepth = maxStepsAtOnce * stepDepth;
                bool canClimb = stepHeight <= config.verticalSnapUp && (stepDepth >= config.stepUpDepth || maxStepsDepth >= config.stepUpDepth);

                Debug.Log($"Computed canClimb:{canClimb} = stepHeight:{stepHeight} <= config.verticalSnapUp:{config.verticalSnapUp} && stepDepth:{stepDepth} >= config.stepUpDepth:{config.stepUpDepth}");

                // Have player move forward until they hit the top of the steps
                Func<bool> climbedSteps = () => canClimb &&
                    playerPosition.transform.position.z >= stairBuilder.transform.position.z + size.z &&
                    playerPosition.transform.position.y >= stairBuilder.transform.position.y + size.y;

                // Move forward 3 units
                Vector3 movement = Vector3.forward * 3;

                // If the player cannot climb up, check to make sure they bounce then stop
                if (!canClimb)
                {
                    var bounces = GetBounces(movement, config).ToList();
                    playerPosition.transform.position = bounces[bounces.Count - 1].finalPosition;
                    yield return null;

                    Debug.Log(string.Join("\n", bounces));

                    // Validate bounce actions
                    Assert.IsTrue(bounces.Count >= 2, $"Expected to find at least {2} bounces, but instead found {bounces.Count}");
                    var lastBounce = bounces[bounces.Count - 1];

                    // Assert that no bounces were snap up
                    bounces.ForEach(bounce => Assert.IsTrue(bounce.action != KCCUtils.MovementAction.SnapUp, $"Found unexpected {KCCUtils.MovementAction.SnapUp} while player should not able to walk up stairs"));

                    KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Bounce);
                    KCCValidation.ValidateKCCBounce(lastBounce, KCCUtils.MovementAction.Stop);
                }

                // If the player can climb the steps, have them move until they climb up all the steps
                int snapUpCount = 0;
                while (canClimb && !climbedSteps())
                {
                    var bounces = GetBounces(movement, config).ToList();

                    // Move player to final position
                    playerPosition.transform.position = bounces[bounces.Count - 1].finalPosition;
                    yield return null;

                    Debug.Log(string.Join("\n", bounces));

                    // Expected to find at least three bounces
                    Assert.IsTrue(bounces.Count >= 2, $"Expected to find at least three bounces, but instead found {bounces.Count}");

                    // First bounces should all be a "StepUp" action
                    // Last bounce should be a "Stop" action
                    // Some steps may take more than one bounce to scale so will let that happen
                    var stepUpBounces = Enumerable.Range(0, bounces.Count - 2).Select(index => bounces[index]);

                    // Validate the sets of bounces
                    foreach (var stepUp in stepUpBounces)
                    {
                        // Movement should also be forward and step up action
                        Assert.IsTrue(
                            stepUp.action == KCCUtils.MovementAction.SnapUp || stepUp.action == KCCUtils.MovementAction.Bounce,
                            $"Expected to find action {KCCUtils.MovementAction.SnapUp} or {KCCUtils.MovementAction.Bounce} but instead found {stepUp.action}");
                        Assert.IsTrue(Vector3.Dot(stepUp.Movement, Vector3.forward) > 0.0f);
                        if (stepUp.action == KCCUtils.MovementAction.SnapUp)
                        {
                            snapUpCount++;
                        }
                    }

                    // If we have finished climbing the steps, validate move and stop bounces
                    if (climbedSteps())
                    {
                        var moveBounce = bounces[bounces.Count - 2];
                        var stopBounce = bounces[bounces.Count - 1];
                        // Last steps should be stop
                        // Last step should have climbed to top of staircase
                        KCCValidation.ValidateKCCBounce(moveBounce, KCCUtils.MovementAction.Move);
                        KCCValidation.ValidateKCCBounce(stopBounce, KCCUtils.MovementAction.Stop);
                        TestUtils.AssertInBounds(stopBounce.finalPosition.y, size.y, stepHeight);
                        TestUtils.AssertInBounds(stopBounce.finalPosition.z, size.z, 6, bound: BoundRange.GraterThan);

                        Assert.IsTrue(climbedSteps());
                        Assert.IsTrue(snapUpCount > 0, $"Expected player to snap up but did not find any snap up events.");
                    }
                } 
            }
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
