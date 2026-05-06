using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DanceRecruitmentTests
{
    [UnityTest]
public IEnumerator Test_Hive_RecruitsOnlookerOnHighQualityDance()
{
    GameObject sunObj = new GameObject("Sun");
    var solar = sunObj.AddComponent<SolarManager>();
    solar.sunLight = sunObj.AddComponent<Light>();

    // 1. Setup Hive
    GameObject hiveObj = new GameObject("Hive");
    var hive = hiveObj.AddComponent<HiveManager>();

    // 2. Setup a Bee in the "Waiting" state
    GameObject beeObj = new GameObject("Bee");
    beeObj.AddComponent<Rigidbody>();
    var stateManager = beeObj.AddComponent<BeeStateManager>();
    
    stateManager.hiveTransform = hiveObj.transform;
    stateManager.SetState(BeeState.DANCING);
    stateManager.latestStatusMessage = "Waiting in Hive (Onlooker)";

    // We have to manually add the bee to the HiveManager's internal list 
    // because FindObjectsByType doesn't always catch new objects mid-test
    var allBeesField = typeof(HiveManager).GetField("allBees", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    allBeesField.SetValue(hive, new System.Collections.Generic.List<BeeStateManager> { stateManager });

    // 3. Register a perfect quality dance (1.0)
    GameObject flower = new GameObject("Flower");
    hive.RegisterWaggleDance(flower.transform, 1.0f);

    // 4. Assert: The bee should now be following the dance
    Assert.AreEqual(BeeState.FOLLOWING_DANCE, stateManager.currentState, "The Hive failed to recruit the onlooker bee!");

    Object.Destroy(hiveObj);
    Object.Destroy(beeObj);
    Object.Destroy(flower);
    yield return null;
}
}
