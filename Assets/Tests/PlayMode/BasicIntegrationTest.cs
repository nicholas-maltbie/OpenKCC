using System.Collections;
using System.Collections.Generic;
using nickmaltbie.OpenKCC.Character;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BasicIntegrationTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void BasicIntegrationTestSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator BasicIntegrationTestWithEnumeratorPasses()
    {
        // Create a basic character controllers
        GameObject go = new GameObject();
        KinematicCharacterController openKCC = go.AddComponent<KinematicCharacterController>();

        yield return null;

        GameObject.DestroyImmediate(go);
    }
}
