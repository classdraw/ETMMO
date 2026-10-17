#ifndef SB_PROPIXELIZER_UBER_INPUT_INCLUDED
#define SB_PROPIXELIZER_UBER_INPUT_INCLUDED

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    float4 _BumpMap_ST;
    half4 _EmissionColor;
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
    half _PixelSize;
    float4 _PixelGridOrigin;
    float4 _EmissionMap_ST;
    half _ID;
    half _UseSceneShadow;
    half _SceneShadowIntensity;
    half4 _SceneShadowColor;
    half4 _SceneShadowChannelMask;
CBUFFER_END

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
