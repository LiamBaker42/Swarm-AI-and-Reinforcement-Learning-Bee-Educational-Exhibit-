using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ColonyManager : MonoBehaviour
{
    [Header("ABC Settings")]
    [Range(0, 1)] public float scoutRatio = 0.9f;
    public float balanceCheckInterval = 5f;

    [Header("Source Management")]
    public float qualityThreshold = 0.2f;

    private List<BeeStateManager> allBees = new List<BeeStateManager>();
    private HiveManager hiveManager;

    void Start()
    {
        hiveManager = GetComponent<HiveManager>();

        // Cache all bee references once at startup
        allBees = Object.FindObjectsByType<BeeStateManager>(FindObjectsSortMode.None).ToList();

        AssignInitialRoles();

        // Start timer to prevent colony stagnation
        InvokeRepeating(nameof(BalanceColony), balanceCheckInterval, balanceCheckInterval);
    }

    void AssignInitialRoles()
    {
        int scoutCount = Mathf.CeilToInt(allBees.Count * scoutRatio);

        for (int i = 0; i < allBees.Count; i++)
        {
            // Split colony between active explorers (Scouts) and hive waiters (Onlookers)
            bool isScout = i < scoutCount;
            allBees[i].SetState(isScout ? BeeState.SCOUTING : BeeState.DANCING);
        }
    }

    // Triggered by HiveManager when a scout returns with food data
    public void OnNewDanceRegistered()
    {
        WaggleDanceInfo bestDance = hiveManager.GetBestDance();
        if (bestDance == null) return;

        foreach (var bee in allBees)
        {
            // Only recruit bees currently idle in the hive
            if (bee.currentState == BeeState.DANCING && bee.latestStatusMessage.Contains("Waiting"))
            {
                // ABC Probability: Higher nectar quality increases recruitment chance
                if (Random.value < bestDance.quality)
                {
                    bee.RecruitToTarget(bestDance.nectarSource, bestDance.quality);
                }
            }
        }
    }

    // Ensures the colony doesn't get stuck if no sources are currently known
    public void BalanceColony()
    {
        // Count bees currently performing field tasks
        int activeBees = allBees.Count(b => b.currentState == BeeState.SCOUTING ||
                                           b.currentState == BeeState.RETURNING_TO_HIVE ||
                                           b.currentState == BeeState.FOLLOWING_DANCE);

        // If colony is idle, force one bee to scout for new sources
        if (activeBees <= 10 && allBees.Count > 0)
        {
            var onlooker = allBees.FirstOrDefault(b => b.currentState == BeeState.DANCING);
            onlooker?.SetState(BeeState.SCOUTING);
        }
    }

    // UI/Debug: Forces every bee to explore the map
    public void SetAllToScouts()
    {
        allBees.ForEach(bee => bee.SetState(BeeState.SCOUTING));
        Debug.Log("<color=yellow>Colony Override: All bees set to SCOUTING.</color>");
    }

    // UI/Debug: Forces every bee to harvest the best known source
    public void SetAllToWorkers()
    {
        WaggleDanceInfo bestDance = hiveManager.GetBestDance();

        if (bestDance != null)
        {
            allBees.ForEach(bee => bee.RecruitToTarget(bestDance.nectarSource, bestDance.quality));
            Debug.Log($"<color=green>Colony Override: All recruited to {bestDance.nectarSource.name}.</color>");
        }
        else
        {
            Debug.LogWarning("Manual Override Failed: No known nectar sources!");
        }
    }

    // Resets the colony to the default scout/onlooker distribution
    public void ResetColony()
    {
        AssignInitialRoles();
        Debug.Log("<color=cyan>Colony Reset: Roles redistributed by ratio.</color>");
    }
}
