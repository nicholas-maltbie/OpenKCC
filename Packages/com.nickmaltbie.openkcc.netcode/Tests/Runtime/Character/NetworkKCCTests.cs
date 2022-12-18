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
using System.Linq;
using nickmaltbie.openkcc.Tests.netcode.TestCommon;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.OpenKCC.netcode.Character;
using nickmaltbie.OpenKCC.netcode.Utils;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;
using static nickmaltbie.OpenKCC.netcode.Character.NetworkKCC;

namespace nickmaltbie.openkcc.Tests.netcode.Runtime.Character
{
    public class MovingGroundConveyer : MonoBehaviour, IMovingGround
    {
        public Vector3 push = Vector3.forward;

        public bool AvoidTransferMomentum()
        {
            return false;
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

    /// <summary>
    /// Simple tests meant to be run in PlayMode
    /// </summary>
    [TestFixture]
    public class NetworkKCCTests : NetcodeInputRuntimeTest<NetworkKCC>
    {
        public const string MoveControlName = "Move";
        public const string JumpControlName = "Jump";
        public const string SprintControlName = "Sprint";
        public const string AnimControllerPath = "Packages/com.nickmaltbie.openkcc/Common/Animations/CharacterAnimationController.controller";

        protected override int NumberOfClients => 2;
        private GameObject floor;

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            base.OneTimeSetup();
        }

        [OneTimeTearDown]
        public override void OneTimeTearDown()
        {
            base.OneTimeTearDown();
        }

        [UnityTearDown]
        public override IEnumerator UnityTearDown()
        {
            yield return base.UnityTearDown();
            Object.Destroy(floor);
        }

        [UnitySetUp]
        public override IEnumerator UnitySetUp()
        {
            yield return base.UnitySetUp();

            // Spawn a floor below the players.
            floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "floor";
            floor.transform.localScale = new Vector3(100, 1, 100);
            floor.transform.position -= Vector3.up * 0.5f;
        }

        public override void SetupClient(NetworkKCC e, int objectIdx, int clientIdx)
        {
            e.TeleportPlayer(Vector3.right * clientIdx * 2 + Vector3.up * 0.025f);
            e.GetComponent<CapsuleCollider>().enabled = objectIdx == clientIdx;
        }

        [UnityTest]
        public IEnumerator Validate_NetworkKCC_Move_Transition()
        {
            yield return SetupPlayersInIdleState();

            for (int i = 0; i <= NumberOfClients; i++)
            {
                NetworkKCC kcc = GetAttachedNetworkBehaviour(i, i);

                // Wait for player to fall to ground
                yield return TestUtils.WaitUntil(() => typeof(IdleState) == kcc.CurrentState || typeof(LandingState) == kcc.CurrentState);
                input.Set(GetTestableNetworkBehaviour(i, i).GetControl<StickControl>(MoveControlName), Vector2.up);
                yield return TestUtils.WaitUntil(() => typeof(WalkingState) == kcc.CurrentState);
                input.Set(GetTestableNetworkBehaviour(i, i).GetControl<ButtonControl>(SprintControlName), 1.0f);
                yield return TestUtils.WaitUntil(() => typeof(SprintingState) == kcc.CurrentState);
                input.Set(GetTestableNetworkBehaviour(i, i).GetControl<StickControl>(MoveControlName), Vector2.zero);
                yield return TestUtils.WaitUntil(() => typeof(IdleState) == kcc.CurrentState);
                input.Set(GetTestableNetworkBehaviour(i, i).GetControl<ButtonControl>(JumpControlName), 1.0f);
                yield return TestUtils.WaitUntil(() => typeof(JumpState) == kcc.CurrentState);
            }
        }

        [UnityTest]
        public IEnumerator Validate_NetworkKCC_MovingGround()
        {
            yield return SetupPlayersInIdleState();

            // Attach a moving ground conveyer to the ground
            MovingGroundConveyer conveyer = floor.AddComponent<MovingGroundConveyer>();

            // Assert that the players are pushed forward in some direction of the moving
            // ground push by one unit. First save their initial positions
            var initialPos = Enumerable.Range(0, NumberOfClients + 1)
                .Select(i => GetObject(i, i).transform.position)
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

            // Have the players jump and assert they retain some forward momentum
            ForEachTestableOwner((player, _) => input.Set(player.GetControl<ButtonControl>(JumpControlName), 1.0f));

            yield return TestUtils.WaitUntil(() =>
                Enumerable.Range(0, NumberOfClients + 1).All(
                    i =>
                    {
                        NetworkKCC networkKCC = GetAttachedNetworkBehaviour(i, i);
                        return Vector3.Project(networkKCC.Velocity, conveyer.push.normalized).magnitude >= 1.0f;
                    }));
        }

        [UnityTest]
        public IEnumerator Validate_NetworkKCC_Sliding()
        {
            SetupPlayersInIdleState();
            floor.transform.position = Vector3.down * 3;
            yield return new WaitForFixedUpdate();
            floor.transform.rotation = Quaternion.Euler(70.0f, 0, 0);

            ForEachOwner((player, i) => player.TeleportPlayer(Vector3.zero));

            // Wait until players are in sliding satte
            yield return TestUtils.WaitUntil(() => ForAllPlayers(player => typeof(SlidingState) == player.CurrentState));
        }

        protected IEnumerator SetupPlayersInIdleState()
        {
            ForEachOwner((player, i) => player.TeleportPlayer(Vector3.right * i * 2 + Vector3.up * 0.1f));
            SetupInputs();
            yield return TestUtils.WaitUntil(() => ForAllPlayers(player => typeof(IdleState) == player.CurrentState));
        }

        public override void SetupInputs(Gamepad gamepad, TestableNetworkBehaviour b, NetworkKCC networkKCC)
        {
            b.SetupControl(MoveControlName, gamepad.leftStick);
            b.SetupControl(JumpControlName, gamepad.aButton);
            b.SetupControl(SprintControlName, gamepad.bButton);

            var jumpInputAction = new InputAction("jumpAction", InputActionType.Button, b.GetControl(JumpControlName).path);
            var sprintInputAction = new InputAction("sprintAction", InputActionType.Value, b.GetControl(SprintControlName).path);
            var moveInputAction = new InputAction("moveAction", InputActionType.Value, b.GetControl(MoveControlName).path);

            // Enable inputs
            jumpInputAction.Enable();
            moveInputAction.Enable();
            sprintInputAction.Enable();

            // Configure jump action
            var jumpInput = new BufferedInput();
            jumpInput.InputAction = jumpInputAction;
            jumpInput.cooldown = 1.0f;
            jumpInput.bufferTime = 3.0f;
            var jumpAction = new JumpAction();
            jumpAction.jumpInput = jumpInput;

            // Setup actions
            networkKCC.config.jumpAction = jumpAction;
            networkKCC.config.MoveAction = moveInputAction;
            networkKCC.config.SprintAction = sprintInputAction;

            networkKCC.SetupInputs();
        }

        public override void SetupPrefab(GameObject go)
        {
            NetworkKCC kcc = go.GetComponent<NetworkKCC>();
            kcc.config.maxPushSpeed = 100.0f;

            go.AddComponent<CapsuleColliderCast>();
            go.AddComponent<ClientNetworkTransform>();

            // Setup the rigidbody
            Rigidbody rb = go.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // Setup animation controller.
            Animator anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath(AnimControllerPath, typeof(AnimatorController)) as AnimatorController;

            // setup capsule collider
            CapsuleCollider capsuleCollider = go.GetComponent<CapsuleCollider>();
            capsuleCollider.center = new Vector3(0, 1, 0);
            capsuleCollider.height = 2.0f;
            capsuleCollider.radius = 0.5f;
        }
    }
}
