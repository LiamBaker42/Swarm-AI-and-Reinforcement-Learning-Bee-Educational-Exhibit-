using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SunManagerTests
{
    [UnityTest]
    public IEnumerator Test_SolarManager_DirectionChangesWithTime()
    {
        // Setup
        GameObject solarObj = new GameObject("Sun");
        var solar = solarObj.AddComponent<SolarManager>();
        solar.sunLight = solarObj.AddComponent<Light>();
        solar.sunLight.type = LightType.Directional;
        solar.dayDurationInSeconds = 10f; // Make the day very fast for the test

        Vector3 initialDir = solar.GetSunDirection();

        // Wait for 1 second of "game time"
        yield return new WaitForSeconds(1f);

        Vector3 newDir = solar.GetSunDirection();

        // Assert: The vectors should not be the same
        Assert.AreNotEqual(initialDir, newDir, "The Sun direction failed to update over time!");

        Object.Destroy(solarObj);
    }
}
