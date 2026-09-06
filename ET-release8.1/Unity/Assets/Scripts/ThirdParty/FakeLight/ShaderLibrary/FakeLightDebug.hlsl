#ifndef FAKE_LIGTH_DEBUG_INCLUDED
#define FAKE_LIGTH_DEBUG_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Debug.hlsl"

sampler2D _DebugFakeLightTexture;

float2 _TileSize;
float _TileLightCount[100];

#define TilesX 10

half4 GetFakeLightDebugColor(float3 worldPos, uint2 pixCoord)
{
    float2 positionOffset = worldPos.xz - _FakeLightRegionStart.xz;
    float2 tile = positionOffset / _TileSize;
    int2 tileID = int2(tile);
    float regionMask = step(0 ,tile.x) * step(tile.x,TilesX) * step(0 ,tile.y) * step(tile.y,TilesX);
    int tileIndex = tileID.y * TilesX + tileID.x;
    int lightCount = _TileLightCount[tileIndex];

    float opacity = 0.8f;
    half4 overlay = 0;
    #if defined(FAKELIGHT_TILE_DEBUG)
        overlay = half4(OverlayHeatMap(pixCoord, 32, lightCount, 8, opacity));
    #elif defined(FAKELIGHT_PIXEL_DEBUG)
        float2 uv = positionOffset * _FakeLightTexUVRate;
        lightCount = (int)(tex2D(_DebugFakeLightTexture, uv).r * 32.0 + 0.5h);
        overlay = half4(OverlayHeatMap(pixCoord, 32, lightCount, 5, opacity));
    #endif
    half3 debugCol = overlay.rgb;
    
    return half4(debugCol, 1) * regionMask;
}

#endif