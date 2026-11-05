using UnityEngine;
using UnityEngine.InputSystem;

public class PcCameraSwitcher : MonoBehaviour, IClickable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Reference to the camera that will be switched to and the main camera
    public Camera CameraToSwitch;
    Camera mainCamera;
    bool isCameraSwitched = false;
    Canvas screenCanvas;
    public Canvas LampCanvas;
    public GameObject ButtonExit;

    void Start()
    {
        // Initialize the main camera reference and screen canvas
        mainCamera = Camera.main;
        screenCanvas = GameObject.FindGameObjectWithTag("Screen").transform.GetChild(0).GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check for right mouse button click to exit the camera view
        var mouseAction = InputSystem.GetDevice<Mouse>().rightButton.isPressed ? 1 : 0;
        if (mouseAction > 0)
        {
            Exit();
        }
    }

    public void Exit()
    {
        // Exit the camera view and switch back to the main camera
        if (!isCameraSwitched)
            return;
        screenCanvas.worldCamera = mainCamera;
        mainCamera.gameObject.SetActive(true);
        CameraToSwitch.gameObject.SetActive(false);
        if (CameraToSwitch.name == "SideLampCamera") LampCanvas.gameObject.SetActive(false);
        if (CameraToSwitch.name == "PcCamera") ButtonExit.gameObject.SetActive(false);
        isCameraSwitched = false;
    }

    public void OnLeftClick()
    {
        // Switch to the specified camera and set it as the world camera for the screen canvas
        if (CameraToSwitch.name == "SideLampCamera")
        {
            screenCanvas.worldCamera = LampCanvas.worldCamera;
            LampCanvas.gameObject.SetActive(true);
        }
        else
        {
            screenCanvas.worldCamera = CameraToSwitch;
        }
        CameraToSwitch.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        isCameraSwitched = true;
        if (CameraToSwitch.name == "PcCamera") ButtonExit.gameObject.SetActive(true);
    }

    public void OnRightClick()
    {
    }
}