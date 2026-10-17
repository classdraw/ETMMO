#ifndef SB_PROPIXELIZER_UBER_INPUT_INCLUDED
#define SB_PROPIXELIZER_UBER_INPUT_INCLUDED

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float4 _BaseMap_TexelSize;
    float4 _BodyMap_ST;
    float4 _BodyMap_TexelSize;
    half4 _BaseColor;
    float4 _BumpMap_ST;
    float4 _BumpMap_TexelSize;
    half4 _EmissionColor;
    half _AlphaClip;
    half _AlphaClipThreshold;
    half _DiffuseVertexColorWeight;
    half _EmissiveVertexColorWeight;
    half4 _SpecColor;
    half _Metallic;
    half _Smoothness;
    half4 _OutlineColor;
    half4 _EdgeHighlightColor;
    half _EdgeBevelWeight;
    float4 _LightingRamp_TexelSize;
    float4 _PaletteLUT_TexelSize;
    float4 _AmbientLight;
    half _LightingRampRange;
    half4 _ShadowTintColor;
    half _PixelSize;
    float4 _PixelGridOrigin;
    float4 _EmissionMap_ST;
    half _ID;
    half _UseSceneShadow;
    half _SceneShadowIntensity;
    half4 _SceneShadowColor;
    half4 _SceneShadowChannelMask;
    float4 _SpecGlossMap_ST;
    float4 _SpecGlossMap_TexelSize;
    float4 _MetallicGlossMap_TexelSize;
CBUFFER_END

half SB_AssignedMapMask(float4 texelSize)
{
    return (texelSize.z > 1.5 && texelSize.w > 1.5) ? 1.0 : 0.0;
}

// 无 Alpha 通道的 RGB 贴图在 Unity 里常为 a=0；未开 AlphaClip 时按不透明处理。
half SB_ResolveOpaqueAlpha(half4 baseSample)
{
    half alpha = baseSample.a * _BaseColor.a;
    if (SB_AssignedMapMask(_BaseMap_TexelSize) > 0.5h && alpha < 0.01h)
    {
        alpha = _BaseColor.a;
    }
    return alpha;
}

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_BodyMap);
SAMPLER(sampler_BodyMap);
TEXTURE2D(_BumpMap);
SAMPLER(sampler_BumpMap);
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_EmissionMap);
TEXTURE2D(_SpecGlossMap);
SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_MetallicGlossMap);
SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_LightingRamp);
SAMPLER(sampler_LightingRamp);
TEXTURE2D(_PaletteLUT);
SAMPLER(sampler_PaletteLUT);

#endif
