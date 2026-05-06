using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GlobalEnvironmentController : MonoBehaviour
{
    private List<NectarRelocator> allNectarRelocators = new List<NectarRelocator>();
    private List<ObstacleRelocator> allObstacleRelocators = new List<ObstacleRelocator>();

    void Awake()
    {
        RefreshRelocatorLists();
    }

 
    // Scans the entire scene for every arena relocator.
    public void RefreshRelocatorLists()
    {
        allNectarRelocators = Object.FindObjectsByType<NectarRelocator>(FindObjectsSortMode.None).ToList();
        allObstacleRelocators = Object.FindObjectsByType<ObstacleRelocator>(FindObjectsSortMode.None).ToList();

        Debug.Log($"Global Controller: Linked to {allNectarRelocators.Count} Nectar and {allObstacleRelocators.Count} Obstacle relocators.");
    }


    // Triggers a full arena reset across all parallel instances.
    public void RelocateAllArenas()
    {
        // Safety check: if lists are empty, try to find them again
        if (allNectarRelocators.Count == 0 || allObstacleRelocators.Count == 0)
            RefreshRelocatorLists();

        // Move Obstacles First
        foreach (var obsRelocator in allObstacleRelocators)
        {
            if (obsRelocator != null) obsRelocator.RelocateObstacles();
        }

        // Move Nectar
        foreach (var nectarRelocator in allNectarRelocators)
        {
            if (nectarRelocator != null) nectarRelocator.RelocateAll();
        }

        Debug.Log("<color=orange><b>GLOBAL SCRAMBLE COMPLETE:</b></color> All parallel arenas randomized.");
    }
}