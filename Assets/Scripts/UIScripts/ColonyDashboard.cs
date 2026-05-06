using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class ColonyDashboard : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI dashboardText;

    [Header("Static Population")]
    [Tooltip("Drag all your Bee GameObjects here from the hierarchy.")]
    public List<BeeStateManager> bees = new List<BeeStateManager>();

    [Header("Update Settings")]
    public float updateFrequency = 0.5f;

    private void Start()
    {
        if (bees.Count > 0)
        {
            InvokeRepeating(nameof(UpdateDashboard), 0.5f, updateFrequency);
        }
        else
        {
            Debug.LogWarning("ColonyDashboard: No bees assigned! Drag bees into the Inspector list.");
        }
    }

    void UpdateDashboard()
    {
        if (dashboardText == null || bees.Count == 0) return;

        // ABC Role Counts
        int scouts = 0;
        int workers = 0;
        int onlookers = 0;
        int carryingNectar = 0;

        foreach (var bee in bees)
        {
            if (bee == null) continue;

            if (bee.currentState == BeeState.SCOUTING) scouts++;
            else if (bee.currentState == BeeState.FOLLOWING_DANCE) workers++;
            else if (bee.currentState == BeeState.RETURNING_TO_HIVE)
            {
                workers++;
                carryingNectar++; // These bees are actively contributing to efficiency
            }
            else if (bee.currentState == BeeState.DANCING) onlookers++;
        }

        // Efficiency Calculation
        float efficiency = (float)carryingNectar / bees.Count * 100f;

        // Build the Dashboard String
        StringBuilder sb = new StringBuilder();

        // Title & Header
        sb.AppendLine("<color=#FFA500><size=130%><b>COLONY CONTROL CENTER</b></size></color>");
        sb.AppendLine("<size=80%><color=#888888>Artificial Bee Colony (ABC) Algorithm Monitor</color></size>");
        sb.AppendLine("--------------------------------------------------");

        // Explaining the ABC Assignment Logic
        sb.AppendLine("<b><color=#FFFF00>ROLE ASSIGNMENT LOGIC:</color></b>");
        sb.AppendLine("<size=85%><color=#CCCCCC>Roles are assigned to balance exploration vs exploitation.</color>");
        sb.AppendLine("<color=#CCCCCC>Bees switch to 'Scout' if a source is empty or to 'Worker'</color>");
        sb.AppendLine("<color=#CCCCCC>if they witness a high-quality dance on the hive floor.</color></size>");
        sb.AppendLine("");

        // The Live Numbers
        sb.AppendLine($"<color=#FFFFFF>Total Population:</color> <b>{bees.Count}</b>");
        sb.AppendLine($"<color=#32CD32>Scouts (Explorers):</color> <b>{scouts}</b> <size=80%>(Searching for new sources)</size>");
        sb.AppendLine($"<color=#1E90FF>Workers (Exploiters):</color> <b>{workers}</b> <size=80%>(Harvesting known nectar)</size>");
        sb.AppendLine($"<color=#FFD700>Onlookers (Waiting):</color> <b>{onlookers}</b> <size=80%>(Watching dances to decide)</size>");

        sb.AppendLine("--------------------------------------------------");

        // Explaining Efficiency
        sb.AppendLine("<b><color=#FFFF00>HIVE EFFICIENCY CALCULATION:</color></b>");
        sb.AppendLine("<size=85%><color=#CCCCCC>Efficiency = (Bees carrying nectar / Total Bees) * 100</color>");
        sb.AppendLine("<color=#CCCCCC>Higher % means the colony has found and is actively</color>");
        sb.AppendLine("<color=#CCCCCC>draining high-quality nectar sources.</color></size>");
        sb.AppendLine("");

        // The Final Readout
        string effColor = efficiency > 50 ? "#00FF00" : "#FF6666";
        sb.AppendLine($"<color=#00FFFF>Current Efficiency:</color> <color={effColor}><b>{efficiency:F1}%</b></color>");

        dashboardText.text = sb.ToString();
    }
}