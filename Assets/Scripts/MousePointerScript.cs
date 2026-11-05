using UnityEngine;

public class MousePointerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // LayerMask to detect the ground or objects
    public LayerMask mask;
    // Camera to use for raycasting
    public Camera cameraToUse;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get BoxCollider component
        var boxCollider = gameObject.GetComponent<BoxCollider>();
        // If the camera is not active, disable the collider and exit
        if (!cameraToUse.isActiveAndEnabled)
        {
            if (boxCollider.enabled)
                gameObject.GetComponent<BoxCollider>().enabled = false;
            return;
        }
        // Ensure the collider is enabled
        if (!boxCollider.enabled)
            boxCollider.enabled = true;
        // Create a ray from the camera through the mouse position
        var ray = cameraToUse.ScreenPointToRay(Input.mousePosition);
        // Get position of hit point
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, mask))
            transform.position = raycastHit.point;
    }
}
