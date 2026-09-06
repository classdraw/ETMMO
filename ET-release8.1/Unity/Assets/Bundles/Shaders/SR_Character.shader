Shader "Custom/SR_Character"
{
    // Cloud shadows: GetShadowAttenuation() via SR_CloudShadowsIntegration when _SSCS_RECEIVE is enabled.
    // Scene shadows: sample global _SceneShadowRT; material Vector4 channel mask matches RT RGBA channels.
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ _SSCS_RECEIVE
            #pragma multi_compile_fragment _ _SHADOWS_3D _SHADOWS_3D_HQ
            #pragma multi_compile_fragment _ _SHADOWS_COVERAGE_MASK _SHADOWS_COVERAGE_MASK_DEBUG
            #pragma multi_compile_fragment _ _BOUNDS
            #pragma multi_compile _ _FAKE_ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "../../Scripts/ThirdParty/FakeLight/ShaderLibrary/FakeLightEnv.hlsl"

            #if defined(_SSCS_RECEIVE)
                #include "SR_CloudShadowsIntegration.hlsl"
            #endif
            #include "SR_SceneShadowIntegration.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _TailMap1_TexelSize;
                float4 _BodyMap_ST;
                float4 _BodyMap_TexelSize;
                float4 _HeadMap_TexelSize;
                float4 _EquipMap1_TexelSize;
                float4 _EquipMap2_TexelSize;
                float4 _TailMap2_TexelSize;
                half4 _BodyColor;
                half4 _EquipColor1;
                half4 _EquipColor2;
                float _OcclusionFade;
                float _GridRows;
                float _GridColumns;
                float _Row;
                float _StartColumn;
                float _EndColumn;
                float _Loop;
                float _Interval;
                half _UseSceneShadow;
                half _SceneShadowIntensity;
                half4 _SceneShadowColor;
                half4 _SceneShadowChannelMask;
            CBUFFER_END

            TEXTURE2D(_TailMap1);
            SAMPLER(sampler_TailMap1);
            TEXTURE2D(_BodyMap);
            SAMPLER(sampler_BodyMap);
            TEXTURE2D(_HeadMap);
            SAMPLER(sampler_HeadMap);
            TEXTURE2D(_EquipMap1);
            SAMPLER(sampler_EquipMap1);
            TEXTURE2D(_EquipMap2);
            SAMPLER(sampler_EquipMap2);
            TEXTURE2D(_TailMap2);
            SAMPLER(sampler_TailMap2);

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

            half AssignedMapMask(float4 texelSize)
            {
                return (texelSize.z > 1.5 && texelSize.w > 1.5) ? 1.0 : 0.0;
            }

            half4 AlphaOver(half4 under, half4 over)
            {
                half a = saturate(over.a);
                half outA = a + under.a * (1.0 - a);
                half3 rgb = (over.rgb * a + under.rgb * under.a * (1.0 - a)) / max(outA, 0.0001);
                return half4(rgb, outA);
            }

            float SnapCell(float value, float maxIndex)
            {
                return clamp(round(value), 0.0, maxIndex);
            }

            void GetCellXY(out float cellX, out float cellY)
            {
                float n = max(_GridColumns, 1.0);
                float m = max(_GridRows, 1.0);

                float row = SnapCell(_Row, m - 1.0);
                float startCol = SnapCell(_StartColumn, n - 1.0);
                float endCol = SnapCell(_EndColumn, n - 1.0);

                float rangeStart = min(startCol, endCol);
                float rangeEnd = max(startCol, endCol);
                uint frameCount = max((uint)round(rangeEnd - rangeStart + 1.0), 1u);

                uint elapsed = (uint)max(floor(_Time.y / max(_Interval, 0.0001)), 0.0);
                uint frameOffset = (_Loop > 0.5)
                    ? (elapsed % frameCount)
                    : min(elapsed, frameCount - 1u);

                cellX = rangeStart + (float)frameOffset;
                cellY = row;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.shadowCoord = GetShadowCoord(positionInputs);
                output.uv = input.uv;

                // Top-down 2D character: always shade with world up normal.
                half3 normalWS = half3(0.0h, 1.0h, 0.0h);
                OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
                OUTPUT_SH(normalWS, output.vertexSH);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float n = max(_GridColumns, 1.0);
                float m = max(_GridRows, 1.0);

                float cellX;
                float cellY;
                GetCellXY(cellX, cellY);

                cellX = SnapCell(cellX, n - 1.0);
                cellY = SnapCell(cellY, m - 1.0);

                // m行n列，行0=最下，列0=最左；u格=1/n，v格=1/m
                float2 uvCell = float2(1.0 / n, 1.0 / m);
                float2 cellMin = float2(cellX, cellY) * uvCell;
                float2 atlasUv = cellMin + input.uv * uvCell;

                half4 color = half4(0, 0, 0, 0);

                half4 tail1 = SAMPLE_TEXTURE2D(_TailMap1, sampler_TailMap1, atlasUv);
                tail1.a *= AssignedMapMask(_TailMap1_TexelSize);
                color = AlphaOver(color, tail1);

                half4 body = SAMPLE_TEXTURE2D(_BodyMap, sampler_BodyMap, atlasUv) * _BodyColor;
                body.a *= AssignedMapMask(_BodyMap_TexelSize);
                color = AlphaOver(color, body);

                half4 head = SAMPLE_TEXTURE2D(_HeadMap, sampler_HeadMap, atlasUv);
                head.a *= AssignedMapMask(_HeadMap_TexelSize);
                color = AlphaOver(color, head);

                half4 equip1 = SAMPLE_TEXTURE2D(_EquipMap1, sampler_EquipMap1, atlasUv) * _EquipColor1;
                equip1.a *= AssignedMapMask(_EquipMap1_TexelSize);
                color = AlphaOver(color, equip1);

                half4 equip2 = SAMPLE_TEXTURE2D(_EquipMap2, sampler_EquipMap2, atlasUv) * _EquipColor2;
                equip2.a *= AssignedMapMask(_EquipMap2_TexelSize);
                color = AlphaOver(color, equip2);

                half4 tail2 = SAMPLE_TEXTURE2D(_TailMap2, sampler_TailMap2, atlasUv);
                tail2.a *= AssignedMapMask(_TailMap2_TexelSize);
                color = AlphaOver(color, tail2);

                half3 normalWS = half3(0.0h, 1.0h, 0.0h);
                Light mainLight = GetMainLight(input.shadowCoord);
                // Half Lambert wrap: keeps 2D sprites visible under side-facing directional lights.
                half ndotl = saturate(dot(normalWS, mainLight.direction) * 0.5h + 0.5h);
                half3 mainLightColor = mainLight.color
                    * mainLight.distanceAttenuation
                    * mainLight.shadowAttenuation
                    * ndotl;

                half3 bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, normalWS);
                MixRealtimeAndBakedGI(mainLight, normalWS, bakedGI);

                half3 lighting = bakedGI + mainLightColor;
                half lightLuma = max(max(lighting.r, lighting.g), lighting.b);
                // If GI/main light data is unavailable, keep original unlit brightness.
                color.rgb *= lerp(half3(1.0h, 1.0h, 1.0h), lighting, saturate(lightLuma * 1000.0h));
                color.rgb += GetFakeLight(input.positionWS, normalWS);

                #if defined(_SSCS_RECEIVE)
                    color.rgb = ApplySSCSCloudShadow(color.rgb, input.positionWS, normalWS);
                #endif

                color.rgb = ApplySRSceneShadow(
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
    }
}
