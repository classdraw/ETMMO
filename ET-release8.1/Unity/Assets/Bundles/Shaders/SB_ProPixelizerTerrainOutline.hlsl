#ifndef SB_PROPIXELIZER_TERRAIN_OUTLINE_INCLUDED
#define SB_PROPIXELIZER_TERRAIN_OUTLINE_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
#include "SB_ProPixelizerTerrainInput.hlsl"
#include "../../Art/ProPixelizer/SRP/ShaderLibrary/PixelUtils.hlsl"
#include "../../Art/ProPixelizer/SRP/ShaderLibrary/PackingUtils.hlsl"
#include "../../Art/ProPixelizer/SRP/ShaderLibrary/ScreenUtils.hlsl"

struct OutlineAttributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct OutlineVaryings
{
    float4 positionCS : SV_POSITION;
    float3 normalVS : TEXCOORD0;
};

OutlineVaryings outline_vert(OutlineAttributes input)
{
    OutlineVaryings output = (OutlineVaryings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    TerrainInstancing(input.positionOS, input.normalOS, input.texcoord);
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.positionCS = vertexInput.positionCS;
    output.normalVS = TransformWorldToViewDir(TransformObjectToWorldNormal(input.normalOS));
    return output;
}

void outline_frag(OutlineVaryings i, out float4 color : COLOR)
{
    float alphaOut;
    float2 ditherUV;
    float4 screenParams;
    GetScaledScreenParameters_float(screenParams);
    float4 pixelPos = float4(i.positionCS.xy, 0.0, 0.0);
#if defined(USE_OBJECT_POSITION_FOR_PIXEL_GRID_ON)
    float3 objectPixelPos = UNITY_MATRIX_M._m03_m13_m23;
    PixelClipAlpha_float(UNITY_MATRIX_VP, objectPixelPos, screenParams, pixelPos, _PixelSize, 1.0, _AlphaClipThreshold, alphaOut, ditherUV);
#else
    PixelClipAlpha_float(UNITY_MATRIX_VP, _PixelGridOrigin.xyz, screenParams, pixelPos, _PixelSize, 1.0, _AlphaClipThreshold, alphaOut, ditherUV);
#endif
    clip(alphaOut - 0.1);
    PackMetadata(_ID, max(1, round(_PixelSize * _ProPixelizer_Pixel_Scale)), i.normalVS, color);
}

#endif
