using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 投影阴影相机控制器。挂在专用阴影 Camera 上，同步主相机视口并渲染 SceneShadow 层到全局 _SceneShadowRT。
/// </summary>
[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class ShadowCameraCtrl : MonoBehaviour
{
    public const string SceneShadowLayerName = "SceneShadow";
    public static readonly int SceneShadowRTId = Shader.PropertyToID("_SceneShadowRT");

    [SerializeField]
    [Tooltip("同步视口参数的主相机；留空则自动使用 Camera.main。")]
    private Camera sourceCamera;

    [SerializeField]
    [Range(0.1f, 1f)]
    [Tooltip("阴影 RT 相对屏幕分辨率的比例。")]
    private float resolutionScale = 0.5f;

    private Camera shadowCamera;
    private RenderTexture sceneShadowRT;
    private int sceneShadowLayer = -1;

    public RenderTexture SceneShadowRT => sceneShadowRT;

    public void SetSourceCamera(Camera camera)
    {
        sourceCamera = camera;
    }

    private void Awake()
    {
        shadowCamera = GetComponent<Camera>();
        sceneShadowLayer = LayerMask.NameToLayer(SceneShadowLayerName);
        SetupCamera();
        CreateOrResizeRenderTexture();
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        ApplyGlobalTexture();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    private void Update()
    {
        CreateOrResizeRenderTexture();
    }

    private void OnDestroy()
    {
        ReleaseRenderTexture();
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == shadowCamera)
        {
            SyncWithSourceCamera();
            return;
        }

        if (camera == GetSourceCamera())
        {
            ApplyGlobalTexture();
        }
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera != shadowCamera || sceneShadowRT == null)
        {
            return;
        }

        ApplyGlobalTexture();
    }

    private Camera GetSourceCamera()
    {
        if (sourceCamera != null && sourceCamera != shadowCamera)
        {
            return sourceCamera;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera != shadowCamera)
        {
            return mainCamera;
        }

        return null;
    }

    private void SyncWithSourceCamera()
    {
        Camera referenceCamera = GetSourceCamera();
        if (referenceCamera == null)
        {
            return;
        }

        Transform referenceTransform = referenceCamera.transform;
        shadowCamera.transform.SetPositionAndRotation(referenceTransform.position, referenceTransform.rotation);
        shadowCamera.fieldOfView = referenceCamera.fieldOfView;
        shadowCamera.orthographic = referenceCamera.orthographic;
        shadowCamera.orthographicSize = referenceCamera.orthographicSize;
        shadowCamera.nearClipPlane = referenceCamera.nearClipPlane;
        shadowCamera.farClipPlane = referenceCamera.farClipPlane;

        if (sceneShadowRT != null)
        {
            shadowCamera.aspect = (float)sceneShadowRT.width / sceneShadowRT.height;
        }
        else
        {
            shadowCamera.aspect = referenceCamera.aspect;
        }
    }

    private void SetupCamera()
    {
        shadowCamera.clearFlags = CameraClearFlags.SolidColor;
        shadowCamera.backgroundColor = Color.black;
        shadowCamera.depth = -10;

        if (sceneShadowLayer >= 0)
        {
            shadowCamera.cullingMask = 1 << sceneShadowLayer;
        }
        else
        {
            Debug.LogWarning(
                $"[{nameof(ShadowCameraCtrl)}] Layer \"{SceneShadowLayerName}\" not found. " +
                "Add it in Project Settings > Tags and Layers.");
        }
    }

    private void CreateOrResizeRenderTexture()
    {
        int width = Mathf.Max(1, Mathf.RoundToInt(Screen.width * resolutionScale));
        int height = Mathf.Max(1, Mathf.RoundToInt(Screen.height * resolutionScale));

        if (sceneShadowRT != null
            && sceneShadowRT.width == width
            && sceneShadowRT.height == height)
        {
            return;
        }

        ReleaseRenderTexture();

        sceneShadowRT = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
        {
            name = "SceneShadowRT",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        sceneShadowRT.Create();

        shadowCamera.targetTexture = sceneShadowRT;
        ApplyGlobalTexture();
    }

    private void ApplyGlobalTexture()
    {
        if (sceneShadowRT == null)
        {
            return;
        }

        Shader.SetGlobalTexture(SceneShadowRTId, sceneShadowRT);
    }

    private void ReleaseRenderTexture()
    {
        if (sceneShadowRT == null)
        {
            return;
        }

        if (shadowCamera != null)
        {
            shadowCamera.targetTexture = null;
        }

        sceneShadowRT.Release();
        Destroy(sceneShadowRT);
        sceneShadowRT = null;
    }
}
