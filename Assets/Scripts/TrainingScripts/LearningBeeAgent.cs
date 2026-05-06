using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class LearningBeeAgent : Agent
{
    [Header("Movement Settings")]
    public float moveForce = 5f;
    public float yawSpeed = 200f;

    [Header("Scene References")]
    public Transform hiveTransform;

    private HiveManager hiveManager;
    private Rigidbody rb;
    private WaggleDanceInfo currentLesson;
    private Vector3 correctNectarTarget;
    private float previousDistance;
    private Vector3 mentalTargetPos;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        hiveManager = transform.parent.GetComponentInChildren<HiveManager>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }

    void Update()
    {
        if (currentLesson != null)
        {
            Debug.DrawLine(transform.position, correctNectarTarget, Color.green);
            Debug.DrawRay(transform.position, SolarManager.Instance.GetSunDirection() * 3f, Color.yellow);
            DrawMentalTargetGizmo(mentalTargetPos, Color.blue);
        }
    }

    void DrawMentalTargetGizmo(Vector3 pos, Color color)
    {
        float size = 0.5f;
        Debug.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size, color);
        Debug.DrawLine(pos + Vector3.up * size, pos + Vector3.down * size, color);
        Debug.DrawLine(pos + Vector3.forward * size, pos + Vector3.back * size, color);
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentLesson = hiveManager.GetBestDance();

        if (currentLesson != null)
        {
            correctNectarTarget = currentLesson.targetPosition;
            Vector3 spawnPos = hiveTransform.position + Random.insideUnitSphere * 1.5f;
            spawnPos.y = hiveTransform.position.y;
            transform.position = spawnPos;
            transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            previousDistance = Vector3.Distance(transform.position, correctNectarTarget);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (currentLesson == null)
        {
            sensor.AddObservation(new float[9]);
            return;
        }

        // Use the sun direction from the time the dance was posted
        Vector3 sunAtDanceTime = currentLesson.sunDirAtTimeOfDance;
        Quaternion danceRot = Quaternion.Euler(0, currentLesson.angleRelativeToSun, 0);
        Vector3 targetDirFromHive = (danceRot * sunAtDanceTime).normalized;

        mentalTargetPos = hiveTransform.position + (targetDirFromHive * currentLesson.distance);

        Vector3 currentSunDir = SolarManager.Instance.GetSunDirection();
        Vector3 vectorToMentalTarget = mentalTargetPos - transform.position;
        float distToMentalTarget = vectorToMentalTarget.magnitude;

        // Dot Product tells the bee if it's facing the target
        float dotToMentalTarget = Vector3.Dot(transform.forward, vectorToMentalTarget.normalized);

        Vector3 dirToTarget = (mentalTargetPos - hiveTransform.position).normalized;
        float currentRequiredAngle = Vector3.SignedAngle(currentSunDir, dirToTarget, Vector3.up);

        // --- OBSERVATIONS (9 Total) ---
        sensor.AddObservation(distToMentalTarget / 60f); // 1
        sensor.AddObservation(dotToMentalTarget); // 2
        sensor.AddObservation(currentSunDir.x); // 3
        sensor.AddObservation(currentSunDir.z); // 4
        sensor.AddObservation(transform.forward.x); // 5
        sensor.AddObservation(transform.forward.z); // 6
        sensor.AddObservation(rb.linearVelocity.magnitude / moveForce); // 7
        sensor.AddObservation(currentRequiredAngle / 180f); // 8
        sensor.AddObservation(currentLesson.distance / 60f); // 9
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var vectorAction = actions.ContinuousActions;
        transform.Rotate(Vector3.up, vectorAction[1] * yawSpeed * Time.fixedDeltaTime);
        float forwardInput = Mathf.Clamp(vectorAction[0], 0f, 1f);
        rb.linearVelocity = transform.forward * forwardInput * moveForce;

        float currentDistance = Vector3.Distance(transform.position, correctNectarTarget);
        // Only reward if the bee is closer than its previous best distance
        if (currentDistance < previousDistance)
        {
            AddReward((previousDistance - currentDistance) * 0.01f);

            previousDistance = currentDistance;
        }
        AddReward(-1f / MaxStep);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NectarSource") && Vector3.Distance(other.transform.position, correctNectarTarget) < 0.5f)
        {
            SetReward(1.0f);
            EndEpisode();
        }
        else if (other.CompareTag("boundary"))
        {
            AddReward(-1.0f);
            EndEpisode();
        }
        else if (other.CompareTag("Obstacle"))
        {
            AddReward(-0.2f); // The negative feedback for the Wall ray-hits
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;

        // Action 0: Forward/Backward (Vertical Axis)
        continuousActionsOut[0] = Input.GetAxis("Vertical");

        // Action 1: Rotation (Horizontal Axis)
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
}