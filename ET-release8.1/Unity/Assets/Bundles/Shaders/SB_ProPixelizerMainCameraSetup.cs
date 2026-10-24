using ProPixelizer;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Ensures gameplay main camera has ProPixelizer snap settings aligned with URP Render Feature.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-200)]
public class SB_ProPixelizerMainCameraSetup : MonoBehaviour
{
    [Tooltip("Must match ProPixelizer Render Feature DefaultLowResScale.")]
    [Range(0.1f, 1f)]
    public float TargetScale = 0.45f;

    [Tooltip("Disable SMAA/TAA on pixel cameras; they cause edge shimmer.")]
    public bool DisableCameraAntialiasing = true;

    void Awake()
    {
        Camera camera = GetComponent<Camera>();
        if (camera == null)
            return;

        ProPixelizerCamera proPixelizer = GetComponent<ProPixelizerCamera>();
        if (proPixelizer == null)
            proPixelizer = gameObject.AddComponent<ProPixelizerCamera>();

        proPixelizer.EnableProPixelizer = true;
        proPixelizer.Mode = ProPixelizerCamera.PixelSizeMode.FixedDownscalingRatio;
        proPixelizer.TargetScale = TargetScale;
        proPixelizer.UsePixelExpansion = true;
        proPixelizer.ClampCameraOrthoSize = true;
        proPixelizer.UsePixelArtUpscalingFilter = true;

        if (DisableCameraAntialiasing)
        {
            UniversalAdditionalCameraData urp = GetComponent<UniversalAdditionalCameraData>();
            if (urp != null)
            {
                urp.antialiasing = AntialiasingMode.None;
                urp.renderPostProcessing = false;
            }
        }
    }
}
