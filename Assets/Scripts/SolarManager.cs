using UnityEngine;

public class SolarManager : MonoBehaviour
{
    public static SolarManager Instance;

    [Header("Physical Sun")]
    public Light sunLight;

    [Header("Settings")]
    public float dayDurationInSeconds = 300f;
    public float sunElevationAngle = 45f;

    private float currentTime;

    void Awake()
    {
        // Simple singleton pattern to allow global access from bee scripts
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Increment day cycle progress from 0.0 to 1.0
        currentTime = (currentTime + Time.deltaTime / dayDurationInSeconds) % 1f;

        UpdateSunPosition();
    }

    void UpdateSunPosition()
    {
        if (sunLight == null) return;

        // Map time progress to a full 360-degree circle
        float azimuth = currentTime * 360f;

        // Update light rotation based on set elevation and calculated compass heading
        sunLight.transform.rotation = Quaternion.Euler(sunElevationAngle, azimuth, 0);
    }

    // Used by bees to calculate their Waggle Dance angle relative to the sun
    public Vector3 GetSunDirection()
    {
        if (sunLight == null) return Vector3.forward;

        // Flatten the sun's direction onto the XZ plane for simple navigation
        Vector3 dir = sunLight.transform.forward;
        dir.y = 0;
        return dir.normalized;
    }
}