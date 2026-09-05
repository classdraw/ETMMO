#ifndef SSCS_INPUT
#define SSCS_INPUT

    float3 _SSCS_LightDir;
    #define LIGHT_DIR _SSCS_LightDir

    half3 _LightColor;
    half3 _ShadowColor;
    half3 _CloudsLightColor, _CloudsDarkColor;

    sampler2D _CloudsTex;
    float4 _CloudsLayer1;
    #define LAYER1_SCALE _CloudsLayer1.x
    #define LAYER1_EDGE_NOISE _CloudsLayer1.y
    #define LAYER1_ANIMATION_SPEED _CloudsLayer1.z
    #define SHADOW_OPACITY _CloudsLayer1.w

    float4 _SSCS_CloudsLayer1Extra;
    #define LAYER1_WIND_OFFSET _SSCS_CloudsLayer1Extra.xz
    #define LAYER1_COVERAGE _SSCS_CloudsLayer1Extra.y
    #define LAYER1_CONTRAST _SSCS_CloudsLayer1Extra.w

    float4 _Offsets;
    #define LAYER1_OFFSET _Offsets.xz
    #define LAYER1_CLOUDS_ALTITUDE _Offsets.y
    #define NOISE_MULTIPLIER _Offsets.w

    float4 _CloudsLayer2Extra;
    #define AMBIENT_LIGHT _CloudsLayer2Extra.x
    #define CLOUDS_OPACITY _CloudsLayer2Extra.y
    #define CLOUDS_DISTANCE_FADE _CloudsLayer2Extra.z
    #define CLOUD_THICKNESS _CloudsLayer2Extra.w

    float4 _CloudsLayer3Extra;
    #define CLOUDS_LIGHT_INTENSITY _CloudsLayer3Extra.x
    #define CLOUDS_ANISOTROPY _CloudsLayer3Extra.y
    #define NORMAL_BIAS _CloudsLayer3Extra.z
    #define BUMPMAP_SCALE _CloudsLayer3Extra.w

    float3 _ShadowsExtraData;
    #define SHADOWS_COVERAGE _ShadowsExtraData.x
    #define SHADOWS_MIN_ALTITUDE _ShadowsExtraData.y
    #define SHADOWS_PRESERVE_DIRECTIONAL _ShadowsExtraData.z

    sampler2D _Clouds2Tex;
    sampler2D _CloudsBumpMap;

    float4 _MaskWorldData;
    sampler2D _CoverageMask;

    float4 _BoundsData;
    #define BOUNDS_SIZE _BoundsData.zw
    #define BOUNDS_CENTER _BoundsData.xy

#endif // SSCS_INPUT