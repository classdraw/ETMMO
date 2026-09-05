using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gives a mesh-based world object one stable Z-sort key.
/// Attach this to a character root, a tree trunk root, a prop, or a world VFX root.
/// Every WorldRenderPart below this root uses the same key and only adds a local order.
/// </summary>
[ExecuteAlways]
public sealed class WorldSortItem : MonoBehaviour
{
    [Header("World ordering")]
    [SerializeField] private WorldRenderPass renderPass = WorldRenderPass.World;
    [Tooltip("World-space Z of this transform is used for occlusion sorting within the same render pass.")]
    [SerializeField] private Transform sortAnchor;
    [SerializeField, Min(1)] private int unitsPerSortStep = 100;
    [SerializeField] private int passOrderOffset;
    [SerializeField] private bool autoRefresh = true;
    [Tooltip("Sort order refresh interval in seconds. 0 = every frame.")]
    [SerializeField, Min(0f)] private float refreshInterval = 0.1f;
    [SerializeField] private List<WorldRenderPart> renderParts = new();

    public int SortingOrder { get; private set; }
    public float SortAnchorWorldZ { get; private set; }
    public WorldRenderPass RenderPass => renderPass;
    public string SortingLayerName => WorldRenderPassUtility.GetSortingLayerName(renderPass);

    private float lastRefreshTime = -1f;

    private void Reset()
    {
        sortAnchor = transform;
        RefreshRender();
    }

    private void OnEnable()
    {
        lastRefreshTime = Time.realtimeSinceStartup;
        RefreshSortOrder();
    }

    private void LateUpdate()
    {
        if (!autoRefresh)
            return;

        if (refreshInterval == 0f)
        {
            RefreshSortOrder();
            return;
        }

        float now = Time.realtimeSinceStartup;
        if (lastRefreshTime < 0f || now - lastRefreshTime >= refreshInterval)
        {
            lastRefreshTime = now;
            RefreshSortOrder();
        }
    }

    private void OnValidate()
    {
        if (sortAnchor == null)
            sortAnchor = transform;

        RefreshSortOrder();
    }

    [ContextMenu("Refresh sort order")]
    public void RefreshSortOrder()
    {
        EnsureRenderPartsCached();

        Transform anchor = sortAnchor != null ? sortAnchor : transform;
        SortAnchorWorldZ = anchor.position.z;

        // Lower world Z is nearer the camera in this XZ-plane convention, so it must draw later.
        SortingOrder = passOrderOffset - Mathf.RoundToInt(SortAnchorWorldZ * unitsPerSortStep);

        for (int i = 0; i < renderParts.Count; i++)
        {
            WorldRenderPart part = renderParts[i];
            if (part != null)
                part.Apply(this);
        }
    }

    [ContextMenu("RefreshRender")]
    public void RefreshRender()
    {
        if (renderParts == null)
            renderParts = new List<WorldRenderPart>();
        else
            renderParts.Clear();

        CollectOwnedRenderParts(transform, renderParts);
    }

    private void CollectOwnedRenderParts(Transform node, List<WorldRenderPart> results)
    {
        if (node.TryGetComponent(out WorldRenderPart part))
            results.Add(part);

        for (int i = 0; i < node.childCount; i++)
        {
            Transform child = node.GetChild(i);
            if (child.TryGetComponent<WorldSortItem>(out _) && child != transform)
                continue;

            CollectOwnedRenderParts(child, results);
        }
    }

    private void EnsureRenderPartsCached()
    {
        if (renderParts == null || renderParts.Count == 0)
            RefreshRender();
    }
}
