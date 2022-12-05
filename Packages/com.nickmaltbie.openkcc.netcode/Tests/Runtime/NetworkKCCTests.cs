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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nickmaltbie.openkcc.Tests.netcode.TestCommon;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.OpenKCC.netcode;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;
using static nickmaltbie.OpenKCC.netcode.NetworkKCC;

namespace nickmaltbie.openkcc.Tests.netcode.Runtime
{
    public class MovingGroundConveyer : MonoBehaviour, IMovingGround
    {
        public Vector3 push = Vector3.forward;

        public bool AvoidTransferMomentum()
        {
            return false;
        }

        public Vector3 GetDisplacementAtPoint(Vector3 point)
        {
            return push * Time.fixedDeltaTime;
        }

        public float GetMovementWeight(Vector3 point, Vector3 playerVelocity)
        {
            return 1.0f;
        }

        public float GetTransferMomentumWeight(Vector3 point, Vector3 playerVelocity)
        {
            return 1.0f;
        }

        public Vector3 GetVelocityAtPoint(Vector3 point)
        {
            return push;
        }

        public bool ShouldAttach()
        {
            return false;
        }
    }

    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }

    /// <summary>
    /// Test setup for configuring a testable network KCC
    /// </summary>
    [RequireComponent(typeof(NetworkKCC))]
    public class TestableNetworkKCC : NetworkBehaviour
    {
        public const string AnimControllerPath = "Packages/com.nickmaltbie.openkcc/Common/Animations/CharacterAnimationController.controller";

        public static Dictionary<(int, int), TestableNetworkKCC> Objects = new();
        public static int CurrentlySpawning = 0;
        public IUnityService unityService;

        public InputAction moveInputAction;
        public InputAction jumpInputAction;
        public InputAction sprintInputAction;
        public InputActionAsset inputActionAsset;
        public JumpAction jumpAction;
        public KCCGroundedState kccGroundedState;
        public ParentConstraint constraint;

        public StickControl moveStick;
        public ButtonControl jumpButton;
        public ButtonControl sprintButton;

        public void Start()
        {
            // Get the components to update and modify.
            Animator anim = GetComponent<Animator>();
            NetworkKCC networkKCC = GetComponent<NetworkKCC>();
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            GetComponent<CapsuleCollider>().enabled = false;

            // Setup the basic unity service for the network kcc.
            unityService = new UnityService();
            networkKCC.unityService = unityService;

            // setup capsule collider
            capsuleCollider.center = new Vector3(0, 1, 0);
            capsuleCollider.height = 2.0f;
            capsuleCollider.radius = 0.5f;

            // Setup animation controller.
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath(AnimControllerPath, typeof(AnimatorController)) as AnimatorController;
        }

        public Gamepad SetupInputs()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>($"Gamepad#{OwnerClientId}");
            NetworkKCC networkKCC = GetComponent<NetworkKCC>();

            moveStick = gamepad.leftStick;
            jumpButton = gamepad.aButton;
            sprintButton = gamepad.bButton;

            jumpInputAction = new InputAction("jumpAction", InputActionType.Button, jumpButton.path);
            sprintInputAction = new InputAction("sprintAction", InputActionType.Value, sprintButton.path);
            moveInputAction = new InputAction("moveAction", InputActionType.Value, moveStick.path);

            // Enable inputs
            jumpInputAction.Enable();
            moveInputAction.Enable();
            sprintInputAction.Enable();

            // Configure jump action
            var jumpInput = new BufferedInput();
            jumpInput.InputAction = jumpInputAction;
            jumpInput.cooldown = 1.0f;
            jumpInput.bufferTime = 3.0f;
            jumpAction = new JumpAction();
            jumpAction.jumpInput = jumpInput;

            // Setup actions
            networkKCC.config.jumpAction = jumpAction;
            networkKCC.config.MoveAction = moveInputAction;
            networkKCC.config.SprintAction = sprintInputAction;

            networkKCC.SetupInputs();

            return gamepad;
        }

        public override void OnNetworkSpawn()
        {
            Objects[(CurrentlySpawning, (int)NetworkManager.LocalClientId)] = this;
        }
    }

    /// <summary>
    /// Simple tests meant to be run in PlayMode
    /// </summary>
    [TestFixture]
    public class NetworkKCCTests : NetcodeInputRuntimeTest
    {
        protected override int NumberOfClients => 2;

        private GameObject m_PrefabToSpawn;
        private GameObject floor;

        protected override void OnServerAndClientsCreated()
        {
            m_PrefabToSpawn = CreateNetworkObjectPrefab("TestableNetworkKCC");
            m_PrefabToSpawn.AddComponent<ParentConstraint>();
            m_PrefabToSpawn.AddComponent<CapsuleCollider>();
            m_PrefabToSpawn.AddComponent<CapsuleColliderCast>();
            m_PrefabToSpawn.AddComponent<Animator>();
            m_PrefabToSpawn.AddComponent<NetworkKCC>();
            m_PrefabToSpawn.AddComponent<ClientNetworkTransform>();
            m_PrefabToSpawn.AddComponent<TestableNetworkKCC>();
        }

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            base.OneTimeSetup();
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [UnityTearDown]
        public override IEnumerator UnityTearDown()
        {
            yield return base.UnityTearDown();
            UnityEngine.Object.Destroy(floor);
            TestableNetworkKCC.Objects.Clear();
        }

        [UnitySetUp]
        public override IEnumerator UnitySetUp()
        {
            yield return base.UnitySetUp();

            // Spawn a floor below the players.
            floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.transform.localScale = new Vector3(10, 1, 10);

            // create a player for each character
            for (int objectIndex = 0; objectIndex < 3; objectIndex++)
            {
                TestableNetworkKCC.CurrentlySpawning = objectIndex;

                NetworkManager ownerManager = ServerNetworkManager;
                if (objectIndex != 0)
                {
                    ownerManager = ClientNetworkManagers[objectIndex - 1];
                }

                SpawnObject(m_PrefabToSpawn, ownerManager);

                // wait for each object to spawn on each client
                for (int clientIndex = 0; clientIndex <= NumberOfClients; clientIndex++)
                {
                    while (!TestableNetworkKCC.Objects.ContainsKey((objectIndex, clientIndex)))
                    {
                        yield return new WaitForSeconds(0.0f);
                    }

                    if (clientIndex == objectIndex)
                    {
                        Vector3 pos = Vector3.right * clientIndex * 2 + Vector3.up * 0.0025f;
                        (int, int) index = (clientIndex, objectIndex);
                        TestableNetworkKCC.Objects[index].transform.position = pos;
                    }
                }
            }
        }

        public bool ForAllPlayers(Func<NetworkKCC, bool> verify)
        {
            return Enumerable.Range(0, NumberOfClients + 1).All(i => ForAllPlayers(i, verify));
        }

        public bool ForAllPlayers(int index, Func<NetworkKCC, bool> verify)
        {
            return Enumerable.Range(0, NumberOfClients + 1).All(i =>
                TestableNetworkKCC.Objects.ContainsKey((index, i)) &&
                verify(TestableNetworkKCC.Objects[(index, i)].GetComponent<NetworkKCC>()));
        }

        [UnityTest]
        public IEnumerator Validate_NetworkKCC_Move_Transition()
        {
            for (int i = 0; i <= NumberOfClients; i++)
            {
                Debug.Log($"Validating client #{i}");
                TestableNetworkKCC demo = TestableNetworkKCC.Objects[(i, i)];
                NetworkKCC networkKCC = demo.GetComponent<NetworkKCC>();
                demo.SetupInputs();

                // Move to a position depending on index
                demo.transform.position = Vector3.right * i * 2 + Vector3.up * 0.0025f;

                // Wait for player to fall to ground
                yield return TestUtils.WaitUntil(() => ForAllPlayers(i, player => typeof(IdleState) == player.CurrentState));
                input.Set(demo.moveStick, Vector2.up);
                yield return TestUtils.WaitUntil(() => ForAllPlayers(i, player => typeof(WalkingState) == player.CurrentState));
                input.Set(demo.sprintButton, 1.0f);
                yield return TestUtils.WaitUntil(() => ForAllPlayers(i, player => typeof(SprintingState) == player.CurrentState));
                input.Set(demo.moveStick, Vector2.zero);
                yield return TestUtils.WaitUntil(() => ForAllPlayers(i, player => typeof(IdleState) == player.CurrentState));
            }
        }

        [UnityTest]
        public IEnumerator Validate_NetworkKCC_MovingGround()
        {
            floor.transform.localScale = Vector3.one;

            for (int i = 0; i <= NumberOfClients; i++)
            {
                TestableNetworkKCC demo = TestableNetworkKCC.Objects[(i, i)];
                demo.SetupInputs();
            }
            // Wait until all players are standing on the ground.
            yield return TestUtils.WaitUntil(() => ForAllPlayers(player => typeof(IdleState) == player.CurrentState));

            // Attach a moving ground conveyer to the ground
            MovingGroundConveyer conveyer = floor.AddComponent<MovingGroundConveyer>();

            // Assert that the players are pushed forward in some direction of the moving
            // ground push by one unit. First save their initial positions
            var initialPos = Enumerable.Range(0, NumberOfClients + 1)
                .Select(i => TestableNetworkKCC.Objects[(i, i)].transform.position)
                .ToList();

            // Wait until the delta between the player and their current position is at least
            // one unit in the push direction.
            yield return TestUtils.WaitUntil(() => ForAllPlayers(player =>
            {
                Vector3 initial = initialPos[(int)player.OwnerClientId];
                Vector3 delta = player.transform.position - initial;

                float alongLine = Vector3.Dot(Vector3.Project(delta, conveyer.push), conveyer.push.normalized);
                return alongLine >= 1.0f;
            }));

            // Have the players jump and assert they retain soem forward momentum
            for (int i = 0; i <= NumberOfClients; i++)
            {
                TestableNetworkKCC demo = TestableNetworkKCC.Objects[(i, i)];
                input.Set(demo.jumpButton, 1.0f);
            }

            yield return TestUtils.WaitUntil(() =>
                Enumerable.Range(0, NumberOfClients + 1).All(
                    i =>
                    {
                        NetworkKCC networkKCC = TestableNetworkKCC.Objects[(i, i)].GetComponent<NetworkKCC>();
                        return Vector3.Project(networkKCC.Velocity, conveyer.push.normalized).magnitude >= 1.0f;
                    }));
        }

        [UnityTest]
        public IEnumerator Validate_NetworkKCC_Sliding()
        {
            for (int i = 0; i <= NumberOfClients; i++)
            {
                TestableNetworkKCC demo = TestableNetworkKCC.Objects[(i, i)];
                demo.transform.position = Vector3.right * i * 2 + Vector3.up * 0.0025f;
            }

            yield return TestUtils.WaitUntil(() => ForAllPlayers(player => typeof(IdleState) == player.CurrentState));

            floor.transform.rotation = Quaternion.Euler(61, 0, 0);
            floor.transform.position += Vector3.back * 10 + Vector3.down * 5;

            // Teleport players back to their original positions
            for (int i = 0; i <= NumberOfClients; i++)
            {
                TestableNetworkKCC demo = TestableNetworkKCC.Objects[(i, i)];
                demo.GetComponent<NetworkKCC>().TeleportPlayer(Vector3.right * i * 2 + Vector3.up * 0.0025f);
            }

            yield return TestUtils.WaitUntil(() => ForAllPlayers(player => typeof(SlidingState) == player.CurrentState));
        }

        [UnityTest]
        public IEnumerator Validate_NetworkKCC_Jump_Transition()
        {
            for (int i = 0; i <= NumberOfClients; i++)
            {
                Debug.Log($"Validating client #{i}");
                TestableNetworkKCC demo = TestableNetworkKCC.Objects[(i, i)];
                NetworkKCC networkKCC = demo.GetComponent<NetworkKCC>();
                demo.SetupInputs();

                // Move to a position depending on index
                demo.transform.position = Vector3.right * i * 2 + Vector3.up * 0.0025f;

                // Wait for player to fall to ground
                yield return TestUtils.WaitUntil(() => ForAllPlayers(i, player => typeof(IdleState) == player.CurrentState));
                input.Set(demo.jumpButton, 1.0f);
                yield return TestUtils.WaitUntil(() => ForAllPlayers(i, player => typeof(JumpState) == player.CurrentState));
            }
        }
    }
}
