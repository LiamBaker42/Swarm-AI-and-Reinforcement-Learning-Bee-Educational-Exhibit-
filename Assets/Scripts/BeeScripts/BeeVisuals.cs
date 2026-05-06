using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum BeeState { SCOUTING, RETURNING_TO_HIVE, DANCING, FOLLOWING_DANCE }

public class BeeVisuals : MonoBehaviour
{
    [Header("Status Icon")]
    public Image statusIcon;
    public Sprite scoutSprite, danceSprite, followSprite, returnSprite;

    [Header("Trail")]
    public TrailRenderer trail;
    public Color scoutColor = Color.white;
    public Color followColor = Color.green;
    public Color returnColor = Color.yellow;

    [Header("Waggle Dance")]
    public GameObject arrowPrefab;
    public float wobbleSpeed = 30.0f;
    public float wobbleAmount = 0.1f;

    // Updates sprites and trail colors based on the current AI state
    public void SetStateVisuals(BeeState newState)
    {
        if (statusIcon == null || trail == null) return;

        statusIcon.enabled = true;
        trail.emitting = (newState != BeeState.DANCING);

        switch (newState)
        {
            case BeeState.SCOUTING:
                UpdateAppearance(scoutSprite, scoutColor);
                break;
            case BeeState.RETURNING_TO_HIVE:
                UpdateAppearance(returnSprite, returnColor);
                break;
            case BeeState.DANCING:
                statusIcon.sprite = danceSprite;
                break;
            case BeeState.FOLLOWING_DANCE:
                UpdateAppearance(followSprite, followColor);
                break;
        }
    }

    // Helper to set both sprite and trail color
    private void UpdateAppearance(Sprite sprite, Color color)
    {
        statusIcon.sprite = sprite;
        trail.startColor = color;
    }

    // Handles the wobble and directional arrow during recruitment
    public IEnumerator PerformVisualDance(Transform advertisedSource, float danceDuration, System.Action onDanceComplete)
    {
        if (advertisedSource != null)
        {
            // Create an arrow pointing toward the nectar source relative to the hive
            GameObject arrow = SpawnDanceArrow(advertisedSource);

            float startTime = Time.time;
            Vector3 startPos = transform.position;

            // Vibrate the bee left and right for the duration
            while (Time.time < startTime + danceDuration)
            {
                float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
                transform.position = startPos + (transform.right * wobble);
                yield return null;
            }

            if (arrow != null) Destroy(arrow);
            transform.position = startPos;
        }
        else
        {
            // If no source is known, the bee just waits silently
            yield return new WaitForSeconds(danceDuration);
        }

        // Trigger the callback to let the StateManager know the bee is available again
        onDanceComplete?.Invoke();
    }

    // Instantiates and orients the UI/World arrow for the waggle dance
    private GameObject SpawnDanceArrow(Transform source)
    {
        if (arrowPrefab == null) return null;

        GameObject arrow = Instantiate(arrowPrefab, transform.position + (Vector3.up * 2f), Quaternion.identity);
        Vector3 targetDir = (source.position - transform.position);
        targetDir.y = 0;

        arrow.transform.rotation = Quaternion.LookRotation(targetDir);
        arrow.transform.Rotate(90, 0, 0);
        return arrow;
    }
}