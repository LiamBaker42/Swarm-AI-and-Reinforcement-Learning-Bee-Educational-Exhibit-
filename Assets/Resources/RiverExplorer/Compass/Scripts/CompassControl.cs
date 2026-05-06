using UnityEngine;
using TMPro;

public class BeeSolarHUD : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform sunNeedle;    // The Red Star/Needle
    public RectTransform nectarArrow; // The Green Arrow
    public TextMeshProUGUI sunAngleText;
    public TextMeshProUGUI danceAngleText;

    void Update()
    {
        if (SolarManager.Instance == null) return;

        // 1. Update the Sun Needle (Red)
        Vector3 sunDir = SolarManager.Instance.GetSunDirection();
        float sunAngle = Mathf.Atan2(sunDir.x, sunDir.z) * Mathf.Rad2Deg;

        // Rotate the needle to point at the sun's azimuth
        sunNeedle.localRotation = Quaternion.Euler(0, 0, -sunAngle);

        if (sunAngleText != null)
        {
            float displayAngle = (sunAngle + 360) % 360;
            sunAngleText.text = $"{displayAngle:F0}°";
        }

        // 2. Update the Nectar Arrow (Green)
        UpdateNectarArrow();
    }

    private void UpdateNectarArrow()
    {
        var selected = BeeSelector.selectedBee;

        // Only show arrow if bee is selected and heading to a target (Worker or Returning)
        if (selected != null && selected.currentMentalTarget != Vector3.zero && selected.currentState != BeeState.SCOUTING)
        {
            nectarArrow.gameObject.SetActive(true);

            // 1. Get the direction from Hive to Nectar in World Space
            Vector3 hiveToNectar = (selected.currentMentalTarget - selected.hiveTransform.position).normalized;

            // 2. Convert that 3D world vector into a 2D angle (0 to 360)
            // We use x and z because y is height (up/down)
            float worldAngle = Mathf.Atan2(hiveToNectar.x, hiveToNectar.z) * Mathf.Rad2Deg;

            if (selected.currentState == BeeState.FOLLOWING_DANCE)
            {
                // 3. Apply the rotation
                nectarArrow.localRotation = Quaternion.Euler(0, 0, -worldAngle + 180f);
                Vector3 currentSunDir = SolarManager.Instance.GetSunDirection();
                float waggleAngle = Vector3.SignedAngle(currentSunDir, hiveToNectar, Vector3.up);

                danceAngleText.text = $"Dance: {waggleAngle:F0}°";
            }
            else
            {
                nectarArrow.localRotation = Quaternion.Euler(0, 0, -worldAngle);
                danceAngleText.text = "0°";
            }
        }
        else
        {
            nectarArrow.gameObject.SetActive(false);
        }
    }
}