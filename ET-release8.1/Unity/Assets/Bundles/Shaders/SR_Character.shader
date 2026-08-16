Shader "Custom/SR_Character"
{
    Properties
    {
        [MainTexture] _BodyMap("Body Texture", 2D) = "white" {}
        [MainColor] _BodyColor("Body Tint", Color) = (1, 1, 1, 1)
        _OcclusionFade("Occlusion Fade", Range(0, 1)) = 1

        [Header(Grid)]
        _GridRows("Grid Rows", Float) = 1
        _GridColumns("Grid Columns", Float) = 1

        [Header(Playback Range)]
        _StartRow("Start Row", Float) = 0
        _StartColumn("Start Column", Float) = 0
        _EndRow("End Row", Float) = 0
        _EndColumn("End Column", Float) = 0

        [Header(Animation)]
        [Toggle] _Loop("Loop", Float) = 1
        _Interval("Interval (Seconds)", Float) = 0.1

        [Header(Ignore Frame)]
        _IgnoreRow("Ignore Row", Float) = -1
        _IgnoreColumn("Ignore Column", Float) = -1
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
                half4 _BodyColor;
                float _OcclusionFade;
                half _GridRows;
                half _GridColumns;
                half _StartRow;
                half _StartColumn;
                half _EndRow;
                half _EndColumn;
                half _Loop;
                half _Interval;
                half _IgnoreRow;
                half _IgnoreColumn;
            CBUFFER_END

            TEXTURE2D(_BodyMap);   SAMPLER(sampler_BodyMap);

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            // UV(0,0) 对应第 0 行、第 0 列（左下角）
            float2 RemapUVToCell(float2 uv, half gridRows, half gridColumns, half cellRow, half cellColumn, float2 texelSize)
            {
                half rows = max(gridRows, 1.0h);
                half columns = max(gridColumns, 1.0h);
                half2 cellSize = half2(1.0h / columns, 1.0h / rows);
                half row = clamp(cellRow, 0.0h, rows - 1.0h);
                half column = clamp(cellColumn, 0.0h, columns - 1.0h);
                half2 cellOffset = half2(column, row) * cellSize;

                // 内缩半像素，避免双线性过滤采样到相邻格子
                half2 inset = half2(texelSize.x, texelSize.y) * 0.5h;
                half2 sampleSize = max(cellSize - inset * 2.0h, half2(0.0h, 0.0h));
                return cellOffset + inset + uv * sampleSize;
            }

            void GetCurrentCell(out half cellRow, out half cellColumn)
            {
                half rows = max(_GridRows, 1.0h);
                half columns = max(_GridColumns, 1.0h);

                half startIndex = clamp(_StartRow, 0.0h, rows - 1.0h) * columns
                    + clamp(_StartColumn, 0.0h, columns - 1.0h);
                half endIndex = clamp(_EndRow, 0.0h, rows - 1.0h) * columns
                    + clamp(_EndColumn, 0.0h, columns - 1.0h);

                half rangeStart = min(startIndex, endIndex);
                half rangeEnd = max(startIndex, endIndex);
                half frameCount = rangeEnd - rangeStart + 1.0h;

                bool hasIgnore = _IgnoreRow >= 0.0h && _IgnoreColumn >= 0.0h;
                half ignoreIndex = clamp(_IgnoreRow, 0.0h, rows - 1.0h) * columns
                    + clamp(_IgnoreColumn, 0.0h, columns - 1.0h);
                bool shouldIgnore = hasIgnore && ignoreIndex >= rangeStart && ignoreIndex <= rangeEnd;

                half effectiveFrameCount = max(frameCount - (shouldIgnore ? 1.0h : 0.0h), 1.0h);

                half interval = max(_Interval, 0.0001h);
                half elapsedFrames = floor(_Time.y / interval);

                half frameOffset;
                if (_Loop > 0.5h)
                {
                    frameOffset = fmod(elapsedFrames, effectiveFrameCount);
                    if (frameOffset < 0.0h)
                    {
                        frameOffset += effectiveFrameCount;
                    }
                }
                else
                {
                    frameOffset = min(elapsedFrames, effectiveFrameCount - 1.0h);
                }

                half currentIndex = rangeStart + frameOffset;
                if (shouldIgnore && currentIndex >= ignoreIndex)
                {
                    currentIndex += 1.0h;
                }
                cellRow = floor(currentIndex / columns);
                cellColumn = fmod(currentIndex, columns);
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
                half cellRow;
                half cellColumn;
                GetCurrentCell(cellRow, cellColumn);

                float2 bodyUv = TRANSFORM_TEX(input.uv, _BodyMap);
                float2 bodyAtlasUv = RemapUVToCell(bodyUv, _GridRows, _GridColumns, cellRow, cellColumn, _BodyMap_TexelSize.xy);
                half4 body = SAMPLE_TEXTURE2D(_BodyMap, sampler_BodyMap, bodyAtlasUv) * _BodyColor;

                return half4(body.rgb, body.a * _OcclusionFade);
            }
            ENDHLSL
        }
    }
}
