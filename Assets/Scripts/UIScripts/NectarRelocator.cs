using UnityEngine;
using System.Collections.Generic;

public class NectarRelocator : MonoBehaviour
{
    [Header("Nectar Sources")]
    public List<Transform> nectarSources = new List<Transform>();

    [Header("Manual Bounds (Transforms)")]
    public Transform cornerTopLeft;
    public Transform cornerTopRight;
    public Transform cornerBottomLeft;
    public Transform cornerBottomRight;

    [Header("Distance Rules")]
    public Transform hiveTransform;
    public float hiveHeight = 1f;
    public float minDistanceFromHive = 10f;
    public float minDistanceBetweenSources = 5f;

    [Header("Obstacle Avoidance")]
    [Tooltip("Which layers should the nectar avoid? (e.g. Hive, Obstacles, Nectar)")]
    public LayerMask blockingLayers;
    public float obstacleSafetyBuffer = 2f;

    [Header("Settings")]
    public bool relocateOnStart = true;
    public int maxSafetyAttempts = 100;

    void Start()
    {
        if (relocateOnStart) RelocateAll();
    }

    // Move all nectar sources to randomized valid positions
    public void RelocateAll()
    {
        foreach (Transform source in nectarSources)
        {
            if (source == null) continue;

            Vector3 finalPos = source.position;
            bool foundSpot = false;

            // Attempt to find a location that satisfies all distance and collision rules
            for (int i = 0; i < maxSafetyAttempts; i++)
            {
                Vector3 candidate = GetRandomPointInBounds();

                // Check for physical collisions with obstacles/other nectar
                if (!Physics.CheckSphere(candidate, minDistanceBetweenSources / 2f, blockingLayers))
                {
                    // Ensure the source isn't spawning too close to the hive
                    if (Vector3.Distance(candidate, hiveTransform.position) >= minDistanceFromHive)
                    {
                        finalPos = candidate;
                        foundSpot = true;
                        break;
                    }
                }
            }

            source.position = finalPos;

            if (!foundSpot)
                Debug.LogWarning($"{gameObject.name}: Could not find safe spot for {source.name}");
        }
    }

    // Calculates a random coordinate within the boundary markers
    private Vector3 GetRandomPointInBounds()
    {
        float minX = Mathf.Min(cornerTopLeft.position.x, cornerBottomLeft.position.x);
        float maxX = Mathf.Max(cornerTopRight.position.x, cornerBottomRight.position.x);
        float minZ = Mathf.Min(cornerBottomLeft.position.z, cornerBottomRight.position.z);
        float maxZ = Mathf.Max(cornerTopLeft.position.z, cornerTopRight.position.z);

        return new Vector3(Random.Range(minX, maxX), transform.position.y - hiveHeight, Random.Range(minZ, maxZ));
    }

    // Draw the boundary lines and hive exclusion zone in the Scene view
    void OnDrawGizmosSelected()
    {
        if (cornerTopLeft && cornerTopRight && cornerBottomLeft && cornerBottomRight)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(cornerTopLeft.position, cornerTopRight.position);
            Gizmos.DrawLine(cornerTopRight.position, cornerBottomRight.position);
            Gizmos.DrawLine(cornerBottomRight.position, cornerBottomLeft.position);
            Gizmos.DrawLine(cornerBottomLeft.position, cornerTopLeft.position);

            if (hiveTransform)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(hiveTransform.position, minDistanceFromHive);
            }
        }
    }
}