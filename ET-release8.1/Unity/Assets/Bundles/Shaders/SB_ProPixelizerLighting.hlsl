#ifndef SB_PROPIXELIZER_LIGHTING_INCLUDED
#define SB_PROPIXELIZER_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "SR_RealtimeSceneShadowLighting.hlsl"
#include "../../Art/ProPixelizer/SRP/ShaderLibrary/OutlineUtils.hlsl"

SAMPLER(sampler_LightingRamp_point_clamp);

void SB_ApplyLightingRamp(half4 inputLight, out half4 output, out half shadowTintStrength)
{
    half3 hsv = RgbToHsv(inputLight.rgb);
    half normalisedBrightness = hsv.b / max(_LightingRampRange, 0.001h);
    half satNormalisedBrightness = saturate(normalisedBrightness);
    hsv.b = SAMPLE_TEXTURE2D(_LightingRamp, sampler_LightingRamp_point_clamp, half2(satNormalisedBrightness, 0.5)).b;
    output.rgb = HsvToRgb(hsv);
    output.a = inputLight.a;
    shadowTintStrength = saturate(pow(1.1h - satNormalisedBrightness, 2.5h - 2 * _ShadowTintColor.a)) * _ShadowTintColor.a;
}

half3 SB_ShadeProPixelizer(
    SBVaryings input,
    half3 albedo,
    half3 normalWS,
    half3 emission,
    half smoothness,
    half3 specColor)
{
    InputData inputData = (InputData)0;
    inputData.positionWS = input.positionWS;
    inputData.positionCS = input.positionCS;
    inputData.normalWS = normalWS;
    inputData.viewDirectionWS = SafeNormalize(input.viewDirWS);
    inputData.shadowCoord = input.shadowCoord;
    inputData.fogCoord = input.fogFactor;
    inputData.vertexLighting = half3(0, 0, 0);
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.bakedGI = SampleSH(normalWS);
    inputData.shadowMask = half4(1, 1, 1, 1);

    SurfaceData surfaceData = (SurfaceData)0;
    surfaceData.albedo = half3(1, 1, 1);
    surfaceData.emission = half3(0, 0, 0);
    surfaceData.alpha = 1;
    surfaceData.occlusion = 1;
    surfaceData.clearCoatMask = 0;
    surfaceData.clearCoatSmoothness = 1;
    surfaceData.smoothness = saturate(smoothness);
#if defined(_SPECULAR_SETUP)
    surfaceData.specular = specColor;
    surfaceData.metallic = 1;
#else
    surfaceData.metallic = _Metallic;
    surfaceData.specular = half3(0.04, 0.04, 0.04);
#endif

    half4 preRampLight = UniversalFragmentPBR(inputData, surfaceData);
    preRampLight += lerp(half4(0, 0, 0, 1), half4(_AmbientLight.rgb, 1), _AmbientLight.a);

    half4 postRampLight;
    half shadowTintStrength;
    SB_ApplyLightingRamp(preRampLight, postRampLight, shadowTintStrength);

    half3 litColor = albedo * postRampLight.rgb;
    litColor = lerp(litColor, _ShadowTintColor.rgb, shadowTintStrength);

    half2 screenCoord = input.screenPos.xy / input.screenPos.w;
    half outlineFactor;
    half edgeFactor;
    half3 edgeNormalVS;
    GetOutline_float(screenCoord, outlineFactor, edgeFactor, edgeNormalVS);

    half edgeHighlight = edgeFactor * _EdgeHighlightColor.a;
    litColor = lerp(litColor, litColor * 2.0h * _EdgeHighlightColor.rgb, edgeHighlight);

    half outlineBlend = outlineFactor * _OutlineColor.a;
    litColor = lerp(litColor, _OutlineColor.rgb, outlineBlend);

    litColor += emission;
    litColor = MixFog(litColor, inputData.fogCoord);
    return litColor;
}

#endif
