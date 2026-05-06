using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class BeeAgent : Agent
{
    private BeeStateManager state;
    private Rigidbody rb;
    private float previousDistance;
    private float lastActionReward;

    [HideInInspector] public float obsTargetDist;
    [HideInInspector] public float obsTargetAlignment;
    [HideInInspector] public float obsSunAngle;

    // Initialize references once at the start
    public override void Initialize()
    {
        state = GetComponent<BeeStateManager>();
        rb = GetComponent<Rigidbody>();
    }

    // Reset tracking variables at the start of every training episode
    public override void OnEpisodeBegin()
    {
        if (state != null)
        {
            previousDistance = Vector3.Distance(transform.position, state.currentMentalTarget);
        }
        lastActionReward = 0f;
    }

    // Pass environment data to the neural network
    public override void CollectObservations(VectorSensor sensor)
    {
        // Zero out observations if the bee is in a non-navigational state
        if (state.currentState == BeeState.DANCING)
        {
            sensor.AddObservation(new float[9]);
            return;
        }

        Vector3 currentSunDir = SolarManager.Instance.GetSunDirection();
        Vector3 vectorToTarget = state.currentMentalTarget - transform.position;
        Vector3 dirFromOrigin = (state.currentMentalTarget - state.currentReferenceOrigin).normalized;

        // Calculate the angle relative to the sun for navigation
        float requiredAngle = Vector3.SignedAngle(currentSunDir, dirFromOrigin, Vector3.up);

        obsTargetDist = vectorToTarget.magnitude / 60f;
        obsTargetAlignment = Vector3.Dot(transform.forward, vectorToTarget.normalized);
        obsSunAngle = requiredAngle / 180f;

        // 9 total observations
        sensor.AddObservation(obsTargetDist);
        sensor.AddObservation(obsTargetAlignment);
        sensor.AddObservation(currentSunDir.x);
        sensor.AddObservation(currentSunDir.z);
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);
        sensor.AddObservation(rb.linearVelocity.magnitude / state.moveForce);
        sensor.AddObservation(obsSunAngle);
        sensor.AddObservation(Vector3.Distance(state.currentReferenceOrigin, state.currentMentalTarget) / 60f);
    }

    // Execute movement based on neural network output and apply rewards
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (state.currentState == BeeState.DANCING) return;

        var vectorAction = actions.ContinuousActions;
        // Rotation (Yaw)
        transform.Rotate(Vector3.up, vectorAction[1] * state.yawSpeed * Time.fixedDeltaTime);

        // Forward movement
        float forwardInput = Mathf.Clamp(vectorAction[0], 0f, 1f);
        rb.linearVelocity = transform.forward * forwardInput * state.moveForce;

        float currentDist = Vector3.Distance(transform.position, state.currentMentalTarget);
        float rewardThisStep = 0f;

        // Reward progress toward target
        if (currentDist < previousDistance)
        {
            state.latestStatusMessage = "<color=#00FF00>(+) Moving Toward Target</color>";
            rewardThisStep = (previousDistance - currentDist) * 0.01f;
            previousDistance = currentDist;
        }
        else
        {
            state.latestStatusMessage = "<color=#FF6666>(-) Moving Away From Target</color>";
        }

        // Time penalty to encourage efficiency
        rewardThisStep -= (1f / MaxStep);
        AddReward(rewardThisStep);
        lastActionReward = rewardThisStep;
    }

    // Penalty for colliding with obstacles
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            state.latestStatusMessage = "<color=red>!!! OBSTACLE COLLISION !!!</color>";

            AddReward(-0.05f);
        }
    }

    // Continuous penalty for staying in contact with obstacles
    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            state.latestStatusMessage = "<color=red>!!! OBSTACLE COLLISION !!!</color>";
            AddReward(-0.01f);
        }
    }

    public float GetLastReward() => lastActionReward;
    public float ResetDistance() => previousDistance = Vector3.Distance(transform.position, state.currentMentalTarget);

    // Manual control for testing without AI
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}