#ifndef SB_PROPIXELIZER_OUTLINE_PASS_INCLUDED
#define SB_PROPIXELIZER_OUTLINE_PASS_INCLUDED

#include "SB_ProPixelizerUberInput.hlsl"

SAMPLER(sampler_BaseMap_point_repeat);

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv0 : TEXCOORD0;
    float3 normalOS : NORMAL;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 texCoord0 : TEXCOORD0;
    float3 normalVS : NORMAL;
};

Varyings outline_vert(Attributes input)
{
    Varyings output = (Varyings)0;
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = float4(vertexInput.positionCS);
    output.normalVS = TransformWorldToViewDir(TransformObjectToWorldNormal(input.normalOS));
    output.texCoord0 = TRANSFORM_TEX(input.uv0, _BaseMap);
    return output;
}

void outline_frag(Varyings i, out float4 color : COLOR)
{
    float alpha_out;
    float2 dither_uv;
    float4 screenParams;
    half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap_point_repeat, i.texCoord0);
    float alpha = SB_ResolveOpaqueAlpha(baseSample);
    GetScaledScreenParameters_float(screenParams);
    float4 pixelPos = float4(i.positionCS.xy, 0.0, 0.0);
#if defined(USE_OBJECT_POSITION_FOR_PIXEL_GRID_ON)
    float3 objectPixelPos = UNITY_MATRIX_M._m03_m13_m23;
    PixelClipAlpha_float(UNITY_MATRIX_VP, objectPixelPos, screenParams, pixelPos, _PixelSize, alpha, _AlphaClipThreshold, alpha_out, dither_uv);
#else
    PixelClipAlpha_float(UNITY_MATRIX_VP, _PixelGridOrigin.xyz, screenParams, pixelPos, _PixelSize, alpha, _AlphaClipThreshold, alpha_out, dither_uv);
#endif
    clip(alpha_out - 0.1);
    PackMetadata(_ID, max(1, round(_PixelSize * _ProPixelizer_Pixel_Scale)), i.normalVS, color);
}

#endif
