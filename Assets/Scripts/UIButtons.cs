using UnityEngine;

public class UIButtons : MonoBehaviour
{
    // Reference to the message GameObject that will be shown/hidden
    [SerializeField] private GameObject message;

    /// <summary>
    /// Toggles the visibility of the message GameObject
    /// If the message is currently active, it will be closed
    /// If the message is currently inactive, it will be shown
    /// </summary>
    public void ShowMessage()
    {
        // Check current state of the message
        if (message.activeSelf)
            CloseMessage(); // Message is active, so close it
        else
            message.SetActive(true); // Message is inactive, so show it
    }

    /// <summary>
    /// Hides the message GameObject by setting it to inactive
    /// </summary>
    public void CloseMessage() => message.SetActive(false);
}
