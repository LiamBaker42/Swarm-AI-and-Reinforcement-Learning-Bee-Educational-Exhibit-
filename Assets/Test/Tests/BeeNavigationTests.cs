using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BeeNavigationTests
{
    [UnityTest]
    public IEnumerator Test_MentalTarget_StaysOnNectar()
    {

        // 1. Setup Sun
        GameObject sunObj = new GameObject("Sun");
        var solar = sunObj.AddComponent<SolarManager>();
        solar.sunLight = sunObj.AddComponent<Light>();

        // 2. Setup Bee WITH the parts it looks for in Awake()
        GameObject beeObj = new GameObject("Bee");
        beeObj.AddComponent<Rigidbody>();
        beeObj.AddComponent<BeeAgent>();
        //beeObj.AddComponent<BeeVisuals>();
        beeObj.AddComponent<BeeAnimationController>();

        // NOW add the StateManager
        var stateManager = beeObj.AddComponent<BeeStateManager>();

        // 3. Setup Hive & Flower
        GameObject hiveObj = new GameObject("Hive");
        stateManager.hiveTransform = hiveObj.transform;
        GameObject flower = new GameObject("Flower");
        flower.transform.position = new Vector3(10, 0, 10);

        // 4. Test
        stateManager.RecruitToTarget(flower.transform, 1.0f);
        yield return null; // Wait for Update()

        Vector3 initialPos = stateManager.currentMentalTarget;
        yield return new WaitForSeconds(0.2f);

        float drift = Vector3.Distance(initialPos, stateManager.currentMentalTarget);
        Assert.Less(drift, 0.1f, "Math Error: The target is drifting with the sun!");
    }
}