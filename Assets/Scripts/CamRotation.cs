using UnityEngine;

public class CamRotation : MonoBehaviour
{
    private const float MaxYRotation = 210f;
    private const float MinYRotation = 140f;
    private const float MaxXRotation = 45f;
    private const float MinXRotation = 0f;
    public float mouseSensitivity = 1000.0f;
    private float rotY = 0.0f;
    private float rotX = 0.0f;

    private const float MinFov = 15f;
    private const float MaxFov = 75f;
    private const float ZoomSensitivity = 10f;

    private Camera mainCam;
    void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    void Update()
    {
        if (Input.GetButton("Fire2"))
        {
            CameraRotation();
        }
        CameraZoom();
    }

    void CameraZoom()
    {
        var fov = mainCam.fieldOfView;
        fov -= Input.GetAxis("Mouse ScrollWheel") * ZoomSensitivity;
        fov = Mathf.Clamp(fov, MinFov, MaxFov);
        mainCam.fieldOfView = fov;
    }

    void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        rotX += mouseY * mouseSensitivity * Time.deltaTime;
        rotY += mouseX * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, MinXRotation, MaxXRotation);
        rotY = Mathf.Clamp(rotY, MinYRotation, MaxYRotation);

        transform.rotation = Quaternion.Euler(rotX, rotY, 0.0f);
    }
}
