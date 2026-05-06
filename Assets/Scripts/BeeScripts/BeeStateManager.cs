using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BeeStateManager : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 5f;
    public float yawSpeed = 200f;

    [Header("Core References")]
    public Transform hiveTransform;
    public float danceDuration = 10.0f;
    public float depletedCooldown = 60.0f;

    [Header("Arena Bounds (For Scouting)")]
    public Transform cornerTopLeft;
    public Transform cornerTopRight;
    public Transform cornerBottomLeft;
    public Transform cornerBottomRight;

    [Header("Timeout Settings")]
    public float maxTimePerTarget = 15f;
    private float targetStartTime;

    [HideInInspector] public BeeState currentState;
    [HideInInspector] public Vector3 currentMentalTarget;
    [HideInInspector] public Vector3 currentReferenceOrigin;
    [HideInInspector] public string latestStatusMessage = "Idle";

    private Vector3 absoluteWorldDirection;
    private float targetDistance;
    private bool isFollowingSunCompass;

    private HiveManager hiveManager;
    private Rigidbody rb;
    private BeeVisuals visuals;
    private BeeSelector selector;
    private BeeMemory memory;
    private BeeAgent agent;
    private BeeAnimationController animController;

    private float nextScoutChangeTime;
    private Transform advertisedSourceTransform;
    private bool advertisedSourceIsDepleted;
    private float lastHarvestedQuality;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        visuals = GetComponent<BeeVisuals>();
        selector = GetComponent<BeeSelector>();
        animController = GetComponent<BeeAnimationController>();
        agent = GetComponent<BeeAgent>();
        hiveManager = Object.FindFirstObjectByType<HiveManager>();

        memory = new BeeMemory(depletedCooldown);
        selector?.Initialize(this, agent, memory);

        // Standardize physics for 2D plane movement
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }

    // Central hub for switching bee behavior and updating tags/animations
    public void SetState(BeeState newState)
    {
        currentState = newState;
        isFollowingSunCompass = (newState == BeeState.FOLLOWING_DANCE);

        // Update tags for filtering in the ColonyManager/Hive logic
        gameObject.tag = newState == BeeState.SCOUTING ? "Scout" :
                         newState == BeeState.FOLLOWING_DANCE ? "Worker" : "Bee";

        visuals?.SetStateVisuals(newState);
        animController?.UpdateAnimationState(newState, rb.linearVelocity.magnitude);

        switch (currentState)
        {
            case BeeState.SCOUTING:
                animController?.TriggerTakeoff();
                currentReferenceOrigin = hiveTransform.position;
                PickNewScoutTarget();
                break;

            case BeeState.RETURNING_TO_HIVE:
                currentMentalTarget = hiveTransform.position;
                currentReferenceOrigin = advertisedSourceTransform ? advertisedSourceTransform.position : transform.position;
                agent?.ResetDistance();
                break;

            case BeeState.DANCING:
                // Stop movement and inform the hive about the discovered food source
                rb.linearVelocity = Vector3.zero;
                animController?.TriggerLanding();
                if (advertisedSourceTransform != null && !advertisedSourceIsDepleted)
                {
                    hiveManager?.RegisterWaggleDance(advertisedSourceTransform, lastHarvestedQuality);
                }
                StartCoroutine(ExecuteDanceSequence());
                break;

            case BeeState.FOLLOWING_DANCE:
                animController?.TriggerTakeoff();
                targetStartTime = Time.time;
                break;
        }
    }

    // Handles the duration of the waggle dance before the bee becomes an onlooker again
    private IEnumerator ExecuteDanceSequence()
    {
        latestStatusMessage = advertisedSourceTransform != null ? "Performing Waggle Dance" : "Waiting in Hive";

        if (visuals != null)
        {
            yield return StartCoroutine(visuals.PerformVisualDance(advertisedSourceTransform, danceDuration, null));
        }
        else
        {
            yield return new WaitForSeconds(danceDuration);
        }

        advertisedSourceTransform = null;
        latestStatusMessage = "Waiting in Hive (Onlooker)";
    }

    // ABC Logic: Stores target info and triggers the flight to the flower
    public void RecruitToTarget(Transform source, float quality)
    {
        advertisedSourceTransform = source;
        absoluteWorldDirection = (source.position - hiveTransform.position).normalized;
        targetDistance = Vector3.Distance(hiveTransform.position, source.position);

        memory.UpdateMemoryFromDance(source, quality < 0.1f);
        SetState(BeeState.FOLLOWING_DANCE);
    }

    // Randomly selects a point within defined arena corners
    public void PickNewScoutTarget()
    {
        if (cornerTopLeft && cornerTopRight)
        {
            float randomX = Random.Range(cornerTopLeft.position.x, cornerTopRight.position.x);
            float randomZ = Random.Range(cornerBottomLeft.position.z, cornerTopLeft.position.z);
            currentMentalTarget = new Vector3(randomX, transform.position.y, randomZ);
            agent?.ResetDistance();
        }
        else
        {
            currentMentalTarget = hiveTransform.position + (Random.insideUnitSphere * 20f);
        }
        nextScoutChangeTime = Time.time + Random.Range(5f, 10f);
    }

    void Update()
    {
        if (currentState == BeeState.SCOUTING && Time.time >= nextScoutChangeTime)
            PickNewScoutTarget();

        // Sun-Compass Navigation: Recalculates target position as the sun moves
        if (isFollowingSunCompass && currentState == BeeState.FOLLOWING_DANCE)
        {
            Vector3 currentSunDir = SolarManager.Instance.GetSunDirection();
            float liveDanceAngle = Vector3.SignedAngle(currentSunDir, absoluteWorldDirection, Vector3.up);
            Vector3 dynamicDir = Quaternion.AngleAxis(liveDanceAngle, Vector3.up) * currentSunDir;
            currentMentalTarget = hiveTransform.position + (dynamicDir * targetDistance);
            agent?.ResetDistance();
            if (Time.time - targetStartTime > maxTimePerTarget)
            {
                HandleTargetTimeout();
            }
        }
    }

    private void HandleTargetTimeout()
    {
        latestStatusMessage = "<color=red>Target Lost/Timed Out</color>";
        memory.ClearMemory();
        SetState(BeeState.SCOUTING);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle returning home to deposit nectar
        if (currentState == BeeState.RETURNING_TO_HIVE && other.CompareTag("Hive"))
        {
            hiveManager?.AddNectar(1);
            agent?.SetReward(1.0f);
            SetState(BeeState.DANCING);
        }
        // Handle reaching a flower
        else if (other.CompareTag("NectarSource"))
        {
            NectarSource source = other.GetComponent<NectarSource>();
            if (source == null) return;

            // Workers only harvest their specifically assigned source
            if (gameObject.CompareTag("Scout") || (gameObject.CompareTag("Worker") && other.transform == advertisedSourceTransform))
            {
                HandleNectarHarvest(source);
            }
        }
    }

    private void HandleNectarHarvest(NectarSource source)
    {
        if (!source.HarvestNectar())
        {
            memory.UpdateMemoryOnArrival(source.transform, true);
            SetState(BeeState.SCOUTING);
            return;
        }

        // Successfully harvested: track quality and return to hive
        lastHarvestedQuality = source.nectarQuality;
        advertisedSourceTransform = source.transform;
        advertisedSourceIsDepleted = false;
        memory.UpdateMemoryOnArrival(source.transform, false);
        agent?.SetReward(1.0f);
        SetState(BeeState.RETURNING_TO_HIVE);
    }
}