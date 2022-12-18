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
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.TestHelpers.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace nickmaltbie.openkcc.Tests.netcode.TestCommon
{
    public abstract class NetcodeInputRuntimeTest<E> : NetcodeInputRuntimeTest where E : Component
    {
        protected virtual int SpawnCount => NumberOfClients + 1;
        protected GameObject m_PrefabToSpawn;

        public virtual void SetupClient(E e, int objectIdx, int clientIdx) { }
        public virtual void SetupInputs(Gamepad gamepad, TestableNetworkBehaviour b, E e) { }
        public abstract void SetupPrefab(GameObject go);

        protected GameObject GetObject(int objectIdx, int clientIdx) =>
            TestableNetworkBehaviour.Objects[(typeof(E), objectIdx, clientIdx)].gameObject;
        protected TestableNetworkBehaviour GetTestableNetworkBehaviour(int objectIdx, int clientIdx) =>
            TestableNetworkBehaviour.Objects[(typeof(E), objectIdx, clientIdx)];
        protected TestableNetworkBehaviour GetTestableNetworkBehaviour(Type type, int objectIdx, int clientIdx) =>
            TestableNetworkBehaviour.Objects[(type, objectIdx, clientIdx)];
        protected E GetAttachedNetworkBehaviour(int objectIdx, int clientIdx) =>
            TestableNetworkBehaviour.Objects[(typeof(E), objectIdx, clientIdx)].GetComponent<E>();
        protected bool HasClient(int objectIdx, int clientIdx) =>
            TestableNetworkBehaviour.Objects.ContainsKey((typeof(E), objectIdx, clientIdx));

        [OneTimeSetUp]
        public override void OneTimeSetup()
        {
            base.OneTimeSetup();
        }

        [UnitySetUp]
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
            TestableNetworkBehaviour.Objects.Clear();
        }

        protected override void OnServerAndClientsCreated()
        {
            m_PrefabToSpawn = CreateNetworkObjectPrefab($"Testable{typeof(E).Name}");
            m_PrefabToSpawn.AddComponent<TestableNetworkBehaviour>();
            m_PrefabToSpawn.AddComponent<E>();
            SetupPrefab(m_PrefabToSpawn);
        }

        protected void SetupInputs()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                Gamepad gamepad = InputSystem.AddDevice<Gamepad>($"Gamepad#{i}");
                TestableNetworkBehaviour demo = TestableNetworkBehaviour.Objects[(typeof(E), i, i)];
                SetupInputs(gamepad, demo, demo.GetComponent<E>());
            }
        }

        [UnitySetUp]
        public override IEnumerator UnitySetUp()
        {
            yield return base.UnitySetUp();

            // create a player for each character
            for (int objectIndex = 0; objectIndex < SpawnCount; objectIndex++)
            {
                yield return SpawnAndWait<E>(m_PrefabToSpawn, objectIndex, SetupClient);
            }
        }

        public IEnumerator ServerSpawnAndWait<SpawnType>(GameObject prefab, Action<SpawnType, int, int> ClientSpawn = null)
        {
            yield return SpawnAndWait(prefab, 0, ClientSpawn);
        }

        public IEnumerator SpawnAndWait<SpawnType>(GameObject prefab, int owner, Action<SpawnType, int, int> ClientSpawn = null)
        {
            TestableNetworkBehaviour.CurrentlyTesting = typeof(SpawnType);
            TestableNetworkBehaviour.CurrentlySpawning = owner;

            NetworkManager ownerManager = ServerNetworkManager;
            if (owner != 0)
            {
                ownerManager = ClientNetworkManagers[owner - 1];
            }

            if (prefab.GetComponent<TestableNetworkBehaviour>() == null)
            {
                prefab.AddComponent<TestableNetworkBehaviour>();
            }

            SpawnObject(prefab, ownerManager);
            for (int clientIndex = 0; clientIndex <= NumberOfClients; clientIndex++)
            {
                while (!TestableNetworkBehaviour.Objects.ContainsKey((typeof(SpawnType), owner, clientIndex)))
                {
                    yield return new WaitForSeconds(0.0f);
                }

                ClientSpawn?.Invoke(TestableNetworkBehaviour.Objects[(typeof(E), owner, clientIndex)].GetComponent<SpawnType>(), owner, clientIndex);
            }
        }

        public void ForEachTestableOwner(Action<TestableNetworkBehaviour, int> apply)
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                apply(GetTestableNetworkBehaviour(i, i), i);
            }
        }

        public void ForEachOwner(Action<E, int> apply)
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                apply(GetAttachedNetworkBehaviour(i, i), i);
            }
        }

        public bool ForAllPlayers(Func<E, bool> verify)
        {
            return Enumerable.Range(0, NumberOfClients + 1).All(i => ForAllPlayers(i, verify));
        }

        public bool ForServerObject(Func<E, bool> verify)
        {
            return ForAllPlayers(0, verify);
        }

        public bool ForAllPlayers(int index, Func<E, bool> verify)
        {
            return Enumerable.Range(0, NumberOfClients + 1).All(i =>
                HasClient(index, i) && verify(GetAttachedNetworkBehaviour(index, i)));
        }
    }

    public class TestableNetworkBehaviour : NetworkBehaviour
    {
        public static Dictionary<(Type, int, int), TestableNetworkBehaviour> Objects = new Dictionary<(Type, int, int), TestableNetworkBehaviour>();
        public static int CurrentlySpawning = 0;
        public static Type CurrentlyTesting = typeof(NetworkBehaviour);
        private Dictionary<string, InputControl> inputControls = new();

        public override void OnNetworkSpawn()
        {
            Objects[(CurrentlyTesting, CurrentlySpawning, (int)NetworkManager.LocalClientId)] = this;
        }

        public void SetupControl(string name, InputControl inputControl)
        {
            inputControls[name] = inputControl;
        }

        public InputControl GetControl(string name)
        {
            return inputControls[name];
        }

        public E GetControl<E>(string name) where E : InputControl
        {
            return GetControl(name) as E;
        }
    }

    public abstract class NetcodeInputRuntimeTest
    {
        protected abstract int NumberOfClients { get; }
        protected NetcodeIntegrationTestHelper netcodeHelper;
        protected InputTestFixture input;

        public static implicit operator NetcodeIntegrationTest(NetcodeInputRuntimeTest nt)
        {
            return nt.netcodeHelper;
        }

        public sealed class NetcodeIntegrationTestHelper : NetcodeIntegrationTest
        {
            private NetcodeInputRuntimeTest netcodeInputRuntime;

            public NetcodeIntegrationTestHelper(NetcodeInputRuntimeTest netcodeInputRuntime)
            {
                this.netcodeInputRuntime = netcodeInputRuntime;
            }

            public new GameObject m_PlayerPrefab => base.m_PlayerPrefab;
            public new NetworkManager m_ServerNetworkManager => base.m_ServerNetworkManager;
            public new NetworkManager[] m_ClientNetworkManagers => base.m_ClientNetworkManagers;

            protected override int NumberOfClients => netcodeInputRuntime.NumberOfClients;

            protected override void OnServerAndClientsCreated()
            {
                netcodeInputRuntime.OnServerAndClientsCreated();
            }

            public new GameObject CreateNetworkObjectPrefab(string baseName)
            {
                return base.CreateNetworkObjectPrefab(baseName);
            }

            public new GameObject SpawnObject(GameObject prefabGameObject, NetworkManager owner, bool destroyWithScene = false)
            {
                return base.SpawnObject(prefabGameObject, owner, destroyWithScene);
            }

            public new List<GameObject> SpawnObjects(GameObject prefabGameObject, NetworkManager owner, int count, bool destroyWithScene = false)
            {
                return base.SpawnObjects(prefabGameObject, owner, count, destroyWithScene);
            }
        }

        public NetcodeInputRuntimeTest()
        {
            netcodeHelper = new NetcodeIntegrationTestHelper(this);
            input = new InputTestFixture();
        }

        /// <summary>
        /// This is invoked before the server and client(s) are started.
        /// Override this method if you want to make any adjustments to their
        /// NetworkManager instances.
        /// </summary>
        protected virtual void OnServerAndClientsCreated()
        {

        }

        /// <summary>
        /// Creates a basic NetworkObject test prefab, assigns it to a new
        /// NetworkPrefab entry, and then adds it to the server and client(s)
        /// NetworkManagers' NetworkConfig.NetworkPrefab lists.
        /// </summary>
        /// <param name="baseName">the basic name to be used for each instance</param>
        /// <returns>NetworkObject of the GameObject assigned to the new NetworkPrefab entry</returns>
        protected GameObject CreateNetworkObjectPrefab(string baseName)
        {
            return netcodeHelper.CreateNetworkObjectPrefab(baseName);
        }

        protected GameObject SpawnObject(GameObject prefabGameObject, NetworkManager owner, bool destroyWithScene = false)
        {
            return netcodeHelper.SpawnObject(prefabGameObject, owner, destroyWithScene);
        }

        protected List<GameObject> SpawnObjects(GameObject prefabGameObject, NetworkManager owner, int count, bool destroyWithScene = false)
        {
            return netcodeHelper.SpawnObjects(prefabGameObject, owner, count, destroyWithScene);
        }

        protected GameObject PlayerPrefab => netcodeHelper.m_PlayerPrefab;
        protected NetworkManager ServerNetworkManager => netcodeHelper.m_ServerNetworkManager;
        protected NetworkManager[] ClientNetworkManagers => netcodeHelper.m_ClientNetworkManagers;

        [OneTimeSetUp]
        public virtual void OneTimeSetup()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene, UnityEditor.SceneManagement.NewSceneMode.Single);
            }
            else
            {
                UnityEngine.SceneManagement.Scene scene = SceneManager.CreateScene("EmptyTestScene");
            }
#endif
            netcodeHelper.OneTimeSetup();
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            netcodeHelper.OneTimeTearDown();
        }

        [SetUp]
        public virtual void SetUp()
        {
            input.Setup();
        }

        [UnitySetUp]
        public virtual IEnumerator UnitySetUp()
        {
            return netcodeHelper.SetUp();
        }

        [TearDown]
        public virtual void TearDown()
        {
            input.TearDown();
        }

        [UnityTearDown]
        public virtual IEnumerator UnityTearDown()
        {
            return netcodeHelper.TearDown();
        }
    }
}
