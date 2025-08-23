// FILE: UIPanelCloser.cs
// PURPOSE: A simple, reusable script to close a designated UI panel.
using UnityEngine;

public class UIPanelCloser : MonoBehaviour
{
    // Assign the UI Panel you want this button to close in the Inspector.
    public GameObject panelToClose;

    /// <summary>
    /// This public function can be called by a UI Button's OnClick() event.
    /// </summary>
    public void ClosePanel()
    {
        if (panelToClose != null)
        {
            // Deactivates the assigned panel, hiding it from view.
            panelToClose.SetActive(false);
            Debug.Log("Closed panel: " + panelToClose.name);
        }
        else
        {
            Debug.LogError("No panel has been assigned to be closed!", this.gameObject);
        }
    }
}
