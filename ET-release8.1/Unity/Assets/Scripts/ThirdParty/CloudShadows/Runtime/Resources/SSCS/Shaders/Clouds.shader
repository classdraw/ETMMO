Shader "Hidden/Kronnect/SSCS/ScreenSpaceClouds"
{
    Properties {
        _MainTex("Main Tex", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1)
        _CloudsTex("Clouds Tex", 2D) = "white" {}
        _Clouds2Tex("Clouds 2 Tex", 2D) = "white" {}
        _CloudsBumpMap("Clouds BumpMap", 2D) = "bump" {}
        _ShadowColor ("Color", Color) = (0,0,0)
        _LightColor ("Light Color", Color) = (1,1,1)
        _CloudsLightColor ("Clouds Light Color", Color) = (1,1,1)
        _CloudsDarkColor ("Clouds Dark Color", Color) = (0.6,0.6,0.6)
        _Offsets("Texture Offsets", Vector) = (0,0,0,0)
        _CloudsLayer1("Data", Vector) = (0.001, 4, 0, 0)
        _CloudsLayer2Extra("Data", Vector) = (0.001, 4, 0, 0)
        _CloudsLayer3Extra("Data", Vector) = (0.001, 4, 0, 0)
        _ShadowsExtraData("Shadows Data", Vector) = (1,0,0,0)
    }

SubShader
{
    Tags { "RenderType" = "Transparent" "Queue" = "Transparent+2" "DisableBatching" = "True" "IgnoreProjector" = "True" }
    ZWrite Off ZTest Always Cull Off

    HLSLINCLUDE
    #pragma target 3.0
    #pragma prefer_hlslcc gles
    #include "UnityCG.cginc"
    #include "CloudShadowsInput.hlsl"
    #include "CloudShadowsCommon.hlsl"
    ENDHLSL

  Pass { // 0
      Blend SrcAlpha OneMinusSrcAlpha
      Name "Clouds"
      HLSLPROGRAM
      #pragma vertex VertSimple
      #pragma fragment FragClouds
      #pragma multi_compile_local_fragment _ _ORTHOGRAPHIC
      #pragma multi_compile_local_fragment _ _ORTHOGRAPHIC_OVERLAY
      #pragma multi_compile_local_fragment _ _CLOUDS_THICK
      #pragma multi_compile_local_fragment _ _CLOUDS_ANISOTROPY
      #pragma multi_compile_local_fragment _ _BOUNDS
      #include "CloudShadowsPass.hlsl"
      ENDHLSL
  }
}
}

