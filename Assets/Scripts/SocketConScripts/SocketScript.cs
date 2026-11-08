using UnityEngine;

public class SocketScript : MonoBehaviour, IClickable
{
    public int SocketID;

    [Tooltip("В каком направлении должен быть направлен коннектор")]
    public Vector3 direction = Vector3.forward;

    private bool isMatched;
    public bool IsMatched => isMatched;

    private Outline outline;
    private bool isBeingMatched;

    private void Awake()
    {
        // Cache this socket's outline before
        // any new nested connectors are created.
        outline = GetComponentInChildren<Outline>();
    }

    public void ApplyMatching()
    {
        isMatched = true;
        isBeingMatched = false;
    }

    public void CancelSelection()
    {
        // Do not reset the state of existing connections.
        isBeingMatched = false;
    }

    public void ResetSocket()
    {
        isBeingMatched = false;
        isMatched = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            transform.position,
            Vector3.one * 0.005f);

        Gizmos.DrawRay(
            transform.position,
            transform.TransformDirection(direction) * 0.2f);
    }

    public void OnLeftClick()
    {
        if (outline == null)
            outline = GetComponentInChildren<Outline>();

        if (outline == null)
            return;

        // Do not check isMatched here:
        // a single logical socket can have multiple wires.
        if (isBeingMatched || outline.IsOutlining)
            return;

        GameObject systemObject =
            GameObject.FindGameObjectWithTag("SocketsSystem");

        if (systemObject == null)
            return;

        var socketSystem =
            systemObject.GetComponent<SocketsSystemScript>();

        if (socketSystem == null)
            return;

        // The system must accept the selection first.
        if (!socketSystem.RegisterSocket(gameObject))
            return;

        isBeingMatched = true;
        outline.StartOutline();
    }

    public void OnRightClick()
    {
    }
}
