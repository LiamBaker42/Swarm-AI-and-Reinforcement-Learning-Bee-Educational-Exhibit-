using UnityEngine;

public class HoneyTankVisual : MonoBehaviour
{
    [Header("References")]
    public HiveManager hive;
    public Transform pivotObject; 

    [Header("Level Settings")]
    public float minScale = 0.01f; 
    public float maxScale = 1.0f;  
    public int maxCapacity = 500;

    [Header("Visual Polish")]
    public Renderer honeyRenderer;
    public Color emptyColor = Color.yellow;
    public Color fullColor = new Color(1, 0.5f, 0); 

    void Update()
    {
        if (hive == null || pivotObject == null) return;

        // Determine how full the tank is (0.0 to 1.0)
        float ratio = Mathf.Clamp01((float)hive.GetNectarAmount() / maxCapacity);

        // Adjust the height of the honey via the pivot's Y-scale
        float targetY = Mathf.Lerp(minScale, maxScale, ratio);
        pivotObject.localScale = new Vector3(1, targetY, 1);

        // Update the visual look if a renderer is assigned
        if (honeyRenderer != null)
        {
            UpdateHoneyVisuals(ratio);
        }
    }

    private void UpdateHoneyVisuals(float ratio)
    {
        // Blend between thin yellow and thick orange based on volume
        Color currentColor = Color.Lerp(emptyColor, fullColor, ratio);
        
        // Apply basic color to the material
        honeyRenderer.material.color = currentColor;
        
        // Apply glow effect that intensifies as the tank fills
        honeyRenderer.material.SetColor("_EmissionColor", currentColor * ratio);
    }
}