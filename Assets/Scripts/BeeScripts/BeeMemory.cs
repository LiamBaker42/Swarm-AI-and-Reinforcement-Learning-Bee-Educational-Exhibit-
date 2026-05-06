using UnityEngine;
using System.Collections.Generic;

// Simple data container for a bee's mental map
public class NectarInfo
{
    public Vector3 location;
    public bool isDepleted = false;
    public float depletedUntilTime = 0f;
}

public class BeeMemory
{
    private Transform committedSource;
    private NectarInfo sourceInfo;
    private float depletedCooldown;

    public BeeMemory(float cooldown)
    {
        depletedCooldown = cooldown;
    }

    // Core ABC Logic: Committing to a source based on shared dance information
    public void UpdateMemoryFromDance(Transform sourceTf, bool dancerSaysIsDepleted)
    {
        SetSource(sourceTf, dancerSaysIsDepleted);
    }

    // Update memory based on first-hand discovery at the flower
    public void UpdateMemoryOnArrival(Transform sourceTf, bool isDepleted)
    {
        SetSource(sourceTf, isDepleted);
    }

    // Internal helper to avoid code duplication when updating source data
    private void SetSource(Transform sourceTf, bool isDepleted)
    {
        if (sourceTf == null) return;

        committedSource = sourceTf;
        sourceInfo = new NectarInfo
        {
            location = sourceTf.position,
            isDepleted = isDepleted,
            depletedUntilTime = isDepleted ? Time.time + depletedCooldown : 0
        };
    }

    // Logic to decide whether to stay committed or abandon the source (triggers scouting)
    public Transform ChooseBestNectarSource(Vector3 myPosition)
    {
        if (committedSource == null) return null;

        // Forget the source if it's currently empty and hasn't recharged yet
        if (sourceInfo.isDepleted && Time.time < sourceInfo.depletedUntilTime)
        {
            ClearMemory();
            return null;
        }

        return committedSource;
    }

    // Completely wipe the bee's current objective
    public void ClearMemory()
    {
        committedSource = null;
        sourceInfo = null;
    }

    // Accessor for the current target and its known data
    public KeyValuePair<Transform, NectarInfo> GetCommittedSource()
    {
        return new KeyValuePair<Transform, NectarInfo>(committedSource, sourceInfo);
    }
}