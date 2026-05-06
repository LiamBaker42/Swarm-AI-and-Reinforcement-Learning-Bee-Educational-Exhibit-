using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Reflection;

public class AgentTests
{
    private void LinkState(BeeAgent agent, BeeStateManager state)
    {
        var field = typeof(BeeAgent).GetField("state", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(agent, state);
    }

    // Helper to setup the Sun for tests
    private SolarManager CreateTestSun()
    {
        GameObject sunObj = new GameObject("Sun");
        var solar = sunObj.AddComponent<SolarManager>();
        solar.sunLight = sunObj.AddComponent<Light>();
        solar.sunLight.type = LightType.Directional;
        return solar;
    }

    [UnityTest]
    public IEnumerator Test_AgentObservations_AreValid()
    {
        var solar = CreateTestSun();

        GameObject beeObj = new GameObject("Bee");
        var rb = beeObj.AddComponent<Rigidbody>();
        var agent = beeObj.AddComponent<BeeAgent>();
        var state = beeObj.AddComponent<BeeStateManager>();

        state.hiveTransform = new GameObject("Hive").transform;
        LinkState(agent, state);

        yield return null;

        state.currentMentalTarget = new Vector3(30, 0, 0);
        beeObj.transform.position = Vector3.zero;

        var sensor = new VectorSensor(9);
        agent.CollectObservations(sensor);

        // This checks if your math is (30 / 60) = 0.5
        Assert.AreEqual(0.5f, agent.obsTargetDist, 0.05f);

        Object.Destroy(solar.gameObject);
        Object.Destroy(beeObj);
    }

    [UnityTest]
    public IEnumerator Test_AgentActions_ApplyMovement()
    {
        var solar = CreateTestSun(); // Added back to prevent NullRef at Line 40

        GameObject beeObj = new GameObject("Bee");
        var rb = beeObj.AddComponent<Rigidbody>();
        var agent = beeObj.AddComponent<BeeAgent>();
        var state = beeObj.AddComponent<BeeStateManager>();
        state.hiveTransform = new GameObject("Hive").transform;
        LinkState(agent, state);

        agent.MaxStep = 5000;

        yield return null;

        state.currentMentalTarget = new Vector3(50, 0, 0);
        agent.OnEpisodeBegin();

        var actions = new ActionBuffers(new float[] { 1f, 0f }, new int[] { });

        // This won't crash now because SolarManager.Instance exists
        agent.OnActionReceived(actions);

        Assert.AreEqual(state.moveForce, rb.linearVelocity.magnitude, 0.1f);

        Object.Destroy(solar.gameObject);
        Object.Destroy(beeObj);
    }

    [UnityTest]
    public IEnumerator Test_Agent_RewardsProximity()
    {
        var solar = CreateTestSun(); // Added back to prevent NullRef at Line 40

        GameObject beeObj = new GameObject("Bee");
        var rb = beeObj.AddComponent<Rigidbody>();
        var agent = beeObj.AddComponent<BeeAgent>();
        var state = beeObj.AddComponent<BeeStateManager>();
        state.hiveTransform = new GameObject("Hive").transform;
        LinkState(agent, state);

        agent.MaxStep = 5000;

        yield return null;

        state.currentMentalTarget = new Vector3(100, 0, 0);
        beeObj.transform.position = Vector3.zero;
        agent.OnEpisodeBegin();

        beeObj.transform.position = new Vector3(10, 0, 0);

        var actions = new ActionBuffers(new float[] { 0.1f, 0f }, new int[] { });
        agent.OnActionReceived(actions);

        Assert.Greater(agent.GetCumulativeReward(), 0f);

        Object.Destroy(solar.gameObject);
        Object.Destroy(beeObj);
    }
}