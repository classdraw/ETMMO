#ifndef SB_PROPIXELIZER_UBER_FORWARD_INCLUDED
#define SB_PROPIXELIZER_UBER_FORWARD_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../../Art/ProPixelizer/SRP/ShaderLibrary/PixelUtils.hlsl"
#include "SB_ProPixelizerColorGrade.hlsl"
#include "SB_ProPixelizerUberInput.hlsl"
#include "SR_RealtimeSceneShadowLighting.hlsl"

half SB_SampleMaterialAlpha(float2 uv, half4 baseSample)
{
    half alpha = baseSample.a * _BaseColor.a;
    half bodyAlpha = SAMPLE_TEXTURE2D(_BodyMap, sampler_BodyMap, uv).a;
    return max(alpha, bodyAlpha * _BaseColor.a);
}

struct SBAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

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
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

half3 SB_SampleNormalWS(SBVaryings input, float2 uv)
{
    half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv));
    half3 bitangentWS = input.tangentWS.w * cross(input.normalWS, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangentWS, input.normalWS);
    return NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, tangentToWorld));
}

SBVaryings SB_Vert(SBAttributes input)
{
    SBVaryings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    output.positionCS = posInputs.positionCS;
    output.positionWS = posInputs.positionWS;
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    output.normalWS = normInputs.normalWS;
    output.tangentWS = half4(normInputs.tangentWS, input.tangentOS.w * GetOddNegativeScale());
    output.color = input.color;
    output.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
    output.shadowCoord = GetShadowCoord(posInputs);
    return output;
}

half4 SB_Frag(SBVaryings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);

    float2 uv = input.uv;
    half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
    half4 bodySample = SAMPLE_TEXTURE2D(_BodyMap, sampler_BodyMap, uv);
    half3 albedo = lerp(baseSample.rgb, bodySample.rgb, bodySample.a) * _BaseColor.rgb;
    albedo *= lerp(half3(1, 1, 1), input.color.rgb, _DiffuseVertexColorWeight);
    half alpha = SB_SampleMaterialAlpha(uv, baseSample);

    half3 normalWS = SB_SampleNormalWS(input, uv);
    half3 viewDirWS = SafeNormalize(input.viewDirWS);

    Light mainLight = SR_GetMainLightOptional(input.shadowCoord);

    half NdotL = saturate(dot(normalWS, mainLight.direction));
    half ramp = SAMPLE_TEXTURE2D(_LightingRamp, sampler_LightingRamp, half2(NdotL, 0.5)).r;
    half3 diffuse = albedo * (mainLight.color * ramp * mainLight.shadowAttenuation + _AmbientLight.rgb);

#if defined(_SPECULAR_SETUP)
    half3 specColor = _SpecColor.rgb;
    half smoothness = _Smoothness;
    half4 specMask = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv);
    specColor *= specMask.rgb;
    smoothness *= specMask.a;
#else
    half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv);
    half3 specColor = lerp(half3(0.04, 0.04, 0.04), albedo, metallicGloss.r * _Metallic);
    half smoothness = metallicGloss.a * _Smoothness;
#endif

    half3 specular = LightingSpecular(mainLight.color * mainLight.shadowAttenuation, mainLight.direction, normalWS, viewDirWS, half4(specColor, 1), exp2(10 * smoothness + 1));

    half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, uv).rgb * _EmissionColor.rgb;
    emission += input.color.rgb * _EmissiveVertexColorWeight * _EmissionColor.rgb;

    half3 color = diffuse + specular + emission;

    float alphaOut;
    float2 ditherUV;
    float4 screenParams = _ScreenParams;
    float4 pixelPos = float4(input.positionCS.xy, 0, 0);
#if defined(USE_OBJECT_POSITION_FOR_PIXEL_GRID_ON)
    float3 gridOrigin = float3(UNITY_MATRIX_M._m03_m13_m23);
#else
    float3 gridOrigin = _PixelGridOrigin.xyz;
#endif

    if (_ProPixelizer_Pass > 0.5)
    {
        PixelClipAlpha_float(UNITY_MATRIX_VP, gridOrigin, screenParams, pixelPos, _PixelSize, alpha, _AlphaClipThreshold, alphaOut, ditherUV);
        clip(alphaOut - 0.001);
    }
    else
    {
        ditherUV = float2(0.5, 0.5);
        clip(alpha - _AlphaClipThreshold);
    }

#if defined(_COLOR_GRADING_ON)
    half4 graded;
    SB_InternalColorGrade(_PaletteLUT, sampler_PaletteLUT, half4(color, alpha), ditherUV, graded);
    color = graded.rgb;
#endif

    color = SR_ApplySceneShadowOnLit(
        color,
        input.positionCS,
        _SceneShadowChannelMask,
        _UseSceneShadow,
        _SceneShadowColor.rgb,
        _SceneShadowIntensity);

    return half4(color, alpha);
}

#endif
