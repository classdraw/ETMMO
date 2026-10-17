#ifndef SB_PROPIXELIZER_TERRAIN_FORWARD_INCLUDED
#define SB_PROPIXELIZER_TERRAIN_FORWARD_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
#include "SB_ProPixelizerTerrainInput.hlsl"
#include "../../Art/ProPixelizer/SRP/ShaderLibrary/PixelUtils.hlsl"
#include "SB_ProPixelizerColorGrade.hlsl"
#include "SR_RealtimeSceneShadowLighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"

struct SBVaryings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    half3 normalWS : TEXCOORD2;
    half4 tangentWS : TEXCOORD3;
    half4 color : COLOR;
    half3 viewDirWS : TEXCOORD4;
    float4 shadowCoord : TEXCOORD5;
    float4 screenPos : TEXCOORD6;
    half fogFactor : TEXCOORD7;
};

#include "SB_ProPixelizerLighting.hlsl"

SBVaryings SB_BuildVaryingsFromTerrain(Varyings IN, InputData inputData)
{
    SBVaryings sb;
    sb.positionCS = IN.clipPos;
    sb.uv = IN.uvMainAndLM.xy;
    sb.positionWS = inputData.positionWS;
    sb.normalWS = inputData.normalWS;
    sb.tangentWS = half4(1, 0, 0, 1);
    sb.color = half4(1, 1, 1, 1);
    sb.viewDirWS = inputData.viewDirectionWS;
    sb.shadowCoord = inputData.shadowCoord;
    sb.screenPos = ComputeScreenPos(IN.clipPos);
#if defined(_ADDITIONAL_LIGHTS_VERTEX)
    sb.fogFactor = IN.fogFactorAndVertexLight.x;
#else
    sb.fogFactor = IN.fogFactor;
#endif
    return sb;
}

void ProPixelizerTerrainFragment(
    Varyings IN
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
    )
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
#ifdef _ALPHATEST_ON
    ClipHoles(IN.uvMainAndLM.xy);
#endif

    half3 normalTS = half3(0.0h, 0.0h, 1.0h);
#ifdef TERRAIN_SPLAT_BASEPASS
    half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvMainAndLM.xy).rgb;
    half smoothness = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvMainAndLM.xy).a;
    half metallic = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MetallicTex, IN.uvMainAndLM.xy).r;
    half alpha = 1;
#else
    half4 hasMask = half4(_LayerHasMask0, _LayerHasMask1, _LayerHasMask2, _LayerHasMask3);
    half4 masks[4];
    ComputeMasks(masks, hasMask, IN);

    float2 splatUV = (IN.uvMainAndLM.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    half4 splatControl = SAMPLE_TEXTURE2D(_Control, sampler_Control, splatUV);

    half alpha = dot(splatControl, 1.0h);
#ifdef _TERRAIN_BLEND_HEIGHT
    if (_NumLayersCount <= 4)
        HeightBasedSplatModify(splatControl, masks);
#endif

    half weight;
    half4 mixedDiffuse;
    half4 defaultSmoothness;
    SplatmapMix(IN.uvMainAndLM, IN.uvSplat01, IN.uvSplat23, splatControl, weight, mixedDiffuse, defaultSmoothness, normalTS);
    half3 albedo = mixedDiffuse.rgb;

    half4 defaultMetallic = half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3);
    half4 maskSmoothness = half4(masks[0].a, masks[1].a, masks[2].a, masks[3].a);
    defaultSmoothness = lerp(defaultSmoothness, maskSmoothness, hasMask);
    half smoothness = dot(splatControl, defaultSmoothness);

    half4 maskMetallic = half4(masks[0].r, masks[1].r, masks[2].r, masks[3].r);
    defaultMetallic = lerp(defaultMetallic, maskMetallic, hasMask);
    half metallic = dot(splatControl, defaultMetallic);
#endif

    InputData inputData;
    InitializeInputData(IN, normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, IN.uvMainAndLM.xy, _BaseMap);

#if defined(_DBUFFER)
    half occlusion = 1;
    half3 specular = half3(0.0h, 0.0h, 0.0h);
    ApplyDecal(IN.clipPos, albedo, specular, inputData.normalWS, metallic, occlusion, smoothness);
#endif

    SBVaryings sbInput = SB_BuildVaryingsFromTerrain(IN, inputData);
    half3 specColor = lerp(half3(0.04h, 0.04h, 0.04h), albedo, metallic);
    half3 color = SB_ShadeProPixelizer(sbInput, albedo, inputData.normalWS, half3(0, 0, 0), smoothness, specColor);

    float alphaOut;
    float2 ditherUV;
    float4 screenParams = _ScreenParams;
    float4 pixelPos = float4(IN.clipPos.xy, 0, 0);
#if defined(USE_OBJECT_POSITION_FOR_PIXEL_GRID_ON)
    float3 gridOrigin = float3(UNITY_MATRIX_M._m03_m13_m23);
#else
    float3 gridOrigin = _PixelGridOrigin.xyz;
#endif

    if (_ProPixelizer_Pass > 0.5)
    {
        PixelClipAlpha_float(UNITY_MATRIX_VP, gridOrigin, screenParams, pixelPos, _PixelSize, 1.0, _AlphaClipThreshold, alphaOut, ditherUV);
        clip(alphaOut - 0.001);
    }
    else
    {
        ditherUV = float2(0.5, 0.5);
    }

#if defined(_COLOR_GRADING_ON)
    half4 graded;
    SB_InternalColorGrade(_PaletteLUT, sampler_PaletteLUT, half4(color, 1), ditherUV, graded);
    color = graded.rgb;
#endif

    color = SR_ApplySceneShadowOnLit(
        color,
        IN.clipPos,
        _SceneShadowChannelMask,
        _UseSceneShadow,
        _SceneShadowColor.rgb,
        _SceneShadowIntensity);

    outColor = half4(color, 1.0h);

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}

#endif
