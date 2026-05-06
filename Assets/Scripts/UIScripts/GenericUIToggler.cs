using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UIToggleGroup
{
    public string groupName;       
    public GameObject panel;       
    public bool startActive = false; 
}

public class GenericUIToggler : MonoBehaviour
{
    [Header("UI Panels to Manage")]
    public List<UIToggleGroup> toggleGroups = new List<UIToggleGroup>();

    void Start()
    {
        // Set initial visibility based on startActive settings
        foreach (var group in toggleGroups)
        {
            if (group.panel != null)
            {
                group.panel.SetActive(group.startActive);
            }
        }
    }

    /// Toggles a panel by its index in the list.
    public void TogglePanel(int index)
    {
        if (index >= 0 && index < toggleGroups.Count)
        {
            GameObject panel = toggleGroups[index].panel;
            if (panel != null)
            {
                bool currentState = panel.activeSelf;
                panel.SetActive(!currentState);
            }
        }
    }

    /// Specifically sets a panel to active or inactive.
    public void SetPanelActive(int index, bool state)
    {
        if (index >= 0 && index < toggleGroups.Count)
        {
            toggleGroups[index].panel?.SetActive(state);
        }
    }
}