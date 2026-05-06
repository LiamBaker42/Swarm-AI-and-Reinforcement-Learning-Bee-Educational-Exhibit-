using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

//Data container for a single waggle dance. Maintains the solar vector at the moment of discovery for the RL agent.
public class WaggleDanceInfo
{
    public Transform nectarSource;
    public Vector3 targetPosition;
    public float distance;
    public float angleRelativeToSun;
    public Vector3 sunDirAtTimeOfDance;
    public float quality;
    public float timestamp;

    public WaggleDanceInfo(Transform source, Vector3 hivePos, Vector3 sunDir, float q)
    {
        nectarSource = source;
        targetPosition = source.position;
        quality = q;
        timestamp = Time.time;
        sunDirAtTimeOfDance = sunDir;

        Vector3 directionToFlower = (targetPosition - hivePos).normalized;
        directionToFlower.y = 0;

        // The core Waggle angle: how far to turn away from the sun's current position
        angleRelativeToSun = Vector3.SignedAngle(sunDir, directionToFlower, Vector3.up);
        distance = Vector3.Distance(hivePos, targetPosition);
    }
}

public class HiveManager : MonoBehaviour
{
    [Header("Hive Resources")]
    public float maxNectar = 100f;
    [SerializeField] private float currentNectar = 0f;
    public Slider nectarGauge;

    [Header("Dance Floor")]
    public List<WaggleDanceInfo> activeDances = new List<WaggleDanceInfo>();
    public float danceDuration = 30f;

    [Header("ABC Ranking Settings")]
    public float distanceWeight = 0.16f;
    [Range(0, 1)] public float scoutRatio = 0.9f;
    public float balanceCheckInterval = 5f;

    private List<BeeStateManager> allBees = new List<BeeStateManager>();

    void Start()
    {
        // Cache colony members and assign initial exploration/waiting roles
        allBees = Object.FindObjectsByType<BeeStateManager>(FindObjectsSortMode.None).ToList();
        AssignInitialRoles();

        InvokeRepeating(nameof(BalanceColony), balanceCheckInterval, balanceCheckInterval);
    }

    void Update()
    {
        // Expire old dances
        activeDances.RemoveAll(d => Time.time - d.timestamp > danceDuration);
    }

    // Called by bees to announce a find; triggers the Onlooker recruitment phase
    public void RegisterWaggleDance(Transform source, float quality)
    {
        if (source == null) return;

        // Prevent duplicate entries for the same flower
        activeDances.RemoveAll(d => d.nectarSource == source);

        Vector3 currentSunDir = SolarManager.Instance.GetSunDirection();
        WaggleDanceInfo newDance = new WaggleDanceInfo(source, transform.position, currentSunDir, quality);

        activeDances.Add(newDance);
        RecruitOnlookers();
    }

    public void AddNectar(float amount)
    {
        currentNectar = Mathf.Min(currentNectar + amount, maxNectar);
        if (nectarGauge != null) nectarGauge.value = currentNectar / maxNectar;
    }

    public float GetNectarAmount() => currentNectar;

    // Standard ABC fitness: quality vs. cost (distance)
    public WaggleDanceInfo GetBestDance()
    {
        var ranked = GetRankedDances();
        return (ranked.Count == 0) ? null : ranked[0];
    }

    // Sorts sources so onlookers choose the most efficient option
    public List<WaggleDanceInfo> GetRankedDances()
    {
        return activeDances
            .OrderByDescending(d => d.quality - ((d.distance / 60f) * distanceWeight))
            .ToList();
    }

    void AssignInitialRoles()
    {
        int scoutCount = Mathf.CeilToInt(allBees.Count * scoutRatio);
        for (int i = 0; i < allBees.Count; i++)
        {
            allBees[i].SetState(i < scoutCount ? BeeState.SCOUTING : BeeState.DANCING);
        }
    }

    // Recruitment: Onlookers follow the best dance based on a quality-probability check
    private void RecruitOnlookers()
    {
        WaggleDanceInfo bestDance = GetBestDance();
        if (bestDance == null) return;

        foreach (var bee in allBees)
        {
            if (bee.currentState == BeeState.DANCING && bee.latestStatusMessage.Contains("Waiting"))
            {
                if (Random.value < bestDance.quality)
                {
                    bee.RecruitToTarget(bestDance.nectarSource, bestDance.quality);
                }
            }
        }
    }

    // Failsafe: if the whole colony is idle, force a scout to go find food
    void BalanceColony()
    {
        bool anyActive = allBees.Any(b => b.currentState == BeeState.SCOUTING ||
                                         b.currentState == BeeState.RETURNING_TO_HIVE ||
                                         b.currentState == BeeState.FOLLOWING_DANCE);

        if (!anyActive && allBees.Count > 0)
        {
            allBees.FirstOrDefault(b => b.currentState == BeeState.DANCING)?.SetState(BeeState.SCOUTING);
        }
    }
}