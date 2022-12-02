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

using System.Collections;
using System.Collections.Generic;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Config;
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

namespace nickmaltbie.openkcc.netcode.Tests.Runtime
{
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

        public static Dictionary<(int, int), TestableNetworkKCC> Objects = new ();
        public static int CurrentlySpawning = 0;
        public IUnityService unityService;

        public StickControl moveStick;
        public ButtonControl jumpButton;
        public ButtonControl sprintButton;

        public Gamepad gamepad;
        public InputActionMap actionMap;
        public InputAction moveInputAction;
        public InputAction jumpInputAction;
        public InputAction sprintInputAction;
        public InputActionAsset inputActionAsset;
        public JumpAction jumpAction;
        public KCCGroundedState kccGroundedState;
        public ParentConstraint constraint;

        public void Awake()
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

            SetupInputs(InputSystem.AddDevice<Gamepad>());
        }

        public void SetupInputs(Gamepad gamepad)
        {
            NetworkKCC networkKCC = GetComponent<NetworkKCC>();
            PlayerInput playerInput = GetComponent<PlayerInput>();

            inputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            actionMap = inputActionAsset.AddActionMap("testMap");
            playerInput.actions = inputActionAsset;
            playerInput.currentActionMap = actionMap;

            jumpButton = gamepad.aButton;
            sprintButton = gamepad.bButton;
            moveStick = gamepad.leftStick;

            jumpInputAction = actionMap.AddAction("jumpAction", InputActionType.Button, jumpButton.path);
            moveInputAction = actionMap.AddAction("moveAction", InputActionType.Value, moveStick.path);
            sprintInputAction = actionMap.AddAction("sprintAction", InputActionType.Value, sprintButton.path);

            // Enable inputs
            jumpInputAction.Enable();
            moveInputAction.Enable();
            sprintInputAction.Enable();

            // Configure jump action
            var jumpInput = new BufferedInput();
            jumpInput.inputAction = InputActionReference.Create(jumpInputAction);
            jumpInput.cooldown = 1.0f;
            jumpInput.bufferTime = 3.0f;
            jumpAction = new JumpAction();
            jumpAction.jumpInput = jumpInput;

            // Setup actions
            networkKCC.config.jumpAction = jumpAction;
            networkKCC.config.moveAction = InputActionReference.Create(moveInputAction);
            networkKCC.config.sprintAction = InputActionReference.Create(sprintInputAction);

            networkKCC.SetupInputs();
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
            m_PrefabToSpawn.AddComponent<PlayerInput>();
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

        [UnityTearDown]
        public override IEnumerator UnityTearDown()
        {
            base.TearDown();
            yield return base.UnityTearDown();
            GameObject.Destroy(floor);
        }

        [UnitySetUp]
        public override IEnumerator UnitySetUp()
        {
            base.SetUp();
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

                GameObject go = SpawnObject(m_PrefabToSpawn, ownerManager);

                // Move to a position depending on index
                go.transform.position = Vector3.right * objectIndex * 2 + Vector3.up;

                // wait for each object to spawn on each client
                for (int clientIndex = 0; clientIndex < 3; clientIndex++)
                {
                    while (!TestableNetworkKCC.Objects.ContainsKey((objectIndex, clientIndex)))
                    {
                        yield return new WaitForSeconds(0.0f);
                    }
                }
            }
        }

        [UnityTest]
        public IEnumerator Validate_KCCStateMachine_Move_Transition()
        {
            TestableNetworkKCC demo = TestableNetworkKCC.Objects[(0, 0)];
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();

            NetworkKCC networkKCC = demo.GetComponent<NetworkKCC>();
            input.Move(gamepad.leftStick, Vector2.up);
            Debug.Log("Move input action value: " + demo.moveInputAction.ReadValue<Vector2>());
            
            while (typeof(KCCStateMachine.WalkingState) != networkKCC.CurrentState)
            {
                Debug.Log("Current state: " + networkKCC.CurrentState);
                yield return null;
            }
            
            Assert.AreEqual(typeof(KCCStateMachine.WalkingState), networkKCC.CurrentState);
            Assert.IsTrue(networkKCC.config.groundedState.StandingOnGround);
            Assert.IsFalse(networkKCC.config.groundedState.Sliding);
            Assert.IsFalse(networkKCC.config.groundedState.Falling);
        }
    }
}
