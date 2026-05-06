using UnityEngine;
using System.Collections;

public class NectarSource : MonoBehaviour
{
    [Header("Nectar Settings")]
    public int maxNectar = 2;
    public float rechargeTime = 30.0f;
    public float nectarQuality = 1.0f;

    [Header("Current Status")]
    [SerializeField] private int currentNectar;
    [SerializeField] private bool isDepleted = false;

    [Header("Visuals")]
    private Renderer sourceRenderer;
    public Material fullMaterial;
    public Material depletedMaterial;

    void Start()
    {
        // Randomize quality
        nectarQuality = Random.Range(0.1f, 1.0f);
        currentNectar = maxNectar;
        sourceRenderer = GetComponent<Renderer>();
        UpdateVisuals();
    }

    // Attempt to take nectar; returns true if successful
    public bool HarvestNectar()
    {
        if (isDepleted) return false;

        currentNectar--;

        // If that was the last disable
        if (currentNectar <= 0)
        {
            TriggerDepletion();
        }

        return true;
    }

    // Handles the transition to an empty state and starts the timer
    private void TriggerDepletion()
    {
        if (isDepleted) return;

        Debug.Log($"{gameObject.name} is now depleted.");
        isDepleted = true;
        currentNectar = 0;

        UpdateVisuals();
        StartCoroutine(Recharge());
    }

    // Coroutine to wait out the recharge duration
    private IEnumerator Recharge()
    {
        yield return new WaitForSeconds(rechargeTime);

        Debug.Log($"{gameObject.name} has recharged!");
        currentNectar = maxNectar;
        isDepleted = false;
        UpdateVisuals();
    }

    // Getter for the depletion state
    public bool IsDepleted() => isDepleted;

    // Swaps materials based on whether nectar is available
    private void UpdateVisuals()
    {
        if (sourceRenderer == null) return;

        Material targetMat = isDepleted ? depletedMaterial : fullMaterial;
        if (targetMat != null) sourceRenderer.material = targetMat;
    }

    // Unity Event: Click the object in-game to force empty it
    void OnMouseDown()
    {
        if (!isDepleted) TriggerDepletion();
    }

    // Public method for external scripts to force depletion
    public void ManuallyDeplete() => TriggerDepletion();
}