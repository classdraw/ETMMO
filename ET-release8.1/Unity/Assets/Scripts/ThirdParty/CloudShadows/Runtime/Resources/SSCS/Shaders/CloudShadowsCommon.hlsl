#ifndef SSCS_COMMON
#define SSCS_COMMON

    UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
    float4 _CameraDepthTexture_TexelSize;

    float GetRawDepth(float2 uv) {
        float depth = UNITY_SAMPLE_DEPTH(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, uv));
        return depth;
    }

    float GetLinearDepth(float2 uv) {
        float rawDepth = GetRawDepth(uv);
        #if _ORTHOGRAPHIC
            #if UNITY_REVERSED_Z
                rawDepth = 1.0 - rawDepth;
            #endif
            return rawDepth;
        #else
            return Linear01Depth(rawDepth);
        #endif
    }

    bool IsSkyBox(float depth01) {
        return depth01 >= 1.0;
    }

    float3 GetWorldPosition(float3 iwpos, float depth01) {
        #if _ORTHOGRAPHIC
            float3 wpos = iwpos - unity_WorldToCamera._m20_m21_m22 * ((1.0 - depth01) * _ProjectionParams.z);
        #else
            float3 wpos = _WorldSpaceCameraPos + (iwpos - _WorldSpaceCameraPos) * depth01;
        #endif
        return wpos;
    }

    float3 GetViewSpacePos(float2 uv) {
        float3 viewSpaceRay = mul(unity_CameraInvProjection, float4(uv * 2.0 - 1.0, 1.0, 1.0) * _ProjectionParams.z).xyz;
        return viewSpaceRay * GetLinearDepth(uv);
    }

    half3 GetViewSpaceNormal(float2 uv) {
        float3 posVS_M = GetViewSpacePos(uv);
        float3 posVS_E = GetViewSpacePos(uv + float2( 1.0, 0.0) * _CameraDepthTexture_TexelSize.xy);
        float3 posVS_N = GetViewSpacePos(uv + float2( 0.0, 1.0) * _CameraDepthTexture_TexelSize.xy);

        float3 hDeriv = posVS_E - posVS_M;
        float3 vDeriv = posVS_N - posVS_M;

        half3 viewNormal = normalize(cross(hDeriv, vDeriv));
        return viewNormal;
    }

    half3 GetViewSpaceNormalHQ(float2 uv) {

        float c = GetRawDepth(uv);

        half3 posVS_M = GetViewSpacePos(uv);
        half3 posVS_W = GetViewSpacePos(uv + float2(-1.0, 0.0) * _CameraDepthTexture_TexelSize.xy);
        half3 posVS_E = GetViewSpacePos(uv + float2( 1.0, 0.0) * _CameraDepthTexture_TexelSize.xy);
        half3 posVS_S = GetViewSpacePos(uv + float2( 0.0,-1.0) * _CameraDepthTexture_TexelSize.xy);
        half3 posVS_N = GetViewSpacePos(uv + float2( 0.0, 1.0) * _CameraDepthTexture_TexelSize.xy);

        half3 l = posVS_M - posVS_W;
        half3 r = posVS_E - posVS_M;
        half3 d = posVS_M - posVS_S;
        half3 u = posVS_N - posVS_M;

        half4 horiz = half4(
              GetRawDepth(uv + float2(-1.0, 0.0) * _CameraDepthTexture_TexelSize.xy),
              GetRawDepth(uv + float2( 1.0, 0.0) * _CameraDepthTexture_TexelSize.xy),
              GetRawDepth(uv + float2(-2.0, 0.0) * _CameraDepthTexture_TexelSize.xy),
              GetRawDepth(uv + float2( 2.0, 0.0) * _CameraDepthTexture_TexelSize.xy)
        );

        half4 vert = half4(
              GetRawDepth(uv + float2(0.0,-1.0) * _CameraDepthTexture_TexelSize.xy),
              GetRawDepth(uv + float2(0.0, 1.0) * _CameraDepthTexture_TexelSize.xy),
              GetRawDepth(uv + float2(0.0,-2.0) * _CameraDepthTexture_TexelSize.xy),
              GetRawDepth(uv + float2(0.0, 2.0) * _CameraDepthTexture_TexelSize.xy)
        );

        half2 he = abs((2 * horiz.xy - horiz.zw) - c);
        half2 ve = abs((2 * vert.xy - vert.zw) - c);

        half3 hDeriv = he.x < he.y ? l : r;
        half3 vDeriv = ve.x < ve.y ? d : u;

        half3 viewNormal = normalize(cross(hDeriv, vDeriv));
        return viewNormal;
    }

    float3 GetWorldNormal(float2 uv) {
        #if _SHADOWS_3D_HQ
            half3 viewNormal = GetViewSpaceNormalHQ(uv);
        #else
            half3 viewNormal = GetViewSpaceNormal(uv);
        #endif
        half3 normalWS = mul((float3x3)unity_MatrixInvV, viewNormal);
        return normalWS;
    }



#endif // SSCS

