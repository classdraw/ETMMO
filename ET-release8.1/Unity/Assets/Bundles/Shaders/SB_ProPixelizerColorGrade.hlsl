#ifndef SB_PROPIXELIZER_COLOR_GRADE_INCLUDED
#define SB_PROPIXELIZER_COLOR_GRADE_INCLUDED

// 从 ProPixelizer ColorGrading.hlsl 摘出的纯 HLSL 路径，避免 UnityTexture2D（ShaderGraph 专用）。
#define SB_PP_MAXCOLOR 16.0
#define SB_PP_RES 16.0
#define SB_PP_DITHER_SIZE 16.0

inline float SB_GetDitherPaletteOffset(float2 ditherUV)
{
    return (((uint(frac(ditherUV.x / 4) * 4)) * 4 + uint(frac(ditherUV.y / 4) * 4)) / SB_PP_DITHER_SIZE);
}

inline float SB_MapToCell(float input, float size)
{
    return (clamp(input * SB_PP_RES, 0, SB_PP_RES - 1) + 0.5) / (SB_PP_RES * size);
}

inline void SB_InternalColorGrade(TEXTURE2D(paletteTex), SAMPLER(samplerPalette), float4 orig, float2 ditherUV, out float4 graded)
{
#ifndef UNITY_COLORSPACE_GAMMA
    orig = pow(orig, 0.454545);
#endif
    orig = clamp(orig, 0.0, 0.9999);

    float u = SB_MapToCell(orig.r, SB_PP_RES);
    float v = SB_MapToCell(orig.g, SB_PP_DITHER_SIZE);
    float cell = round(clamp(orig.b * SB_PP_MAXCOLOR, 0.0, SB_PP_RES - 1.0));
    u = cell / SB_PP_RES + u;
    v = v + SB_GetDitherPaletteOffset(ditherUV);

    graded = SAMPLE_TEXTURE2D(paletteTex, samplerPalette, float2(u, v));
}

#endif
