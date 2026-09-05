#ifndef SSCS_LIBRARY
#define SSCS_LIBRARY

    #define dot2(x) dot(x,x)

    half GetRayIntersectionDistance(float3 rayOrigin, float3 rayDir, float altitude) {
        float dist = (altitude - rayOrigin.y) / rayDir.y;
        return dist;
    }

    half3 GetRayIntersection(float3 rayOrigin, float3 rayDir, float altitude) {
        float dist = GetRayIntersectionDistance(rayOrigin, rayDir, altitude);
        float3 hitPos = rayOrigin + rayDir * dist;
        return hitPos;
    }

    half SampleCloudTexture(sampler2D samp, float3 hitPos, float scale, float2 offset) {
        half x = tex2D(samp, hitPos.xz * scale + offset).x;
        half x2 = tex2D(samp, hitPos.xz * scale * 0.25 + offset + 0.17).x;
        x *= x2 * NOISE_MULTIPLIER; // two texture samples to break tiling
        return x;
    }

    half GetHaze(float3 hitPos, float2 windOffset, float edgeNoise, float animationSpeed, float scale, half contrast, half coverage, float2 texOffset) {
        hitPos.xz += windOffset;
        float3 edgePos = hitPos;
        edgePos.xz += _Time.y * float2(animationSpeed * 0.77, animationSpeed);
        half hazeNoise = SampleCloudTexture(_CloudsTex, edgePos, scale * 7.77, texOffset);
        half haze = SampleCloudTexture(_Clouds2Tex, hitPos, scale, texOffset);
        half noise = 1.0 - hazeNoise * edgeNoise;
        haze = lerp(haze * noise * 2.0, haze, haze);

        haze = max(0, haze - coverage);

        haze = saturate(0.5 + (haze - 0.5) * contrast);
         
        return haze;
    }

    half4 GetShadowAttenuation (float3 wpos, float depth01, float3 normalWS) {

        if (wpos.y < SHADOWS_MIN_ALTITUDE || wpos.y >= LAYER1_CLOUDS_ALTITUDE) return 1;

        // Compute shadow term
        float3 hitPos = GetRayIntersection(wpos, LIGHT_DIR, LAYER1_CLOUDS_ALTITUDE);
        half haze = GetHaze(hitPos, LAYER1_WIND_OFFSET, LAYER1_EDGE_NOISE, LAYER1_ANIMATION_SPEED, LAYER1_SCALE, LAYER1_CONTRAST, SHADOWS_COVERAGE, LAYER1_OFFSET);

        // Attenuate at dawn
        half dawn = saturate(-LIGHT_DIR.y * 2.0);
        
        #if _SHADOWS_3D || _SHADOWS_3D_HQ
            // Attenuate shadows on ceilings
            half floor = saturate( (normalWS.y + NORMAL_BIAS) * 10.0 );
            half shadow = saturate(haze * dawn * floor);
        #else
            half shadow = saturate(haze * dawn);
        #endif

        // Attenuate at distance
        float distanceAtten = saturate(CLOUDS_DISTANCE_FADE / dot2(depth01 * _ProjectionParams.z));
        shadow *= distanceAtten;

        #if _SHADOWS_COVERAGE_MASK || _SHADOWS_COVERAGE_MASK_DEBUG
            float2 maskUV = (wpos.xz - _MaskWorldData.yw) / _MaskWorldData.xz + 0.5;
            half mask = tex2D(_CoverageMask, maskUV).r;
            shadow *= mask;
        #endif

        // Apply shadow tint color
        half4 outputColor;
        outputColor.rgb = lerp(1.0, _ShadowColor.rgb, shadow * SHADOW_OPACITY);

        #if _SHADOWS_COVERAGE_MASK_DEBUG
            outputColor.rgb *= half3(0,0,0.5);
            outputColor.a = mask * 0.9;
        #else
            outputColor.a = 1.0;

            #if defined(USE_UNITY_FOG)
                // Compute & apply fog term
                float z = mul(UNITY_MATRIX_VP, float4(wpos, 1.0)).z;
                #ifdef USE_URP
                    outputColor.rgb = MixFog(outputColor.rgb, z);
                #else
                    UNITY_APPLY_FOG(z, outputColor);
                #endif
            #endif
        #endif

        return outputColor;
    }


#endif // SSCS_LIBRARY
