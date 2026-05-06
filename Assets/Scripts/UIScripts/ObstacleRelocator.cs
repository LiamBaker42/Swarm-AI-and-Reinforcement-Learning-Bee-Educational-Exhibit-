using UnityEngine;
using System.Collections.Generic;

public class ObstacleRelocator : MonoBehaviour
{
    [Header("Obstacles")]
    public List<Transform> obstacleTransforms = new List<Transform>();

    [Header("Bounds")]
    public Transform cornerTopLeft;
    public Transform cornerTopRight;
    public Transform cornerBottomLeft;
    public Transform cornerBottomRight;

    [Header("Safety Settings")]
    public float obstacleCollisionRadius = 3f;
    public LayerMask blockingLayers;
    public int maxSafetyAttempts = 100;

    // Iterates through all assigned obstacles and moves them to valid random positions
    public void RelocateObstacles()
    {
        foreach (Transform obs in obstacleTransforms)
        {
            if (obs == null) continue;

            Vector3 originalPos = obs.position;

            // Move object out of the way temporarily so it doesn't collide with itself during CheckSphere
            obs.position = new Vector3(0, -100, 0);

            Vector3 bestPos = originalPos;
            bool foundSpot = false;

            // Attempt to find a clear spot up to maxSafetyAttempts times
            for (int i = 0; i < maxSafetyAttempts; i++)
            {
                Vector3 candidate = GetRandomPointInBounds();

                // Check if the candidate position is clear of other colliders
                if (!Physics.CheckSphere(candidate, obstacleCollisionRadius, blockingLayers))
                {
                    bestPos = candidate;
                    foundSpot = true;
                    break;
                }
            }

            obs.position = bestPos;

            // Apply a random rotation if a valid spot was found
            if (foundSpot)
            {
                obs.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }
    }

    // Calculates a random coordinate within the defined rectangular area
    private Vector3 GetRandomPointInBounds()
    {
        float minX = Mathf.Min(cornerTopLeft.position.x, cornerBottomLeft.position.x);
        float maxX = Mathf.Max(cornerTopRight.position.x, cornerBottomRight.position.x);
        float minZ = Mathf.Min(cornerBottomLeft.position.z, cornerBottomRight.position.z);
        float maxZ = Mathf.Max(cornerTopLeft.position.z, cornerTopRight.position.z);

        return new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
    }

    // Visualize the collision radius of obstacles in the Unity Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        foreach (var obs in obstacleTransforms)
        {
            if (obs != null) Gizmos.DrawSphere(obs.position, obstacleCollisionRadius);
        }
    }
}