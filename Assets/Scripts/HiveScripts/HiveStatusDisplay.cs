using UnityEngine;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public class HiveStatusDisplay : MonoBehaviour
{
    public HiveManager hiveManager;
    public TextMeshProUGUI infoText;

    [Header("Pagination Settings")]
    public float timePerPage = 5.0f;
    private float timer;
    private int currentPageIndex = 0;

    private StringBuilder sb = new StringBuilder();

    void Update()
    { 
    
        if (hiveManager == null || infoText == null) return;

        // ABC Logic: Sort dances by fitness so the most attractive sources are shown first
        List<WaggleDanceInfo> dances = hiveManager.GetRankedDances();

        // Handle Empty State if no scouts have returned yet
        if (dances.Count == 0)
        {
            currentPageIndex = 0;
            ShowEmptyState();
            return;
        }

        // Prevent index errors if a dance expires while viewing it
        if (currentPageIndex >= dances.Count) currentPageIndex = 0;

        // Cycle through pages based on the timer
        timer += Time.deltaTime;
        if (timer >= timePerPage)
        {
            timer = 0;
            currentPageIndex = (currentPageIndex + 1) % dances.Count;
        }

        // Update the text display
        RenderPage(dances[currentPageIndex], dances.Count);
    }

    void RenderPage(WaggleDanceInfo dance, int totalCount)
    {
        sb.Clear();

        // Calculate fitness breakdown using the HiveManager's specific weightings
        float qualityPart = dance.quality;
        float distPenalty = (dance.distance / 60f) * hiveManager.distanceWeight;
        float totalScore = qualityPart - distPenalty;
        float remainingTime = Mathf.Max(0, hiveManager.danceDuration - (Time.time - dance.timestamp));

        // Display Header and Rank info
        sb.AppendLine($"<size=120%><b>RANK #{currentPageIndex + 1} OF {totalCount}</b></size>");
        sb.AppendLine("<color=#888888>----------------------------</color>");

        // Show the Nectar Source name 
        sb.AppendLine($"<size=120%><color=#FFA500><b>{dance.nectarSource.name}</b></color></size>");

        // ABC Decision Data: Why the bees are choosing this source
        sb.AppendLine($"<b>Quality:</b> {qualityPart:P0}");
        sb.AppendLine($"<b>Dist. Penalty:</b> -{distPenalty:F2}");
        sb.AppendLine($"<color=#00FF00><b>Fitness Score: {totalScore:F2}</b></color>");

        sb.AppendLine("<color=#888888>----------------------------</color>");

        // Technical solar-navigation data
        sb.AppendLine($"<size=90%>Distance: {dance.distance:F1}m</size>");
        sb.AppendLine($"<size=90%>Sun Angle: {dance.angleRelativeToSun:F1}°</size>");
        sb.AppendLine($"<size=90%><color=#FF6666>Expires in: {remainingTime:F0}s</color></size>");

        // Visual timer bar to show when the page will flip
        float progress = 1f - (timer / timePerPage);
        int barLength = Mathf.RoundToInt(progress * 10);
        sb.Append("\n<size=80%><color=#444444>[");
        sb.Append('=', barLength);
        sb.Append('.', 10 - barLength);
        sb.AppendLine("]</color></size>");

        infoText.text = sb.ToString();
    }

    void ShowEmptyState()
    {
        infoText.text = "<size=120%><b>HIVE KNOWLEDGE (0)</b></size>\n" +
                        "<color=#888888>----------------------------</color>\n\n" +
                        "<i>Scouts searching for nectar...</i>";
    }
}