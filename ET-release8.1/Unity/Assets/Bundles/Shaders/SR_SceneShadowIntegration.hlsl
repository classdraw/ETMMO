#ifndef SR_SCENE_SHADOW_INTEGRATION_INCLUDED
#define SR_SCENE_SHADOW_INTEGRATION_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_SceneShadowRT);
SAMPLER(sampler_SceneShadowRT);

// channelMask: 材质/贴图遮罩，与投影 RT 按 RGBA 通道一一对应。
// 例：遮罩 (1,0,0,0) + RT (0,1,0,0) → dot=0 无阴影；(1,1,0,0) + RT (0,1,0,0) → dot=1 有阴影。
half3 ApplySRSceneShadow(
    half3 color,
    float4 positionCS,
    half4 channelMask,
    half useSceneShadow,
    half3 sceneShadowColor,
    half sceneShadowIntensity)
{
    if (useSceneShadow < 0.5h)
    {
        return color;
    }

    float2 shadowUV = GetNormalizedScreenSpaceUV(positionCS);
    half4 shadowSample = SAMPLE_TEXTURE2D(_SceneShadowRT, sampler_SceneShadowRT, shadowUV);
    half shadowMask = saturate(dot(channelMask, shadowSample));
    // Ignore RT edge bleed; only apply meaningful shadow values.
    shadowMask = smoothstep(0.05h, 0.15h, shadowMask);

    half3 tintedShadow = lerp(half3(1.0h, 1.0h, 1.0h), sceneShadowColor, sceneShadowIntensity);
    return color * lerp(half3(1.0h, 1.0h, 1.0h), tintedShadow, shadowMask);
}

#endif
