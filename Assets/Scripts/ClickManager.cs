using UnityEngine;

// Interface defining click behavior for objects
public interface IClickable
{
    void OnLeftClick();
    void OnRightClick();
}

// Manager class responsible for handling click input and detecting IClickable objects
public class ClickManager : MonoBehaviour
{
    // Singleton pattern to ensure only one instance exists
    private static ClickManager instance;
    public static ClickManager Instance => instance;

    // Reference to the camera used for raycasting (for multiple camera setups)
    private Camera currentActiveCamera;

    private void Awake()
    {
        // Singleton initialization - destroy duplicate instances
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Initialize with the main camera by default
        SetActiveCamera(Camera.main);
    }

    // Method to explicitly set which camera should be used for raycasting
    public void SetActiveCamera(Camera newCamera)
    {
        // Prevent unnecessary camera reassignment
        if (newCamera == null || newCamera == currentActiveCamera) return;

        currentActiveCamera = newCamera;
    }

    // Utility method to automatically find and set an active camera
    // Useful for dynamic camera setups or scene transitions
    public void FindAndSetActiveCamera()
    {
        Camera activeCam = null;
        Camera[] cameras = FindObjectsOfType<Camera>();

        // Find the first active and enabled camera in the scene
        foreach (Camera cam in cameras)
        {
            if (cam.gameObject.activeInHierarchy && cam.enabled)
            {
                activeCam = cam;
                break;
            }
        }

        // Set the found camera as active if one was found
        if (activeCam != null)
        {
            SetActiveCamera(activeCam);
        }
    }

    private void Update()
    {
        // Check for left or right mouse button clicks each frame
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Create a ray from the camera through the mouse position
            Ray ray = currentActiveCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform raycast to detect objects under the mouse cursor
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object implements the IClickable interface
                IClickable clickable = hit.collider.GetComponent<IClickable>();
                if (clickable != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        clickable.OnLeftClick();
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        clickable.OnRightClick();
                    }
                }
            }
        }
    }
}
