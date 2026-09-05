#ifndef SSCS_PASS_CLOUDS
#define SSCS_PASS_CLOUDS

#include "CloudShadowsLibrary.hlsl"

struct AttributesSimple {
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsClouds {
    float4 pos : SV_POSITION;
    float3 wpos : TEXCOORD0;
    float4 scrPos : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

VaryingsClouds VertClouds(AttributesSimple v) {
    VaryingsClouds o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.scrPos = ComputeScreenPos(o.pos);
    o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;

    #if defined(UNITY_REVERSED_Z)
        o.pos.z = o.pos.w * UNITY_NEAR_CLIP_VALUE * 0.99999;
    #else
        o.pos.z = o.pos.w - 1.0e-6f;
    #endif

    return o;
}

half3 GetCloudBumpMap(float3 hitPos, float2 windOffset, float scale, float2 texOffset, float animationSpeed) {
    hitPos.xz += windOffset;
    hitPos.xz += _Time.y * float2(animationSpeed * 0.77, animationSpeed);
    half3 nrm = UnpackNormal(tex2D(_CloudsBumpMap, hitPos.xz * scale + texOffset));
    half3 nrm2 = UnpackNormal(tex2D(_CloudsBumpMap, hitPos.xz * scale * 2.0 + texOffset + 0.33));
    nrm = (nrm * 0.666 + nrm2 * 0.333).xzy;
    return nrm;
}

half4 FragClouds(VaryingsClouds i) : SV_Target {
    float2 uv = i.scrPos.xy / i.scrPos.w;
    float depth01 = GetLinearDepth(uv);
    float3 wpos = GetWorldPosition(i.wpos, depth01);

    #if _BOUNDS
        if (any(floor((wpos.xz - BOUNDS_CENTER) / BOUNDS_SIZE + 0.5) != 0)) return 0;
    #endif

    #if _ORTHOGRAPHIC
        float3 rayDir = unity_WorldToCamera._m20_m21_m22;
        float3 camPos = i.wpos - rayDir * _ProjectionParams.z;
    #else
        float3 camPos = _WorldSpaceCameraPos.xyz;
        float3 ray = wpos - camPos;
        float3 rayDir = normalize(ray);
    #endif

    float heightDiff = camPos.y - LAYER1_CLOUDS_ALTITUDE;
    if (heightDiff * rayDir.y >= 0) return 0;

    float planeDistance = GetRayIntersectionDistance(camPos, rayDir, LAYER1_CLOUDS_ALTITUDE);

    #if _ORTHOGRAPHIC
        // Perspective demo uses Euclidean distance; for ortho top-down that rejects every pixel.
        // Compare scene depth along the view ray instead (matches Kronnect intent).
        float distAlongRay = dot(wpos - camPos, -rayDir);
        if (distAlongRay + 0.01 < planeDistance) return 0;
        if (!IsSkyBox(depth01) && wpos.y > LAYER1_CLOUDS_ALTITUDE + 0.01) return 0;
    #else
        if (distance(camPos, wpos) < planeDistance) return 0;
    #endif

    float3 hitPos = camPos + rayDir * planeDistance;
    half haze = GetHaze(hitPos, LAYER1_WIND_OFFSET, LAYER1_EDGE_NOISE, LAYER1_ANIMATION_SPEED, LAYER1_SCALE, LAYER1_CONTRAST, LAYER1_COVERAGE, LAYER1_OFFSET);

    #if _CLOUDS_THICK
        hitPos -= rayDir * haze * CLOUD_THICKNESS;
        half haze2 = GetHaze(hitPos, LAYER1_WIND_OFFSET, LAYER1_EDGE_NOISE, LAYER1_ANIMATION_SPEED, LAYER1_SCALE, LAYER1_CONTRAST, LAYER1_COVERAGE, LAYER1_OFFSET);
        haze = lerp(haze, haze + haze2 * (1.0 - haze), haze * 4.0);
    #endif

    clip(haze - 0.001);

    half3 bumpMap = GetCloudBumpMap(hitPos, LAYER1_WIND_OFFSET, BUMPMAP_SCALE, LAYER1_OFFSET, LAYER1_ANIMATION_SPEED);
    bumpMap.y = lerp(-bumpMap.y * 0.1, bumpMap.y, saturate(haze / 0.45));
    bumpMap = normalize(bumpMap);

    half diff = dot(bumpMap, -LIGHT_DIR) * 0.5 + 0.5;

    #if _CLOUDS_ANISOTROPY
        diff += pow(max(0, dot(-LIGHT_DIR, rayDir)), CLOUDS_ANISOTROPY);
    #endif

    half3 albedo = lerp(_CloudsLightColor, _CloudsDarkColor, smoothstep(0, 1, haze));
    half4 outputColor = half4(albedo * _LightColor * (diff * CLOUDS_LIGHT_INTENSITY), haze * 4.0);

    half dawn = saturate(-LIGHT_DIR.y * 2.0) + AMBIENT_LIGHT;
    #if _ORTHOGRAPHIC
        // Top-down 2D sun is usually horizontal; original dawn drives RGB ~ 0 while alpha stays.
        dawn = max(dawn, max(AMBIENT_LIGHT, 0.35));
    #endif
    outputColor.rgb *= dawn;
    outputColor = saturate(outputColor);

    float distanceAtten = saturate(CLOUDS_DISTANCE_FADE / dot2(hitPos.xz - camPos.xz));
    outputColor *= distanceAtten;

    outputColor.a *= CLOUDS_OPACITY;
    return outputColor;
}

#endif
