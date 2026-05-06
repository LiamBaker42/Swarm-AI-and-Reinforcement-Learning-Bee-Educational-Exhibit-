using UnityEngine;
using TMPro;

public class BeeSelector : MonoBehaviour
{
    public static BeeStateManager selectedBee;

    [Header("UI Box References")]
    public TextMeshProUGUI headerBox;
    public TextMeshProUGUI sensorPod;
    public TextMeshProUGUI actionPod;
    public TextMeshProUGUI footerBox;

    private BeeAgent agent;
    private BeeStateManager stateManager;
    private BeeMemory memory;
    private Rigidbody rb;
    private RTS_Camera mainCamera;

    [Header("Visuals")]
    public LineRenderer navigationLine;
    public GameObject targetMarkerPrefab;
    private GameObject spawnedMarker;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Object.FindFirstObjectByType<RTS_Camera>();

        // Auto-find UI if the inspector references are missing
        if (headerBox == null) FindUIReferences();
        if (navigationLine != null) navigationLine.enabled = false;
    }

    private void FindUIReferences()
    {
        headerBox = GameObject.Find("HeaderBox")?.GetComponent<TextMeshProUGUI>();
        sensorPod = GameObject.Find("SensorPod")?.GetComponent<TextMeshProUGUI>();
        actionPod = GameObject.Find("ActionPod")?.GetComponent<TextMeshProUGUI>();
        footerBox = GameObject.Find("FooterBox")?.GetComponent<TextMeshProUGUI>();
    }

    // Connects the UI script to the bee's specific logic components
    public void Initialize(BeeStateManager manager, BeeAgent agent, BeeMemory memory)
    {
        this.stateManager = manager;
        this.agent = agent;
        this.memory = memory;
    }

    // Handles clicking on a bee to select it and focus the camera
    void OnMouseDown()
    {
        selectedBee = this.stateManager;
        UpdateUI();

        mainCamera?.SetFollowTarget(this.transform);
    }

    void Update()
    {
        // Only run UI/Visual logic if this specific bee is the selected one
        if (selectedBee == this.stateManager && stateManager != null)
        {
            UpdateUI();
            UpdateNavigationVisuals();
        }
        else
        {
            // Hide navigation lines and markers for unselected bees
            if (navigationLine != null) navigationLine.enabled = false;
            if (spawnedMarker != null) spawnedMarker.SetActive(false);
        }
    }

    void UpdateUI()
    {
        if (stateManager == null || agent == null) return;

        // Identity & Role: Displays name, state, and recent status
        if (headerBox)
        {
            headerBox.text = $"<color=#FFA500><size=130%><b>{gameObject.name}</b></size></color>\n" +
                             $"<size=90%>{stateManager.currentState} | {stateManager.latestStatusMessage}</size>";
        }

        // Sensor Pod: Shows raw environmental data used by ML-Agents
        if (sensorPod)
        {
            Vector3 sunDir = SolarManager.Instance.GetSunDirection();
            float dist = Vector3.Distance(transform.position, stateManager.currentMentalTarget);
            Vector3 toTarget = (stateManager.currentMentalTarget - transform.position).normalized;
            float alignment = Vector3.Dot(transform.forward, toTarget);

            sensorPod.text = $"<color=#00FFFF><b>[ SENSORS ]</b></color>\n" +
                             $"Sun: ({sunDir.x:F1}, {sunDir.z:F1})\n" +
                             $"Target Dist: {dist:F1}m\n" +
                             $"Alignment: {(alignment > 0.8f ? "<color=#00FF00>" : "<color=#FFFFFF>")}{alignment:F2}</color>";
        }

        // Action Pod: Displays physical output and current AI reward performance
        if (actionPod)
        {
            actionPod.text = $"<color=#FFD700><b>[ ACTIONS ]</b></color>\n" +
                             $"Thrust: {rb.linearVelocity.magnitude:F2} m/s\n" +
                             $"Yaw: {transform.eulerAngles.y:F0}°\n" +
                             $"Reward: <color=#00FF00>{agent.GetLastReward():F4}</color>";
        }

        // Footer: Displays ABC-specific memory and source availability
        if (footerBox)
        {
            var source = memory.GetCommittedSource();
            if (source.Key != null)
            {
                string status = source.Value.isDepleted ? "<color=red>DEPLETED</color>" : "<color=green>ACTIVE</color>";
                footerBox.text = $"<b>ABC TARGET:</b> {source.Key.name} | <b>STATUS:</b> {status}\n" +
                                 $"<size=80%><color=#AAAAAA>Exploiting source vector until exhausted.</color></size>";
            }
            else
            {
                footerBox.text = "<i>Searching... (No committed source)</i>";
            }
        }
    }

    // Draws a line in the scene view to show the bee's current destination
    void UpdateNavigationVisuals()
    {
        Vector3 target = stateManager.currentMentalTarget;
        if (navigationLine != null)
        {
            navigationLine.enabled = true;
            navigationLine.SetPosition(0, transform.position);
            navigationLine.SetPosition(1, target);
        }

        // Instantiate or move the 3D target marker
        if (targetMarkerPrefab != null)
        {
            if (spawnedMarker == null) spawnedMarker = Instantiate(targetMarkerPrefab);
            spawnedMarker.SetActive(true);
            spawnedMarker.transform.position = target + Vector3.up * 2.0f;
            spawnedMarker.transform.Rotate(Vector3.up, 100f * Time.deltaTime);
        }
    }
}