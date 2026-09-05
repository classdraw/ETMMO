using UnityEngine;

/// <summary>
/// Put this on a tree canopy / roof front object in the WorldFront pass.
/// The object's shader must multiply its alpha by the per-renderer float _OcclusionFade.
/// </summary>
[ExecuteAlways]
public sealed class WorldOccluderFader : MonoBehaviour
{
    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Collider2D fadeArea;
    [SerializeField] private Transform target;
    [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.45f;
    [SerializeField, Min(0.01f)] private float fadeSpeed = 7f;

    private static readonly int OcclusionFadeId = Shader.PropertyToID("_OcclusionFade");
    private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
    private float currentAlpha = 1f;

    private void Reset()
    {
        targetRenderers = GetComponentsInChildren<Renderer>();
        fadeArea = GetComponent<Collider2D>();
    }

    private void OnEnable()
    {
        if (target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    private void LateUpdate()
    {
        var shouldFade = target != null && fadeArea != null && fadeArea.OverlapPoint(target.position);
        var wantedAlpha = shouldFade ? fadedAlpha : 1f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, wantedAlpha, fadeSpeed * Time.deltaTime);

        foreach (var targetRenderer in targetRenderers)
        {
            if (targetRenderer == null)
                continue;

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(OcclusionFadeId, currentAlpha);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
