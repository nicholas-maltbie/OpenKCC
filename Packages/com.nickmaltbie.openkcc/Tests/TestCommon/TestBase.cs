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
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.TestCommon
{
    /// <summary>
    /// Basic unity test base class.
    /// </summary>
    public class TestBase : InputTestFixture
    {
        /// <summary>
        /// Set of basic directions for tests.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Vector3> TestDirections()
        {
            return new[]
            {
                Vector3.forward,
                Vector3.back,
                Vector3.left,
                Vector3.right,
                Vector3.up,
                Vector3.down,
                Vector3.zero
            };
        }

        /// <summary>
        /// List of game objects created as part of the test.
        /// </summary>
        protected List<GameObject> gameObjects;

        /// <summary>
        /// List of scriptable objects created as part of the test.
        /// </summary>
        protected List<ScriptableObject> scriptableObjects;

        /// <summary>
        /// List of created input devices.
        /// </summary>
        protected List<InputDevice> createdDevices;

        /// <summary>
        /// Setup a basic test.
        /// </summary>
        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            }
#endif

            gameObjects = new List<GameObject>();
            scriptableObjects = new List<ScriptableObject>();
            createdDevices = new List<InputDevice>();
        }

        /// <summary>
        /// Cleanup created objects in the basic test.
        /// </summary>
        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            while (gameObjects.Count > 0)
            {
                GameObject go = gameObjects[0];
                gameObjects.RemoveAt(0);
                GameObject.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Cleanup created objects in the basic test.
        /// </summary>
        [UnityTearDown]
        public virtual void UnityTearDown()
        {
            while (gameObjects.Count > 0)
            {
                GameObject go = gameObjects[0];
                gameObjects.RemoveAt(0);

                GameObject.DestroyImmediate(go);
            }
        }

        /// <summary>
        /// Setup an input device and given action map.
        /// </summary>
        /// <typeparam name="E">Type of device craeted.</typeparam>
        /// <returns>Tuple of the input device, player input, and created action map.</returns>
        public (E, PlayerInput, InputActionMap) SetupInputDevice<E>() where E : InputDevice
        {
            E inputDevice = InputSystem.AddDevice<E>();
            GameObject go = CreateGameObject();
            PlayerInput playerInput = go.AddComponent<PlayerInput>();
            InputActionAsset inputActionAsset = CreateScriptableObject<InputActionAsset>();
            InputActionMap actionMap = inputActionAsset.AddActionMap("testMap");

            playerInput.actions = inputActionAsset;
            playerInput.currentActionMap = actionMap;

            RegisterInputDevice(inputDevice);

            return (inputDevice, playerInput, actionMap);
        }

        /// <summary>
        /// Validate that given gizmos are drawn.
        /// </summary>
        /// <param name="GizmoAction">Gizmo action to validate.</param>
        public IEnumerator ValidateDrawGizmo(Action GizmoAction, int maxIter = 60, bool assertDrawn = false)
        {
            GameObject go = CreateGameObject();
            GizmoValidator gv = go.AddComponent<GizmoValidator>();
            gv.GizmoAction = GizmoAction;
            return gv.ValidateGizmosDrawn(maxIter, assertDrawn);
        }

        /// <summary>
        /// Create a scriptable object and register it as part of the test.
        /// </summary>
        /// <typeparam name="E">Type of scriptable object to create.</typeparam>
        /// <returns>Created scriptable object.</returns>
        protected E CreateScriptableObject<E>() where E : ScriptableObject
        {
            E created = ScriptableObject.CreateInstance<E>();
            RegisterScriptableObject(created);
            return created;
        }

        /// <summary>
        /// Create a game object and save it to cleanup at the end of the test.
        /// </summary>
        /// <param name="original">Original object to create from base.</param>
        /// <returns>Created game object.</returns>
        protected GameObject CreateGameObject(GameObject original)
        {
            var go = GameObject.Instantiate(original);
            RegisterGameObject(go);
            return go;
        }

        /// <summary>
        /// Create a game object and save it to cleanup at the end of the test.
        /// </summary>
        /// <returns>Created game object.</returns>
        protected GameObject CreateGameObject()
        {
            var go = new GameObject();
            RegisterGameObject(go);
            return go;
        }

        /// <summary>
        /// Register a game object as part of a test to cleanup once the test is completed.
        /// </summary>
        /// <param name="go">Game object to register.</param>
        protected void RegisterGameObject(GameObject go)
        {
            gameObjects.Add(go);
        }

        /// <summary>
        /// Register a ScriptableObject as part of a test to cleanup once the test is completed.
        /// </summary>
        /// <param name="so">ScriptableObject to register.</param>
        protected void RegisterScriptableObject(ScriptableObject so)
        {
            scriptableObjects.Add(so);
        }

        /// <summary>
        /// Register an InputDevice as part of a test to cleanup once the test is completed.
        /// </summary>
        /// <param name="device">InputDevice to register.</param>
        protected void RegisterInputDevice(InputDevice device)
        {
            createdDevices.Add(device);
        }

        /// <summary>
        /// Basic mono behavior for validating gizmo behavior
        /// </summary>
        public class GizmoValidator : MonoBehaviour
        {
            /// <summary>
            /// Has the gizmo action been invoked.
            /// </summary>
            public bool Invoked { get; set; }

            /// <summary>
            /// Gizmo action to validate.
            /// </summary>
            public Action GizmoAction { get; set; }

            public void OnDrawGizmos()
            {
                GizmoAction?.Invoke();
                Invoked = true;
            }

            public IEnumerator ValidateGizmosDrawn(int maxIter = 60, bool assertValidation = false)
            {
                Invoked = false;
                for (int i = 0; i < maxIter && !Invoked; i++)
                {
                    yield return null;
                }

                if (assertValidation)
                {
                    Assert.IsTrue(Invoked);
                }
            }
        }
    }
}
