using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSpeedController : MonoBehaviour
{
    public Slider speedSlider;
    public TextMeshProUGUI speedText;

    private float lastSpeed = 1.0f;
    private bool isPaused = false;

    void Start()
    {
        speedSlider.value = Time.timeScale;
        UpdateUI(Time.timeScale);

        speedSlider.onValueChanged.AddListener(SetGameSpeed);
    }

    // Called by the Slider
    public void SetGameSpeed(float speed)
    {
        if (speed > 0)
        {
            Time.timeScale = speed;
            lastSpeed = speed; 
            isPaused = false;
        }
        else
        {
            Time.timeScale = 0;
            isPaused = true;
        }
        UpdateUI(speed);
    }

    // Called by the Button
    public void TogglePause()
    {
        if (isPaused)
        {
            // Resume to last known speed
            SetGameSpeed(lastSpeed);
            speedSlider.value = lastSpeed;
        }
        else
        {
            // Save current speed and pause
            lastSpeed = Time.timeScale > 0 ? Time.timeScale : 1.0f;
            SetGameSpeed(0f);
            speedSlider.value = 0f;
        }
    }

    void UpdateUI(float speed)
    {
        if (speedText != null)
            speedText.text = speed <= 0 ? "Paused" : "Speed: " + speed.ToString("F1") + "x";
    }
}