using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class MoveAppScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // Reference to the main camera
    public Camera MainCamera;
    float borderPadding = 0.003f;
    // Offset between mouse position and object position
    Vector3 offset;
    // Reference to the object's renderer
    RectTransform rectTransform;
    // Flag to indicate if the object is being dragged or can be dragged
    public bool IsDragging = false;
    public bool CanBeDriven = true;
    void Start()
    {
        // Initialize the renderer reference
        rectTransform = GetComponent<RectTransform>();
        // Initialize the offset
        offset = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDragging && !MainCamera.isActiveAndEnabled)
            IsDragging = false;
        // Disable dragging if not allowed or camera is inactive
        if (!CanBeDriven || !MainCamera.isActiveAndEnabled)
            return;
        // Drag the window if the left mouse button is pressed
        var mouseAction = InputSystem.GetDevice<Mouse>().leftButton.isPressed ? 1 : 0;
        if (mouseAction > 0)
            DragWindowWithBounds(mouseAction);
        else
            IsDragging = false;
    }

    void DragWindowWithBounds(float mouseAction)
    {
        // Ray from camera through mouse, intersect with screen plane
        var screen = GameObject.FindGameObjectWithTag("Screen").GetComponent<MeshRenderer>();
        var plane = new Plane(screen.transform.forward, screen.transform.position);
        var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        // if ray doesn't hit the plane, exit
        if (!plane.Raycast(ray, out float dist)) return;
        var mousePosition = ray.GetPoint(dist);
        // Calculate offset only when starting to drag
        if (!IsDragging)
            offset = mousePosition - transform.position;
        // Find real visual bounds by combining all child meshes
        float objMinX = float.MaxValue, objMaxX = float.MinValue;
        float objMinY = float.MaxValue, objMaxY = float.MinValue;
        var allFilters = GetComponentsInChildren<MeshFilter>();
        foreach (var filter in allFilters)
        {
            var mesh = filter.sharedMesh;
            if (mesh == null) continue;
            var min = mesh.bounds.min;
            var max = mesh.bounds.max;
            Vector3[] corners = new Vector3[8] {
            new(min.x, min.y, min.z), new(min.x, min.y, max.z),
            new(min.x, max.y, min.z), new(min.x, max.y, max.z),
            new(max.x, min.y, min.z), new(max.x, min.y, max.z),
            new(max.x, max.y, min.z), new(max.x, max.y, max.z)};
            foreach (var corner in corners)
            {
                var worldCorner = filter.transform.TransformPoint(corner);
                var localCorner = screen.transform.InverseTransformPoint(worldCorner);
                if (localCorner.x < objMinX) objMinX = localCorner.x;
                if (localCorner.x > objMaxX) objMaxX = localCorner.x;
                if (localCorner.y < objMinY) objMinY = localCorner.y;
                if (localCorner.y > objMaxY) objMaxY = localCorner.y;
            }
        }
        // Calculate radius where can be held by mouse and distance to mouse
        var distanceMouse = Vector2.Distance(transform.position, mousePosition);
        // Check if mouse is inside the object's rectangular area
        Vector3 mouseLocal = screen.transform.InverseTransformPoint(mousePosition);
        bool isMouseOver = (mouseLocal.x >= objMinX && mouseLocal.x <= objMaxX && mouseLocal.y >= objMinY && mouseLocal.y <= objMaxY);
        // Only allow dragging if within radius or already dragging
        if (isMouseOver || IsDragging)
        {
            // Store the original z position to maintain it during dragging
            var zPosition = transform.localPosition.z;
            // Set dragging flag
            IsDragging = true;
            // Screen bounds in its own local space (shrink y by padding)
            var screenBounds = screen.localBounds;
            screenBounds.min += new Vector3(0, borderPadding, 0);
            screenBounds.max -= new Vector3(0, borderPadding, 0);
            // Calculate new position based on mouse position and offset
            var newPosition = new Vector3(mousePosition.x - offset.x, mousePosition.y - offset.y, transform.position.z);
            // Convert pivot to screen local space
            var pivotLocal = screen.transform.InverseTransformPoint(transform.position);
            var targetLocal = screen.transform.InverseTransformPoint(newPosition);
            // Offsets from pivot to real object bounds
            float minExtX = objMinX - pivotLocal.x;
            float maxExtX = objMaxX - pivotLocal.x;
            float minExtY = objMinY - pivotLocal.y;
            float maxExtY = objMaxY - pivotLocal.y;
            // Clamp pivot inside screen
            if (targetLocal.x + minExtX <= screenBounds.min.x)
                targetLocal.x = screenBounds.min.x - minExtX;
            else if (targetLocal.x + maxExtX >= screenBounds.max.x)
                targetLocal.x = screenBounds.max.x - maxExtX;
            if (targetLocal.y + minExtY <= screenBounds.min.y)
                targetLocal.y = screenBounds.min.y - minExtY;
            else if (targetLocal.y + maxExtY >= screenBounds.max.y)
                targetLocal.y = screenBounds.max.y - maxExtY;
            // Convert back to world and apply
            newPosition = screen.transform.TransformPoint(targetLocal);
            transform.localPosition = transform.parent.InverseTransformPoint(newPosition);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, zPosition);
        }
        if (mouseAction == 0)
            IsDragging = false;
    }
}
