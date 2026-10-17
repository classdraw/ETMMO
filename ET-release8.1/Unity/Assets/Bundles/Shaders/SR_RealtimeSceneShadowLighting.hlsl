#ifndef SR_REALTIME_SCENE_SHADOW_LIGHTING_INCLUDED
#define SR_REALTIME_SCENE_SHADOW_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "SR_SceneShadowIntegration.hlsl"

// 2D / 场景 Sprite：默认接收 URP 主光 ShadowMap；_RECEIVE_SHADOWS_OFF 时关闭。
Light SR_GetMainLightRealtime(float4 shadowCoord)
{
#if defined(_RECEIVE_SHADOWS_OFF)
    return GetMainLight();
#elif defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
    return GetMainLight(shadowCoord);
#else
    return GetMainLight();
#endif
}

// ProPixelizer 等：仅 _RECEIVE_SHADOWS_ON 时采样 ShadowMap。
Light SR_GetMainLightOptional(float4 shadowCoord)
{
#if defined(_RECEIVE_SHADOWS_OFF)
    return GetMainLight();
#elif defined(_RECEIVE_SHADOWS_ON)
    #if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
        return GetMainLight(shadowCoord);
    #endif
#endif
    return GetMainLight();
}

half SR_HalfLambertNdotL(half3 normalWS, half3 lightDir)
{
    return saturate(dot(normalWS, lightDir) * 0.5h + 0.5h);
}

half3 SR_MainLightLambert(Light mainLight, half3 normalWS)
{
    half ndotl = SR_HalfLambertNdotL(normalWS, mainLight.direction);
    return mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * ndotl;
}

// 实时光照乘 albedo；无有效 GI/主光数据时保持原亮度（2D 常用）。
half3 SR_ApplySpriteRealtimeLighting(half3 albedo, Light mainLight, half3 normalWS, half3 bakedGI)
{
    half3 mainLightColor = SR_MainLightLambert(mainLight, normalWS);
    MixRealtimeAndBakedGI(mainLight, normalWS, bakedGI);
    half3 lighting = bakedGI + mainLightColor;
    half lightLuma = max(max(lighting.r, lighting.g), lighting.b);
    return albedo * lerp(half3(1.0h, 1.0h, 1.0h), lighting, saturate(lightLuma * 1000.0h));
}

// 在实时光照结果上叠加 Scene Shadow RT（与主光阴影独立、可同时生效）。
half3 SR_ApplySceneShadowOnLit(
    half3 litRgb,
    float4 positionCS,
    half4 sceneShadowChannelMask,
    half useSceneShadow,
    half3 sceneShadowColor,
    half sceneShadowIntensity)
{
    return ApplySRSceneShadow(
        litRgb,
        positionCS,
        sceneShadowChannelMask,
        useSceneShadow,
        sceneShadowColor,
        sceneShadowIntensity);
}

#endif
