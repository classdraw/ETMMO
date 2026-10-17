Shader "Custom/SB_ProPixelizerUber"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        [Normal] _BumpMap("Normal Map", 2D) = "bump" {}
        [HDR] _EmissionColor("EmissionColor", Color) = (0, 0, 0, 1)
        _EmissionMap("Emission", 2D) = "white" {}
        _AlphaClipThreshold("Alpha Clip Threshold", Range(0, 1)) = 0.5
        _DiffuseVertexColorWeight("DiffuseVertexColorWeight", Range(0, 1)) = 1
        _EmissiveVertexColorWeight("EmissiveVertexColorWeight", Range(0, 1)) = 0

        [Header(Specular)]
        [Toggle(_SPECULAR_SETUP)] _WorkflowMode("Specular Setup", Float) = 1
        _SpecColor("SpecularColor", Color) = (0.2, 0.2, 0.2, 1)
        _SpecGlossMap("SpecularMap", 2D) = "white" {}
        _MetallicGlossMap("MetallicMap", 2D) = "white" {}
        _Metallic("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0.5

        [Header(Outlines)]
        _OutlineColor("OutlineColor", Color) = (0.22, 0.22, 0.22, 1)
        _EdgeHighlightColor("Edge Highlight Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _EdgeBevelWeight("EdgeBevelWeight", Float) = 0

        [Header(ProPixelizer)]
        _LightingRamp("LightingRamp", 2D) = "white" {}
        _PaletteLUT("PaletteLUT", 2D) = "white" {}
        _AmbientLight("AmbientLight", Color) = (0.2, 0.2, 0.2, 1)
        [IntRange] _PixelSize("PixelSize", Range(1, 5)) = 3
        _PixelGridOrigin("PixelGridOrigin", Vector) = (0, 0, 0, 0)
        [IntRange] _ID("Object ID", Range(0, 255)) = 1
        [Toggle] _COLOR_GRADING("Use Color Grading", Float) = 0
        [Toggle] _PROPIXELIZER_DITHERING("Use Dithering", Float) = 1
        [Toggle] _RECEIVE_SHADOWS("Receive Realtime Shadows", Float) = 1
        [Toggle] _USE_OBJECT_POSITION_FOR_PIXEL_GRID("Use Object Position for Pixel Grid", Float) = 0

        [Header(Scene Shadow)]
        [Toggle] _UseSceneShadow("Use Scene Shadow", Float) = 1
        _SceneShadowChannelMask("Scene Shadow Channel Mask", Vector) = (1, 0, 0, 0)
        _SceneShadowColor("Scene Shadow Color", Color) = (0, 0, 0, 1)
        _SceneShadowIntensity("Scene Shadow Intensity", Range(0, 1)) = 0.5

        [HideInInspector] _BodyMap("Body Texture (legacy)", 2D) = "white" {}
        [HideInInspector] _MainTex("MainTex", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "ProPixelizer"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma target 2.5
            #pragma vertex SB_Vert
            #pragma fragment SB_Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma shader_feature_local _ _SPECULAR_SETUP
            #pragma shader_feature_local _ _COLOR_GRADING_ON
            #pragma shader_feature_local _ _PROPIXELIZER_DITHERING_ON
            #pragma shader_feature_local _ _RECEIVE_SHADOWS_ON
            #pragma multi_compile_local USE_OBJECT_POSITION_FOR_PIXEL_GRID_ON _

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SB_ProPixelizerUberForward.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ProPixelizerPass"
            Tags
            {
                "LightMode" = "ProPixelizer"
                "DisableBatching" = "True"
            }

            ZWrite On
            Cull Off
            Blend Off

            HLSLPROGRAM
            #pragma vertex outline_vert
            #pragma fragment outline_frag
            #pragma target 2.5
            #pragma multi_compile_local USE_OBJECT_POSITION_FOR_PIXEL_GRID_ON _
            #pragma shader_feature_local _ _PROPIXELIZER_DITHERING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../Art/ProPixelizer/SRP/ShaderLibrary/PixelUtils.hlsl"
            #include "../../Art/ProPixelizer/SRP/ShaderLibrary/PackingUtils.hlsl"
            #include "../../Art/ProPixelizer/SRP/ShaderLibrary/ScreenUtils.hlsl"
            #include "SB_ProPixelizerUberInput.hlsl"
            #include "../../Art/ProPixelizer/SRP/ShaderLibrary/OutlinePass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            #include "SB_ProPixelizerUberInput.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDir = normalize(_LightPosition - positionWS);
            #else
                float3 lightDir = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDir));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                output.positionCS = positionCS;
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half alpha = baseSample.a * _BaseColor.a;
                half bodyAlpha = SAMPLE_TEXTURE2D(_BodyMap, sampler_BodyMap, input.uv).a * _BaseColor.a;
                alpha = max(alpha, bodyAlpha);
                clip(alpha - _AlphaClipThreshold);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SB_ProPixelizerUberInput.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 DepthFrag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                clip(alpha - _AlphaClipThreshold);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthNormalsVert
            #pragma fragment DepthNormalsFrag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "SB_ProPixelizerUberInput.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 tangentWS : TEXCOORD2;
            };

            Varyings DepthNormalsVert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = posInputs.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = normInputs.normalWS;
                output.tangentWS = half4(normInputs.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                return output;
            }

            half4 DepthNormalsFrag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).a * _BaseColor.a;
                clip(alpha - _AlphaClipThreshold);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv));
                half3 bitangentWS = input.tangentWS.w * cross(input.normalWS, input.tangentWS.xyz);
                half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangentWS, input.normalWS);
                half3 normalWS = NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, tangentToWorld));
                return half4(normalWS * 0.5 + 0.5, 0);
            }
            ENDHLSL
        }
    }

    FallBack Off
    CustomEditor "SB_ProPixelizerUberShaderGUI"
}
