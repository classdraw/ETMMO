#ifndef SR_CLOUDSHADOWS_INTEGRATION_INCLUDED
#define SR_CLOUDSHADOWS_INTEGRATION_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "../../Scripts/ThirdParty/CloudShadows/Runtime/Resources/SSCS/Shaders/CloudShadowsInput.hlsl"
#include "../../Scripts/ThirdParty/CloudShadows/Runtime/Resources/SSCS/Shaders/CloudShadowsLibrary.hlsl"

float GetSSCSDepth01(float3 positionWS)
{
    if (unity_OrthoParams.w > 0.0)
    {
        return length(positionWS.xz - _WorldSpaceCameraPos.xz) / max(_ProjectionParams.z, 1.0);
    }

    return distance(_WorldSpaceCameraPos, positionWS) / max(_ProjectionParams.z, 1.0);
}

half3 ApplySSCSCloudShadow(half3 litColor, float3 positionWS, half3 normalWS)
{
    float depth01 = GetSSCSDepth01(positionWS);
    half4 cloudShadow = GetShadowAttenuation(positionWS, depth01, normalWS);
    return litColor * cloudShadow.rgb;
}

#endif
