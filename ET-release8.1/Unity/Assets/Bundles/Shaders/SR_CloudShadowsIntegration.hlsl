#ifndef SR_CLOUDSHADOWS_INTEGRATION_INCLUDED
#define SR_CLOUDSHADOWS_INTEGRATION_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "../../Scripts/ThirdParty/CloudShadows/Runtime/Resources/SSCS/Shaders/CloudShadowsInput.hlsl"
#include "../../Scripts/ThirdParty/CloudShadows/Runtime/Resources/SSCS/Shaders/CloudShadowsLibrary.hlsl"

float GetSSCSDepth01(float3 positionWS)
{
    if (unity_OrthoParams.w > 0.0)
    {
        float horizDist = length(positionWS.xz - _WorldSpaceCameraPos.xz);
        float depth01 = horizDist / max(_ProjectionParams.z, 1.0);
        return min(depth01, 0.35);
    }

    return distance(_WorldSpaceCameraPos, positionWS) / max(_ProjectionParams.z, 1.0);
}

half3 ApplySSCSCloudShadow(half3 litColor, float3 positionWS, half3 normalWS)
{
    if (_SSCS_ReceiveActive < 0.5)
    {
        return litColor;
    }

    if (unity_OrthoParams.w > 0.0)
    {
        normalWS = half3(0.0h, 1.0h, 0.0h);
    }

    float depth01 = GetSSCSDepth01(positionWS);
    half4 cloudShadow = GetShadowAttenuation(positionWS, depth01, normalWS);
    return litColor * cloudShadow.rgb;
}

#endif
