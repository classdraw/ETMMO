Shader "Custom/SR_Character"
{
    Properties
    {
        [Header(Layers Bottom To Top)]
        [MainTexture] _BodyMap("Body Texture", 2D) = "white" {}
        [MainColor] _BodyColor("Body Tint", Color) = (1, 1, 1, 1)
        _HeadMap("Head Texture", 2D) = "white" {}
        _EquipMap1("Equip Texture 1", 2D) = "white" {}
        _EquipColor1("Equip Tint 1", Color) = (1, 1, 1, 1)
        _EquipMap2("Equip Texture 2", 2D) = "white" {}
        _EquipColor2("Equip Tint 2", Color) = (1, 1, 1, 1)
        _TailMap("Tail Texture", 2D) = "white" {}
        _OcclusionFade("Occlusion Fade", Range(0, 1)) = 1

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
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BodyMap_ST;
                float4 _BodyMap_TexelSize;
                float4 _HeadMap_TexelSize;
                float4 _EquipMap1_TexelSize;
                float4 _EquipMap2_TexelSize;
                float4 _TailMap_TexelSize;
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
            CBUFFER_END

            TEXTURE2D(_BodyMap);
            SAMPLER(sampler_BodyMap);
            TEXTURE2D(_HeadMap);
            SAMPLER(sampler_HeadMap);
            TEXTURE2D(_EquipMap1);
            SAMPLER(sampler_EquipMap1);
            TEXTURE2D(_EquipMap2);
            SAMPLER(sampler_EquipMap2);
            TEXTURE2D(_TailMap);
            SAMPLER(sampler_TailMap);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

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
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
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

                half4 color = SAMPLE_TEXTURE2D(_BodyMap, sampler_BodyMap, atlasUv) * _BodyColor;

                half4 head = SAMPLE_TEXTURE2D(_HeadMap, sampler_HeadMap, atlasUv);
                head.a *= AssignedMapMask(_HeadMap_TexelSize);
                color = AlphaOver(color, head);

                half4 equip1 = SAMPLE_TEXTURE2D(_EquipMap1, sampler_EquipMap1, atlasUv) * _EquipColor1;
                equip1.a *= AssignedMapMask(_EquipMap1_TexelSize);
                color = AlphaOver(color, equip1);

                half4 equip2 = SAMPLE_TEXTURE2D(_EquipMap2, sampler_EquipMap2, atlasUv) * _EquipColor2;
                equip2.a *= AssignedMapMask(_EquipMap2_TexelSize);
                color = AlphaOver(color, equip2);

                half4 tail = SAMPLE_TEXTURE2D(_TailMap, sampler_TailMap, atlasUv);
                tail.a *= AssignedMapMask(_TailMap_TexelSize);
                color = AlphaOver(color, tail);

                return half4(color.rgb, color.a * _OcclusionFade);
            }
            ENDHLSL
        }
    }
}
