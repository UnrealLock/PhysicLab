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
        // Запоминаем свою подсветку до появления
        // новых вложенных коннекторов.
        outline = GetComponentInChildren<Outline>();
    }

    public void ApplyMatching()
    {
        isMatched = true;
        isBeingMatched = false;
    }

    public void CancelSelection()
    {
        // Не сбрасываем состояние существующих соединений.
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

        // isMatched здесь не проверяем:
        // одно логическое гнездо может иметь несколько проводов.
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

        // Сначала система должна принять выбор.
        if (!socketSystem.RegisterSocket(gameObject))
            return;

        isBeingMatched = true;
        outline.StartOutline();
    }

    public void OnRightClick()
    {
    }
}
