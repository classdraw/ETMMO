#ifndef SR_CHARACTER_COMMON_INCLUDED
#define SR_CHARACTER_COMMON_INCLUDED

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
    float _AnimStartTime;
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

half SR_CharacterAssignedMapMask(float4 texelSize)
{
    return (texelSize.z > 1.5 && texelSize.w > 1.5) ? 1.0 : 0.0;
}

half4 SR_CharacterAlphaOver(half4 under, half4 over)
{
    half a = saturate(over.a);
    half outA = a + under.a * (1.0 - a);
    half3 rgb = (over.rgb * a + under.rgb * under.a * (1.0 - a)) / max(outA, 0.0001);
    return half4(rgb, outA);
}

float SR_CharacterSnapCell(float value, float maxIndex)
{
    return clamp(round(value), 0.0, maxIndex);
}

void SR_CharacterGetCellXY(out float cellX, out float cellY)
{
    float n = max(_GridColumns, 1.0);
    float m = max(_GridRows, 1.0);

    float row = SR_CharacterSnapCell(_Row, m - 1.0);
    float startCol = SR_CharacterSnapCell(_StartColumn, n - 1.0);
    float endCol = SR_CharacterSnapCell(_EndColumn, n - 1.0);

    float rangeStart = min(startCol, endCol);
    float rangeEnd = max(startCol, endCol);
    uint frameCount = max((uint)round(rangeEnd - rangeStart + 1.0), 1u);

    float animTime = max(_Time.y - _AnimStartTime, 0.0);
    uint elapsed = (uint)max(floor(animTime / max(_Interval, 0.0001)), 0.0);
    uint frameOffset = (_Loop > 0.5)
        ? (elapsed % frameCount)
        : min(elapsed, frameCount - 1u);

    cellX = rangeStart + (float)frameOffset;
    cellY = row;
}

float2 SR_CharacterAtlasUV(float2 meshUv)
{
    float n = max(_GridColumns, 1.0);
    float m = max(_GridRows, 1.0);

    float cellX;
    float cellY;
    SR_CharacterGetCellXY(cellX, cellY);

    cellX = SR_CharacterSnapCell(cellX, n - 1.0);
    cellY = SR_CharacterSnapCell(cellY, m - 1.0);

    float2 uvCell = float2(1.0 / n, 1.0 / m);
    float2 cellMin = float2(cellX, cellY) * uvCell;
    return cellMin + meshUv * uvCell;
}

half4 SR_CharacterSampleComposite(float2 atlasUv)
{
    half4 color = half4(0, 0, 0, 0);

    half4 tail1 = SAMPLE_TEXTURE2D(_TailMap1, sampler_TailMap1, atlasUv);
    tail1.a *= SR_CharacterAssignedMapMask(_TailMap1_TexelSize);
    color = SR_CharacterAlphaOver(color, tail1);

    half4 body = SAMPLE_TEXTURE2D(_BodyMap, sampler_BodyMap, atlasUv) * _BodyColor;
    body.a *= SR_CharacterAssignedMapMask(_BodyMap_TexelSize);
    color = SR_CharacterAlphaOver(color, body);

    half4 head = SAMPLE_TEXTURE2D(_HeadMap, sampler_HeadMap, atlasUv);
    head.a *= SR_CharacterAssignedMapMask(_HeadMap_TexelSize);
    color = SR_CharacterAlphaOver(color, head);

    half4 equip1 = SAMPLE_TEXTURE2D(_EquipMap1, sampler_EquipMap1, atlasUv) * _EquipColor1;
    equip1.a *= SR_CharacterAssignedMapMask(_EquipMap1_TexelSize);
    color = SR_CharacterAlphaOver(color, equip1);

    half4 equip2 = SAMPLE_TEXTURE2D(_EquipMap2, sampler_EquipMap2, atlasUv) * _EquipColor2;
    equip2.a *= SR_CharacterAssignedMapMask(_EquipMap2_TexelSize);
    color = SR_CharacterAlphaOver(color, equip2);

    half4 tail2 = SAMPLE_TEXTURE2D(_TailMap2, sampler_TailMap2, atlasUv);
    tail2.a *= SR_CharacterAssignedMapMask(_TailMap2_TexelSize);
    color = SR_CharacterAlphaOver(color, tail2);

    return color;
}

#endif
