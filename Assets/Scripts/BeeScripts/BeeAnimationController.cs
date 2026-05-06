using UnityEngine;

public class BeeAnimationController : MonoBehaviour
{
    private Animator animator;
    private AudioSource buzzSound;
    private bool isInitialized = false;
    private BeeState lastRequestedState = BeeState.SCOUTING;

    void Awake()
    {
        animator = GetComponent<Animator>();
        buzzSound = GetComponent<AudioSource>();
    }

    void Start()
    {
        isInitialized = true;

        // Ensure the bee starts with a flying motion
        TriggerTakeoff();
        UpdateAnimationState(lastRequestedState, 0f);
    }

    // sync visual state with the Bee's current logic
    public void UpdateAnimationState(BeeState state, float speed)
    {
        lastRequestedState = state;
        if (!isInitialized || animator == null) return;

        switch (state)
        {
            case BeeState.SCOUTING:
            case BeeState.FOLLOWING_DANCE:
            case BeeState.RETURNING_TO_HIVE:
                // Set animation parameters for active flight
                SetAnimBools(isFlying: true, isWalking: false, isIdle: false, flyInPlace: false);
                if (buzzSound) buzzSound.enabled = true;
                break;

            case BeeState.DANCING:
                // Special case for the waggle dance or hive waiting
                SetAnimBools(isFlying: false, isWalking: false, isIdle: false, flyInPlace: false);
                if (buzzSound) buzzSound.enabled = true;
                break;

            default:
                // Default to grounded/idle behavior
                SetAnimBools(isFlying: false, isWalking: true, isIdle: true, flyInPlace: false);
                if (buzzSound) buzzSound.enabled = false;
                break;
        }
    }

    // Helper method to reduce repetitive SetBool calls
    private void SetAnimBools(bool isFlying, bool isWalking, bool isIdle, bool flyInPlace)
    {
        animator.SetBool("fly", isFlying);
        animator.SetBool("walk", isWalking);
        animator.SetBool("idle", isIdle);
        animator.SetBool("flyinplace", flyInPlace);
    }

    // Triggered when a bee leaves the hive or a flower
    public void TriggerTakeoff()
    {
        if (!isInitialized || animator == null) return;
        animator.SetBool("takeoff", true);
        animator.SetBool("landing", false);
    }

    // Triggered when a bee reaches the hive or a flower
    public void TriggerLanding()
    {
        if (!isInitialized || animator == null) return;
        animator.SetBool("landing", true);
        animator.SetBool("takeoff", false);
    }
}