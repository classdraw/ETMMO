using UnityEngine;

/// <summary>
/// 云影全局 Shader 参数驱动。挂到场景 GameObject 上，启用后写入全局 _CloudTex / _CloudSizeAndSpeed / _CloudStrength。
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class CloudShadowTexture : MonoBehaviour
{
    private const string UseCloudShadowKeyword = "_USE_CLOUD_SHADOW";

    private static readonly int CloudTexId = Shader.PropertyToID("_CloudTex");
    private static readonly int CloudSizeAndSpeedId = Shader.PropertyToID("_CloudSizeAndSpeed");
    private static readonly int CloudStrengthId = Shader.PropertyToID("_CloudStrength");

    [SerializeField]
    [Tooltip("默认 fx_tex_CityCloudNoise.png")]
    private Texture2D cloudTex;

    [SerializeField]
    private float cloudSizeX = 0.0035f;

    [SerializeField]
    private float cloudSizeZ = 0.0035f;

    [SerializeField]
    private float cloudSpeedX = 0.013f;

    [SerializeField]
    private float cloudSpeedZ = 0.022f;

    [SerializeField]
    private float cloudStrength = 1f;

    private Texture2D cachedTex;
    private Vector4 cachedSizeAndSpeed;
    private float cachedStrength = float.NaN;

    private void OnEnable()
    {
        ApplyGlobals(force: true);
        Shader.EnableKeyword(UseCloudShadowKeyword);
    }

    private void OnDisable()
    {
        Shader.DisableKeyword(UseCloudShadowKeyword);
    }

    private void Update()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        ApplyGlobals(force: false);
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        ApplyGlobals(force: true);
    }

    private void ApplyGlobals(bool force)
    {
        Vector4 sizeAndSpeed = new Vector4(cloudSizeX, cloudSizeZ, cloudSpeedX, cloudSpeedZ);
        if (!force
            && cachedTex == cloudTex
            && cachedSizeAndSpeed == sizeAndSpeed
            && Mathf.Approximately(cachedStrength, cloudStrength))
        {
            return;
        }

        cachedTex = cloudTex;
        cachedSizeAndSpeed = sizeAndSpeed;
        cachedStrength = cloudStrength;

        if (cachedTex != null)
        {
            Shader.SetGlobalTexture(CloudTexId, cachedTex);
        }

        Shader.SetGlobalVector(CloudSizeAndSpeedId, cachedSizeAndSpeed);
        Shader.SetGlobalFloat(CloudStrengthId, cachedStrength);
    }
}
