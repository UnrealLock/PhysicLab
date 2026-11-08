using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Outline : MonoBehaviour
{
    [Header("Настройки свечения")]
    [Range(10f, 40f)]
    [SerializeField] private float flickerSpeed = 20f; // Speed of the flickering/pulsing effect
    [SerializeField] private Color outlineColor; // Color of the outline glow
    private float colorChangeDuration = 1.5f; // Duration before reverting to original outline color
    private Color originalOutlineColor; // Stores the color set in the Inspector

    private MeshRenderer meshRenderer;
    private Material material;
    private bool isOutlining;
    public bool IsOutlining { get => isOutlining; }

    private void Awake()
    {
        // Cache components for performance
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;

        // Set default color if none is specified in Inspector
        if (outlineColor == default(Color)) outlineColor = Color.green;

        // Store the original color for later restoration
        originalOutlineColor = outlineColor;
    }

    private void Update()
    {
        // Only update outline effect when active
        if (!isOutlining) return;

        // Use PingPong for smooth back-and-forth pulsing effect
        float alpha = Mathf.PingPong(Time.time * flickerSpeed * 0.1f, 1f);
        SetAlpha(alpha);
    }

    /// <summary>
    /// Sets the alpha channel of the current outline color
    /// </summary>
    private void SetAlpha(float alpha)
    {
        Color color = outlineColor;
        color.a = alpha;
        material.color = color;
    }

    /// <summary>
    /// Starts the pulsing outline effect
    /// </summary>
    public void StartOutline()
    {
        isOutlining = true;
    }

    /// <summary>
    /// Stops the pulsing and resets alpha to transparent
    /// </summary>
    public void StopOutline()
    {
        // Cancel any pending color restoration
        CancelInvoke(nameof(RestoreOutlineColor));
        isOutlining = false;

        // Restore original color if available
        outlineColor = originalOutlineColor != default ? originalOutlineColor : outlineColor;

        // Set alpha to 0 (fully transparent)
        SetAlpha(0f);
    }

    /// <summary>
    /// Temporarily changes the outline to red for visual feedback
    /// </summary>
    public void FlashRed()
    {
        // Store current color before changing to red
        originalOutlineColor = outlineColor;
        isOutlining = true;
        outlineColor = Color.red;

        // Schedule color restoration after specified duration
        Invoke(nameof(RestoreOutlineColor), colorChangeDuration);
    }

    /// <summary>
    /// Restores the outline color to the one set in the Inspector
    /// </summary>
    private void RestoreOutlineColor()
    {
        outlineColor = originalOutlineColor;
        StopOutline();
    }
}