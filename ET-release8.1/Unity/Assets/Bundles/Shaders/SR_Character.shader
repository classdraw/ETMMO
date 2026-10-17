Shader "Custom/SR_Character"
{
    // 实时光：主光 Half-Lambert + URP ShadowMap；叠加 Scene Shadow RT、FakeLight、云阴影。
    Properties
    {
        [Header(Layers Bottom To Top)]
        _TailMap1("Tail Texture 1", 2D) = "white" {}
        [MainTexture] _BodyMap("Body Texture", 2D) = "white" {}
        [MainColor] _BodyColor("Body Tint", Color) = (1, 1, 1, 1)
        _HeadMap("Head Texture", 2D) = "white" {}
        _EquipMap1("Equip Texture 1", 2D) = "white" {}
        _EquipColor1("Equip Tint 1", Color) = (1, 1, 1, 1)
        _EquipMap2("Equip Texture 2", 2D) = "white" {}
        _EquipColor2("Equip Tint 2", Color) = (1, 1, 1, 1)
        _TailMap2("Tail Texture 2", 2D) = "white" {}
        _OcclusionFade("Occlusion Fade", Range(0, 1)) = 1

        [Header(Realtime Light)]
        [Toggle] _ReceiveShadows("Receive Realtime Shadows", Float) = 1

        [Header(Scene Shadow)]
        [Toggle] _UseSceneShadow("Use Scene Shadow", Float) = 1
        _SceneShadowChannelMask("Scene Shadow Channel Mask", Vector) = (1, 1, 1, 1)
        _SceneShadowColor("Scene Shadow Color", Color) = (0, 0, 0, 1)
        _SceneShadowIntensity("Scene Shadow Intensity", Range(0, 1)) = 0.5

        [Header(Grid m Rows n Columns)]
        _GridRows("Grid Rows M", Float) = 1
        _GridColumns("Grid Columns N", Float) = 1

        [Header(Playback From 0)]
        _Row("Row (0=bottom)", Float) = 0
        _StartColumn("Start Column (0=left)", Float) = 0
        _EndColumn("End Column (0=left)", Float) = 0

        [Header(Animation)]
        [Toggle] _Loop("Loop", Float) = 1
        _Interval("Interval (Seconds)", Float) = 0.1
        _AnimStartTime("Anim Start Time", Float) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Name "Character"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "SR_2DForwardPragmas.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../Scripts/ThirdParty/FakeLight/ShaderLibrary/FakeLightEnv.hlsl"

            #if defined(_SSCS_RECEIVE)
                #include "SR_CloudShadowsIntegration.hlsl"
            #endif
            #include "SR_CharacterCommon.hlsl"
            #include "SR_RealtimeSceneShadowLighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 3);
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.uv = input.uv;

                half3 normalWS = half3(0.0h, 1.0h, 0.0h);
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_SH(normalWS, output.vertexSH);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 atlasUv = SR_CharacterAtlasUV(input.uv);
                half4 color = SR_CharacterSampleComposite(atlasUv);

                half3 normalWS = half3(0.0h, 1.0h, 0.0h);
                Light mainLight = SR_GetMainLightRealtime(input.shadowCoord);
                half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, normalWS);

                color.rgb = SR_ApplySpriteRealtimeLighting(color.rgb, mainLight, normalWS, bakedGI);
                color.rgb += GetFakeLight(input.positionWS, normalWS);

                #if defined(_SSCS_RECEIVE)
                    color.rgb = ApplySSCSCloudShadow(color.rgb, input.positionWS, normalWS);
                #endif

                color.rgb = SR_ApplySceneShadowOnLit(
                    color.rgb,
                    input.positionCS,
                    _SceneShadowChannelMask,
                    _UseSceneShadow,
                    _SceneShadowColor.rgb,
                    _SceneShadowIntensity);

                return half4(color.rgb, color.a * _OcclusionFade);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "SR_CharacterCommon.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            ShadowVaryings ShadowVert(ShadowAttributes input)
            {
                ShadowVaryings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(float3(0, 1, 0));
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDir = normalize(_LightPosition - positionWS);
            #else
                float3 lightDir = _LightDirection;
            #endif
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDir));
            #if UNITY_REVERSED_Z
                output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                output.uv = input.uv;
                return output;
            }

            half4 ShadowFrag(ShadowVaryings input) : SV_Target
            {
                float2 atlasUv = SR_CharacterAtlasUV(input.uv);
                half alpha = SR_CharacterSampleComposite(atlasUv).a;
                clip(alpha - 0.001h);
                return 0;
            }
            ENDHLSL
        }
    }

    CustomEditor "SR_ReceiveShadowsShaderGUI"
}
