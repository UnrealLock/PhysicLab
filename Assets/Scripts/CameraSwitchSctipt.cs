using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitchSctipt : MonoBehaviour, IClickable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Reference to the mouse pointer object
    GameObject mousePointer;
    // Collider for this object
    Collider objectCollider;
    // Necesary references for camera switching
    public Camera CameraToSwitch;
    Camera mainCamera;
    // Flag to indicate if the camera is switched
    bool isCameraSwitched = false;

    void Start()
    {
        // Initialize references
        objectCollider = GetComponent<Collider>();
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        // Control exiting the switched camera
        ExitIfPressed();
    }

    void ExitIfPressed()
    {
        // If camera is not switched, do nothing
        if (!isCameraSwitched)
            return;
        // Check for right mouse button press to exit camera view
        var mouseAction = InputSystem.GetDevice<Mouse>().rightButton.isPressed ? 1 : 0;
        if (mouseAction > 0)
        {
            // Restore the main camera
            mainCamera.gameObject.SetActive(true);
            CameraToSwitch.gameObject.SetActive(false);
            //mousePointer.SetActive(true);
            // Reset the camera switched flag
            isCameraSwitched = false;
        }
        // Update ClickManager with the active camera
        if (ClickManager.Instance != null)
        {
            ClickManager.Instance.SetActiveCamera(mainCamera);
        }
    }

    public void OnLeftClick()
    {
        // If camera is already switched, do nothing
        if (isCameraSwitched) return;
        // Switch to the specified camera
        CameraToSwitch.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        //mousePointer.SetActive(false);
        // Set the flag indicating the camera is switched
        isCameraSwitched = true;
        // Update ClickManager with the active camera
        if (ClickManager.Instance != null)
        {
            ClickManager.Instance.SetActiveCamera(CameraToSwitch);
        }
    }

    public void OnRightClick()
    {
    }
}
