Shader "Custom/SR_WorldSpriteTransparent"
{
    // Cloud shadows: GetShadowAttenuation() via SR_CloudShadowsIntegration when _SSCS_RECEIVE is enabled.
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Tint", Color) = (1, 1, 1, 1)
        _OcclusionFade("Occlusion Fade", Range(0, 1)) = 1
        _BorderClip("Border Clip (Left, Top, Right, Bottom)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "WorldSpriteTransparent"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _SSCS_RECEIVE
            #pragma multi_compile_fragment _ _SHADOWS_3D _SHADOWS_3D_HQ
            #pragma multi_compile_fragment _ _SHADOWS_COVERAGE_MASK _SHADOWS_COVERAGE_MASK_DEBUG
            #pragma multi_compile_fragment _ _BOUNDS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #if defined(_SSCS_RECEIVE)
                #include "SR_CloudShadowsIntegration.hlsl"
            #endif

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _OcclusionFade;
                float4 _BorderClip;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 meshUV : TEXCOORD5;
                float3 positionWS : TEXCOORD6;
                float3 normalWS : TEXCOORD1;
                half fogFactor : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 4);
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.meshUV = input.uv;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = normalInputs.normalWS;
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.shadowCoord = GetShadowCoord(positionInputs);

                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_SH(output.normalWS, output.vertexSH);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Border clip uses mesh UV (0-1). x=left, y=top, z=right, w=bottom.
                float2 meshUV = input.meshUV;
                float4 borderClip = _BorderClip;
                clip(min(
                    min(meshUV.x - borderClip.x, (1.0 - borderClip.z) - meshUV.x),
                    min(meshUV.y - borderClip.w, (1.0 - borderClip.y) - meshUV.y)));

                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half3 albedo = color.rgb;
                half alpha = color.a * _OcclusionFade;

                half3 normalWS = normalize(input.normalWS);
                Light mainLight = GetMainLight(input.shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 mainLightColor = mainLight.color * mainLight.distanceAttenuation * mainLight.shadowAttenuation * ndotl;

                half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, normalWS);
                MixRealtimeAndBakedGI(mainLight, normalWS, bakedGI);

                half3 litColor = albedo * (bakedGI + mainLightColor);

                #if defined(_SSCS_RECEIVE)
                    litColor = ApplySSCSCloudShadow(litColor, input.positionWS, normalWS);
                #endif

                litColor = MixFog(litColor, input.fogFactor);
                return half4(litColor, alpha);
            }
            ENDHLSL
        }
    }
}
