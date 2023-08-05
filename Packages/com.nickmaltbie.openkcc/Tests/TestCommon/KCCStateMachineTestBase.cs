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

using System.Collections.Generic;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Animation;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#if UNITY_EDITOR
#endif

namespace nickmaltbie.OpenKCC.Tests.TestCommon
{
    /// <summary>
    /// Tests for the <see cref="nickmaltbie.OpenKCC.Character.KCCStateMachine"/>
    /// state machine.
    /// </summary>
    public class KCCStateMachineTestBase : TestBase
    {
        protected InputActionAsset inputActionAsset;

        protected StickControl moveStick;
        protected ButtonControl jumpButton;
        protected ButtonControl sprintButton;

        protected Gamepad gamepad;
        protected InputAction moveInputAction;
        protected InputAction jumpInputAction;
        protected InputAction sprintInputAction;
        protected JumpAction jumpAction;
        protected KCCStateMachine kccStateMachine;
        protected KCCMovementEngine moveEngine;
        protected KCCGroundedState KCCGroundedState => moveEngine.GroundedState;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            var go = new GameObject();
            CapsuleCollider capsuleCollider = go.AddComponent<CapsuleCollider>();
            go.AddComponent<CapsuleColliderCast>();
            capsuleCollider.center = new Vector3(0, 1, 0);
            capsuleCollider.height = 2.0f;
            capsuleCollider.radius = 0.5f;

            var controller = new AnimatorController();
            controller.AddLayer("base");
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);

            rootStateMachine.AddState(HumanoidKCCAnim.IdleAnimState);
            rootStateMachine.AddState(HumanoidKCCAnim.JumpAnimState);
            rootStateMachine.AddState(HumanoidKCCAnim.LandingAnimState);
            rootStateMachine.AddState(HumanoidKCCAnim.WalkingAnimState);
            rootStateMachine.AddState(HumanoidKCCAnim.SlidingAnimState);
            rootStateMachine.AddState(HumanoidKCCAnim.FallingAnimState);
            rootStateMachine.AddState(HumanoidKCCAnim.SprintingAnimState);
            rootStateMachine.AddState(HumanoidKCCAnim.LongFallingAnimState);

            Animator anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;

            var jumpInput = new BufferedInput();
            gamepad = InputSystem.AddDevice<Gamepad>();
            inputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap actionMap = inputActionAsset.AddActionMap("testMap");
            jumpButton = gamepad.aButton;
            sprintButton = gamepad.bButton;
            moveStick = gamepad.leftStick;
            jumpInputAction = actionMap.AddAction("jumpAction", InputActionType.Button, jumpButton.path);
            moveInputAction = actionMap.AddAction("moveAction", InputActionType.Value, moveStick.path);
            sprintInputAction = actionMap.AddAction("sprintAction", InputActionType.Value, sprintButton.path);

            jumpInputAction.Enable();
            moveInputAction.Enable();
            sprintInputAction.Enable();

            jumpInput.inputActionReference = InputActionReference.Create(jumpInputAction);
            jumpInput.cooldown = 1.0f;
            jumpInput.bufferTime = 3.0f;
            jumpAction = new JumpAction();
            jumpAction.jumpInput = jumpInput;

            kccStateMachine = go.AddComponent<KCCStateMachine>();
            kccStateMachine.jumpAction = jumpAction;
            kccStateMachine.MoveAction = moveInputAction;
            kccStateMachine.SprintAction = sprintInputAction;
            kccStateMachine.Awake();

            moveEngine = go.GetComponent<KCCMovementEngine>();
        }

        [TearDown]
        public override void TearDown()
        {
            InputSystem.RemoveDevice(gamepad);
            Object.DestroyImmediate(kccStateMachine.gameObject);
            ScriptableObject.DestroyImmediate(inputActionAsset);
            base.TearDown();
        }
    }
}
