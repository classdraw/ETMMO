#ifndef SB_PROPIXELIZER_TERRAIN_INPUT_INCLUDED
#define SB_PROPIXELIZER_TERRAIN_INPUT_INCLUDED

CBUFFER_START(SBProPixelizerTerrain)
    float4 _LightingRamp_TexelSize;
    float4 _PaletteLUT_TexelSize;
    float4 _AmbientLight;
    half _LightingRampRange;
    half4 _ShadowTintColor;
    half _PixelSize;
    float4 _PixelGridOrigin;
    half _ID;
    half _AlphaClipThreshold;
    half4 _OutlineColor;
    half4 _EdgeHighlightColor;
    half _EdgeBevelWeight;
    half _UseSceneShadow;
    half _SceneShadowIntensity;
    half4 _SceneShadowColor;
    half4 _SceneShadowChannelMask;
CBUFFER_END

TEXTURE2D(_LightingRamp);
SAMPLER(sampler_LightingRamp);
TEXTURE2D(_PaletteLUT);
SAMPLER(sampler_PaletteLUT);

#endif
