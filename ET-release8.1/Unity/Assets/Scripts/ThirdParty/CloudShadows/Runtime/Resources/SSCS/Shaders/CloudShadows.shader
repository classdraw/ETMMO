Shader "Hidden/Kronnect/SSCS/ScreenSpaceCloudShadows"
{
    Properties {
        _MainTex("Main Tex", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1)
        _CloudsTex("Clouds Tex", 2D) = "white" {}
        _Clouds2Tex("Clouds 2 Tex", 2D) = "white" {}
        _ShadowColor ("Color", Color) = (0,0,0)
        _LightColor ("Light Color", Color) = (1,1,1)
        _CloudsLightColor ("Clouds Light Color", Color) = (1,1,1)
        _CloudsDarkColor ("Clouds Dark Color", Color) = (0.6,0.6,0.6)
        _Offsets("Texture Offsets", Vector) = (0,0,0,0)
        _CloudsLayer1("Data", Vector) = (0.001, 4, 0, 0)
        _CloudsLayer2Extra("Data", Vector) = (0.001, 4, 0, 0)
        _CloudsLayer3Extra("Data", Vector) = (0.001, 4, 0, 0)
        _ShadowsExtraData("Shadows Data", Vector) = (1,0,0,0)
        _CoverageMask("Coverage Mask", 2D) = "white" {}
        _MaskWorldData("Coverage World Data", Vector) = (1,1,1,1)
        _BlendSrc("Blend Src", Int) = 0
        _BlendDst("Blend Dst", Int) = 3
    }

SubShader
{
    Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+1" "DisableBatching" = "True" "IgnoreProjector" = "True" }
    ZWrite Off ZTest Always Cull Off

  Pass { // 0
      PackageRequirements {
        "com.unity.render-pipelines.universal":""
      }
      Name "Cloud Shadows"
      Blend [_BlendSrc] [_BlendDst]
      HLSLPROGRAM
      #pragma vertex VertSimple
      #pragma fragment FragShadows
      #pragma target 3.0
      #pragma prefer_hlslcc gles
      #pragma multi_compile_fog
      #pragma multi_compile_local_fragment _ _ORTHOGRAPHIC
      #pragma multi_compile_local_fragment _ _SHADOWS_3D _SHADOWS_3D_HQ
      #pragma multi_compile_local_fragment _ _SHADOWS_COVERAGE_MASK _SHADOWS_COVERAGE_MASK_DEBUG
	    #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
      #pragma multi_compile_local_fragment _ _PRESERVE_DIRECTIONAL_SHADOWS
      #pragma multi_compile_local_fragment _ _BOUNDS
      #pragma multi_compile _ _SSCS_SHOW_SHADOWS
      #define USE_URP
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
      #include "CloudShadowsInput.hlsl"
      #include "CloudShadowsCommonURP.hlsl"
      #include "CloudShadowsPass.hlsl"
      ENDHLSL
  }
}

SubShader
{
    Tags { "RenderType" = "Transparent" "Queue" = "Transparent+1" "DisableBatching" = "True" "IgnoreProjector" = "True" }
    ZWrite Off ZTest Always Cull Off

  Pass { // 0
      Blend [_BlendSrc] [_BlendDst]
      Name "Cloud Shadows"
      HLSLPROGRAM
      #pragma vertex VertSimple
      #pragma fragment FragShadows
      #pragma target 3.0
      #pragma prefer_hlslcc gles
      #pragma multi_compile_fog
      #pragma multi_compile_local_fragment _ _ORTHOGRAPHIC
      #pragma multi_compile_local_fragment _ _SHADOWS_3D _SHADOWS_3D_HQ
      #pragma multi_compile_local_fragment _ _SHADOWS_COVERAGE_MASK _SHADOWS_COVERAGE_MASK_DEBUG
      #pragma multi_compile_local_fragment _ _BOUNDS
      #include "UnityCG.cginc"
      #include "CloudShadowsInput.hlsl"
      #include "CloudShadowsCommon.hlsl"
      #include "CloudShadowsPass.hlsl"
      ENDHLSL
  }
}

}


	