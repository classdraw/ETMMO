#ifndef FAKE_LIGHT_ENV_INCLUDED
#define FAKE_LIGHT_ENV_INCLUDED

#if defined(SAMPLE_TEXTURE2D)
TEXTURE2D(_FakeLightTexture);
SAMPLER(sampler_FakeLightTexture);
#define SAMPLE_FAKE_LIGHT_TEXTURE(uv) SAMPLE_TEXTURE2D(_FakeLightTexture, sampler_FakeLightTexture, uv)
#else
sampler2D _FakeLightTexture;
#define SAMPLE_FAKE_LIGHT_TEXTURE(uv) tex2D(_FakeLightTexture, uv)
#endif

// URP: CommandBuffer.SetGlobal* 不能放在自定义 CBUFFER 里
float2 _FakeLightTexUVRate;
float3 _FakeLightRegionStart;
float4 _FakeLightPoses[32];
float4 _FakeLightColors[32];
float _FakeLightRanges[32];
float4 _FakeSpotLightDirs[32];
float4 _FakeSpotLightAttens[32];

#if defined(FAKELIGHT_TILE_DEBUG) || defined(FAKELIGHT_PIXEL_DEBUG)
#include "FakeLightDebug.hlsl"
#endif

half3 GetChannelFakeLight(float3 position, float3 normal, half fakeLightTexChannel)
{
    half3 fakeLightChannel = 0;
    
    int fakeLightPosIndex = (int)(fakeLightTexChannel * 255 + 0.5h) - 1;
    if (fakeLightPosIndex < 0 || fakeLightPosIndex >= 32)
        return fakeLightChannel;

    float4 lightPosAndRange = _FakeLightPoses[fakeLightPosIndex];
    
    float3 fakeLightVector = lightPosAndRange.xyz - position;
    float distanceSqr = dot(fakeLightVector, fakeLightVector);
    float lightAttenuation = lightPosAndRange.w;
    float distance = sqrt(distanceSqr);
    
    float atten = 0;
    #ifdef LIGHT_DISTANCE_ATTENUATION_AFFECT_BY_RANGE
        float lightRange = _FakeLightRanges[fakeLightPosIndex];
        float f = max(0, lightRange - distance);
        atten = lightAttenuation * f * f;
    #else
        float lightAtten = rcp(distanceSqr);
        half factor = half(distanceSqr * lightAttenuation);
        half smoothFactor = saturate(half(1.0) - factor * factor);
        smoothFactor = smoothFactor * smoothFactor;
        atten = lightAtten * smoothFactor;
    #endif
    
    if(atten <= 0)
        return fakeLightChannel;

    float3 fakeLightDir = fakeLightVector / max(distance, 1e-5);
    half3 fakeLightColor = _FakeLightColors[fakeLightPosIndex].rgb;
    
    if(_FakeSpotLightAttens[fakeLightPosIndex].x < 0)
    {
        // 基于 XZ 索引的 FakeLight 面向地面/贴花，同高度点光方向几乎水平，不能用 Lambert
        fakeLightChannel = 1.0h;
    }
    else
    {   
        float3 spotLightDir = _FakeSpotLightDirs[fakeLightPosIndex].xyz;
        float angleAtten = saturate(dot(spotLightDir, fakeLightDir) * _FakeSpotLightAttens[fakeLightPosIndex].x + _FakeSpotLightAttens[fakeLightPosIndex].y);
        fakeLightChannel = angleAtten * angleAtten;
    }
    fakeLightChannel *= fakeLightColor * atten;
    
    return fakeLightChannel;
}

half3 GetFakeLight(float3 position, float3 normal)
{
    half3 fakeLight = 0;
    
    #if defined(_FAKE_ADDITIONAL_LIGHTS)
        float2 positionOffset = position.xz - _FakeLightRegionStart.xz;
        float2 uv = positionOffset * _FakeLightTexUVRate;
    
        half4 fakeLightTex = SAMPLE_FAKE_LIGHT_TEXTURE(uv);
        half regionMask = (uv.x > 0 && uv.x < 1 && uv.y > 0 && uv.y < 1) ? 1 : 0;
        fakeLightTex *= regionMask;
    
        if(fakeLightTex.r <= 0)
            return fakeLight;
        
        fakeLight += GetChannelFakeLight(position, normal, fakeLightTex.r);
        if(fakeLightTex.g <= 0)
            return fakeLight;
        
        fakeLight += GetChannelFakeLight(position, normal, fakeLightTex.g);
        if(fakeLightTex.b <= 0)
            return fakeLight;
        
        fakeLight += GetChannelFakeLight(position, normal, fakeLightTex.b);
        if(fakeLightTex.a <= 0)
            return fakeLight;
        
        fakeLight += GetChannelFakeLight(position, normal, fakeLightTex.a);
    #endif

    return fakeLight;
}
#endif
