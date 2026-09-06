// Tilemap / Sprite lit shader for URP Forward Renderer.
// Based on Sprite-Lit-Default properties, with ambient + main light in UniversalForward.
// Cloud shadows: include local SSCS GetShadowAttenuation() when _SSCS_RECEIVE is enabled.
// Scene shadows: sample global _SceneShadowRT; material Vector4 channel mask matches RT RGBA channels.
Shader "Custom/SR_TilemapSpriteLit"{
    Properties
    {
        [MainTexture] _MainTex("Diffuse", 2D) = "white" {}
        [MainColor] _Color("Tint", Color) = (1, 1, 1, 1)
        _MaskTex("Mask", 2D) = "white" {}
        [Normal] _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalScale("Normal Scale", Float) = 1.0
        [Toggle] _ZWrite("ZWrite", Float) = 0

        [Header(Scene Shadow)]
        [Toggle] _UseSceneShadow("Use Scene Shadow", Float) = 1
        _SceneShadowChannelMask("Scene Shadow Channel Mask", Vector) = (1, 1, 1, 1)
        _SceneShadowColor("Scene Shadow Color", Color) = (0, 0, 0, 1)
        _SceneShadowIntensity("Scene Shadow Intensity", Range(0, 1)) = 0.5

        // Legacy sprite properties for material migration from Sprite-Lit-Default.
        [HideInInspector] _RendererColor("RendererColor", Color) = (1, 1, 1, 1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]
        ZTest LEqual

        Pass
        {
            Name "TilemapSpriteLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _SSCS_RECEIVE
            #pragma multi_compile_fragment _ _SHADOWS_3D _SHADOWS_3D_HQ
            #pragma multi_compile_fragment _ _SHADOWS_COVERAGE_MASK _SHADOWS_COVERAGE_MASK_DEBUG
            #pragma multi_compile_fragment _ _BOUNDS
            #pragma shader_feature_local _NORMALMAP
            #pragma multi_compile _ _FAKE_ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../../Scripts/ThirdParty/FakeLight/ShaderLibrary/FakeLightEnv.hlsl"

            #if defined(_SSCS_RECEIVE)
                #include "SR_CloudShadowsIntegration.hlsl"
            #endif
            #include "SR_SceneShadowIntegration.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _RendererColor;
                half _NormalScale;
                half _UseSceneShadow;
                half _SceneShadowIntensity;
                half4 _SceneShadowColor;
                half4 _SceneShadowChannelMask;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 staticLightmapUV : TEXCOORD1;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half4 tangentWS : TEXCOORD3;
                half fogFactor : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
                half4 vertexColor : COLOR;
                DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 6);
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            half3 GetNormalWS(Varyings input)
            {
                half3 normalWS = normalize(input.normalWS);

                #if defined(_NORMALMAP)
                    half3 normalTS = UnpackNormalScale(
                        SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv),
                        _NormalScale);
                    half3 bitangentWS = input.tangentWS.w * cross(normalWS, input.tangentWS.xyz);
                    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangentWS, normalWS);
                    normalWS = normalize(TransformTangentToWorld(normalTS, tangentToWorld));
                #endif

                return normalWS;
            }

            half3 GetAdditionalLighting(half3 positionWS, half3 normalWS)
            {
                half3 lighting = half3(0.0h, 0.0h, 0.0h);

                #if defined(_ADDITIONAL_LIGHTS)
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, positionWS);
                        half ndotl = saturate(dot(normalWS, light.direction));
                        lighting += light.color * (light.distanceAttenuation * light.shadowAttenuation * ndotl);
                    }
                #endif

                return lighting;
            }

            Varyings Vert(Attributes input)            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = half4(normalInputs.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.vertexColor = input.color * _Color * _RendererColor;

                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_SH(output.normalWS, output.vertexSH);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv).a;
                half4 color = mainTex * input.vertexColor;
                color.a *= mask;

                clip(color.a - 0.001h);

                half3 albedo = color.rgb;
                half3 normalWS = GetNormalWS(input);

                Light mainLight = GetMainLight(input.shadowCoord);
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half3 mainLightColor = mainLight.color
                    * mainLight.distanceAttenuation
                    * mainLight.shadowAttenuation
                    * ndotl;

                half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, normalWS);
                MixRealtimeAndBakedGI(mainLight, normalWS, bakedGI);

                half3 lighting = bakedGI + mainLightColor + GetAdditionalLighting(input.positionWS, normalWS);
                half3 litColor = albedo * lighting;
                litColor += GetFakeLight(input.positionWS, normalWS);

                #if defined(_SSCS_RECEIVE)
                    litColor = ApplySSCSCloudShadow(litColor, input.positionWS, normalWS);
                #endif

                litColor = ApplySRSceneShadow(
                    litColor,
                    input.positionCS,
                    _SceneShadowChannelMask,
                    _UseSceneShadow,
                    _SceneShadowColor.rgb,
                    _SceneShadowIntensity);

                litColor = MixFog(litColor, input.fogFactor);                return half4(litColor, color.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half4 _RendererColor;
                half _NormalScale;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 vertexColor : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.vertexColor = input.color * _Color * _RendererColor;
                return output;
            }

            half4 DepthFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, input.uv).a;
                clip(mainTex.a * input.vertexColor.a * mask - 0.001h);
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
