#ifndef SSCS_PASS
#define SSCS_PASS

    #define USE_UNITY_FOG
    #include "CloudShadowsLibrary.hlsl"

    struct AttributesSimple {
        float4 vertex : POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

	struct VaryingsShadows {
	    float4 pos : SV_POSITION;
        float3 wpos: TEXCOORD0;
        float4 scrPos: TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
	};

	VaryingsShadows VertSimple(AttributesSimple v) {
    	VaryingsShadows o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	    o.pos = UnityObjectToClipPos(v.vertex);
        o.scrPos = ComputeScreenPos(o.pos);

        o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;

        #if defined(UNITY_REVERSED_Z)
		    o.pos.z = o.pos.w * UNITY_NEAR_CLIP_VALUE * 0.99999; //  0.99999 avoids precision issues on some Android devices causing unexpected clipping of light mesh
		#else
		    o.pos.z = o.pos.w - 1.0e-6f;
		#endif

    	return o;
	}

    half4 FragShadows (VaryingsShadows i) : SV_Target {
        #ifndef _SSCS_SHOW_SHADOWS
            return 1;
        #endif

        float2 uv = i.scrPos.xy / i.scrPos.w;
        float depth01 = GetLinearDepth(uv);
        if (IsSkyBox(depth01)) return 1;
        
        float3 wpos = GetWorldPosition(i.wpos, depth01);

        #if _BOUNDS
            if (any (floor( (wpos.xz - BOUNDS_CENTER) / BOUNDS_SIZE + 0.5)) != 0) return 1;
        #endif

        #if _SHADOWS_3D || _SHADOWS_3D_HQ
            // Attenuate shadows on ceilings
            float3 normalWS = GetWorldNormal(uv);
        #else
            float3 normalWS = (float3)0;
        #endif
        half4 outputColor = GetShadowAttenuation(wpos, depth01, normalWS);
        #if _PRESERVE_DIRECTIONAL_SHADOWS && (_MAIN_LIGHT_SHADOWS || _MAIN_LIGHT_SHADOWS_CASCADE)
	        float4 shadowCoord = TransformWorldToShadowCoord(wpos);
	        float atten = MainLightRealtimeShadow(shadowCoord);
            float shadowPreserve = (1.0 - atten) * SHADOWS_PRESERVE_DIRECTIONAL;
            shadowPreserve = saturate(shadowPreserve * (1.0 - fwidth(shadowPreserve) * 10.0));
            //if (fwidth(shadowPreserve) > 0.0001) shadowPreserve = 0.2;
            outputColor = lerp(outputColor, 1.0, shadowPreserve);
        #endif
        return outputColor;
    }

    half3 GetBumpMap(float3 hitPos, float2 windOffset, float scale, float2 texOffset, float animationSpeed) {
        hitPos.xz += windOffset;
        hitPos.xz += _Time.y * float2(animationSpeed * 0.77, animationSpeed);
        half3 nrm = UnpackNormal(tex2D(_CloudsBumpMap, hitPos.xz * scale + texOffset));
        half3 nrm2 = UnpackNormal(tex2D(_CloudsBumpMap, hitPos.xz * scale * 2.0 + texOffset + 0.33));
        nrm = (nrm * 0.666 + nrm2 * 0.333).xzy;
        return nrm;
    }

    half4 ComposeCloudOutput(float3 hitPos, float3 camPos, float3 rayDir, half haze) {
        #if _CLOUDS_THICK
            hitPos -= rayDir * haze * CLOUD_THICKNESS;
            half haze2 = GetHaze(hitPos, LAYER1_WIND_OFFSET, LAYER1_EDGE_NOISE, LAYER1_ANIMATION_SPEED, LAYER1_SCALE, LAYER1_CONTRAST, LAYER1_COVERAGE, LAYER1_OFFSET);
            haze = lerp(haze, haze + haze2 * (1.0 - haze), haze * 4.0);
        #endif

        half3 bumpMap = GetBumpMap(hitPos, LAYER1_WIND_OFFSET, BUMPMAP_SCALE, LAYER1_OFFSET, LAYER1_ANIMATION_SPEED);
        bumpMap.y = lerp(-bumpMap.y * 0.1, bumpMap.y, saturate(haze / 0.45));
        bumpMap = normalize(bumpMap);

        half diff = dot(bumpMap, -LIGHT_DIR) * 0.5 + 0.5;

        #if _CLOUDS_ANISOTROPY
            diff += pow(max(0, dot(-LIGHT_DIR, rayDir)), CLOUDS_ANISOTROPY);
        #endif

        half3 albedo = lerp(_CloudsLightColor, _CloudsDarkColor, smoothstep(0, 1, haze));
        half4 outputColor = half4(albedo * _LightColor * (diff * CLOUDS_LIGHT_INTENSITY), haze * 4.0);

        half dawn = saturate(-LIGHT_DIR.y * 2.0) + AMBIENT_LIGHT;
        #if _ORTHOGRAPHIC || _ORTHOGRAPHIC_OVERLAY
            dawn = max(dawn, max(AMBIENT_LIGHT, 0.35));
        #endif

        outputColor.rgb *= dawn;
        outputColor = saturate(outputColor);

        float distanceAtten = saturate(CLOUDS_DISTANCE_FADE / dot2(hitPos.xz - camPos.xz));
        outputColor *= distanceAtten;
        outputColor.a *= CLOUDS_OPACITY;
        return outputColor;
    }

    half4 FragClouds (VaryingsShadows i) : SV_Target {

        #if _ORTHOGRAPHIC_OVERLAY
            float3 camPos = _WorldSpaceCameraPos.xyz;
            float3 hitPos = float3(i.wpos.x, LAYER1_CLOUDS_ALTITUDE, i.wpos.z);
            float3 rayDir = float3(0.0, -1.0, 0.0);

            #if _BOUNDS
                if (any(floor((hitPos.xz - BOUNDS_CENTER) / BOUNDS_SIZE + 0.5) != 0)) return 0;
            #endif

            half haze = GetHaze(hitPos, LAYER1_WIND_OFFSET, LAYER1_EDGE_NOISE, LAYER1_ANIMATION_SPEED, LAYER1_SCALE, LAYER1_CONTRAST, LAYER1_COVERAGE, LAYER1_OFFSET);
            clip(haze - 0.001);
            return ComposeCloudOutput(hitPos, camPos, rayDir, haze);
        #endif

        float2 uv = i.scrPos.xy / i.scrPos.w;
        float depth01 = GetLinearDepth(uv);

        float3 wpos = GetWorldPosition(i.wpos, depth01);

        #if _BOUNDS
            if (any (floor( (wpos.xz - BOUNDS_CENTER) / BOUNDS_SIZE + 0.5)) != 0) return 0;
        #endif

        #if _ORTHOGRAPHIC
            float3 rayDir = unity_WorldToCamera._m20_m21_m22;
            float3 camPos = _WorldSpaceCameraPos.xyz;

            // Camera must sit above the cloud layer (typical top-down setup).
            if (camPos.y <= LAYER1_CLOUDS_ALTITUDE + 0.01) return 0;

            // Top-down ortho: parallel rays, project clouds onto XZ at cloud altitude.
            if (abs(rayDir.y) > 0.99) {
                if (!IsSkyBox(depth01) && wpos.y > LAYER1_CLOUDS_ALTITUDE + 0.01) return 0;
            } else {
                float planeDistance = GetRayIntersectionDistance(camPos, rayDir, LAYER1_CLOUDS_ALTITUDE);
                float distAlongRay = dot(wpos - camPos, rayDir);
                if (distAlongRay + 0.01 < planeDistance) return 0;
                if (!IsSkyBox(depth01) && wpos.y > LAYER1_CLOUDS_ALTITUDE + 0.01) return 0;
            }
        #else
            float3 camPos = _WorldSpaceCameraPos.xyz;
            float3 ray = wpos - camPos;
            float3 rayDir = normalize(ray);

            float planeDistance = GetRayIntersectionDistance(camPos, rayDir, LAYER1_CLOUDS_ALTITUDE);
            if (distance(camPos, wpos) < planeDistance) return 0;
        #endif

        float heightDiff = camPos.y - LAYER1_CLOUDS_ALTITUDE;
        if (heightDiff * rayDir.y >= 0) return 0;

        #if _ORTHOGRAPHIC
            float3 hitPos = abs(rayDir.y) > 0.99
                ? float3(wpos.x, LAYER1_CLOUDS_ALTITUDE, wpos.z)
                : camPos + rayDir * GetRayIntersectionDistance(camPos, rayDir, LAYER1_CLOUDS_ALTITUDE);
        #else
            float3 hitPos = camPos + rayDir * GetRayIntersectionDistance(camPos, rayDir, LAYER1_CLOUDS_ALTITUDE);
        #endif
        half haze = GetHaze(hitPos, LAYER1_WIND_OFFSET, LAYER1_EDGE_NOISE, LAYER1_ANIMATION_SPEED, LAYER1_SCALE, LAYER1_CONTRAST, LAYER1_COVERAGE, LAYER1_OFFSET);
        clip(haze - 0.001);
        return ComposeCloudOutput(hitPos, camPos, rayDir, haze);
}

#endif // SSCS_PASS