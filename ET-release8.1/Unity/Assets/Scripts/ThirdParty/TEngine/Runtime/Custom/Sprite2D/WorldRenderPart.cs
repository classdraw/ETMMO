using UnityEngine;

/// <summary>
/// A visual part inside a WorldSortItem.
/// Works with MeshRenderer as well as SpriteRenderer, but is intended for custom mesh quads.
/// localOrder controls only ordering inside its owner (body, weapon, cape, etc.).
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public sealed class WorldRenderPart : MonoBehaviour
{
    [Tooltip("Ordering within the owning WorldSortItem. Example: cape -10, body 0, front weapon 20.")]
    [SerializeField] private int localOrder;

    [Tooltip("Optional override. Normally inherited from the parent WorldSortItem.")]
    [SerializeField] private string sortingLayerOverride;

    private Renderer cachedRenderer;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        ApplyFromParent();
    }

    private void OnValidate()
    {
        cachedRenderer = GetComponent<Renderer>();
        ApplyFromParent();
    }

    [ContextMenu("Apply from parent")]
    public void ApplyFromParent()
    {
        var owner = GetComponentInParent<WorldSortItem>();
        if (owner != null)
            Apply(owner);
    }

    public void Apply(WorldSortItem owner)
    {
        if (cachedRenderer == null)
            cachedRenderer = GetComponent<Renderer>();

        if (cachedRenderer == null)
            return;

        cachedRenderer.sortingLayerName = string.IsNullOrEmpty(sortingLayerOverride)
            ? owner.SortingLayerName
            : sortingLayerOverride;
        cachedRenderer.sortingOrder = owner.SortingOrder + localOrder;
    }
}
