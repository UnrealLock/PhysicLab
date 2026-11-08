using System.Collections;
using UnityEngine;

public class OnSwitchButtonScript : MonoBehaviour, IClickable
{
    [SerializeField] private GameObject message;
    // Reference to the sockets system script
    SocketsSystemScript socketsSystemScript;
    // Flag to indicate if the system is activated
    public bool IsActivated = false;
    // Cooldown flag
    private bool isCooldown = false;
    // Rotation delta for visual effect
    private float rotationDelta = 15f;

    public Material emissionMaterial;
    private Material defaultMaterial;

    // Start is called before the first frame update
    void Start()
    {
        // Get reference to the sockets system script
        socketsSystemScript = GameObject.FindGameObjectWithTag("SocketsSystem").transform.GetComponent<SocketsSystemScript>();
    }

    // Update is called once per frame
    void Update()
    {
        // Auto turn off the system if not all sockets are matched
        if (!socketsSystemScript.IsAllMatched && IsActivated)
        {
            FinishSystem();
        }
    }

    void StartSystem()
    {
        // Get the MeshRenderer component
        var renderer = GetComponent<MeshRenderer>();
        defaultMaterial = renderer.material;
        // Get the material
        //var mat = renderer.material;
        renderer.material = emissionMaterial;
        // Enable emission and set color to red
        //mat.EnableKeyword("_EMISSION");
        //mat.SetColor("_EmissionColor", Color.red * 1f);
        // Rotate the switch
        var rot = transform.eulerAngles;
        rot.x -= rotationDelta;
        transform.eulerAngles = rot;
        // Set activated flag
        IsActivated = true;
    }

    public void FinishSystem()
    {
        // Get the MeshRenderer component
        var renderer = GetComponent<MeshRenderer>();
        renderer.material = defaultMaterial;
        //var mat = renderer.material;
        // Disable emission
        //mat.DisableKeyword("_EMISSION");
        //mat.SetColor("_EmissionColor", Color.black);
        // Rotate the switch
        var rot = transform.eulerAngles;
        rot.x += rotationDelta;
        transform.eulerAngles = rot;
        // Reset activated flag
        IsActivated = false;
    }

    System.Collections.IEnumerator Cooldown(float cooldownTimer)
    {
        // Set cooldown flag and wait for the specified time
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTimer);
        isCooldown = false;
    }

    IEnumerator ShowMessage() // message if the sockets are matched wrong
    {
        message.SetActive(true);
        yield return new WaitForSeconds(1.2f);
        message.SetActive(false);
    }

    public void OnLeftClick()
    {
        // Start system only if not in cooldown and all sockets are matched
        if (socketsSystemScript.IsAllMatched && !IsActivated)
        {
            StartSystem();
            StartCoroutine(Cooldown(0.5f));
        }
        // Turn off system if all sockets are matched and system is activated
        else if (socketsSystemScript.IsAllMatched && IsActivated)
        {
            FinishSystem();
            StartCoroutine(Cooldown(0.5f));
        }
        // Show message if sockets are not all matched
        else
        {
            StartCoroutine(ShowMessage());
        }
    }

    public void OnRightClick()
    {
    }
}
